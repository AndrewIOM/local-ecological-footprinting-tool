using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ecoset.WebUI.Options;
using Ecoset.WebUI.Services.Abstract;
using Ecoset.GeoTemporal.Remote;
using System.Linq;
using Ecoset.WebUI.Models;

namespace Ecoset.WebUI.Services.Concrete
{
    public class EcosetDataRegistry : IDataRegistry
    {
        private IGeoSpatialConnection _connection;
        private IOptions<ReportContentOptions> _options;
        private readonly ILogger<EcoSetJobProcessor> _logger;
        private List<Models.Variable> _availableDatasets;
        private DateTime? _lastUpdated;
        private readonly TimeSpan _refreshInterval;

        public EcosetDataRegistry(IGeoSpatialConnection connection, IOptions<ReportContentOptions> options, ILogger<EcoSetJobProcessor> logger) {
            _connection = connection;
            _logger = logger;
            _options = options;
            _availableDatasets = new List<Models.Variable>();
            _refreshInterval = TimeSpan.FromMinutes(1);
        }

        public async Task<List<Models.Variable>> GetAvailableVariables()
        {
            if (_lastUpdated.HasValue) {
                if (DateTime.Now - _lastUpdated.Value > _refreshInterval)
                await RefreshAvailableDatasets();
            } else {
                await RefreshAvailableDatasets();
            }            
            return _availableDatasets;
        }

        /// If a name * method combination is ambiguous, will return false.
        public async Task<AvailableVariable> IsAvailable(string variableName, string variableMethod)
        {
            if (!_lastUpdated.HasValue) await RefreshAvailableDatasets();
            var available = _availableDatasets.FirstOrDefault(d => d.Id == variableName);
            if (available == null) return null;
            if (available.Methods == null) return null;
            if (string.IsNullOrEmpty(variableMethod)) {
                if (available.Methods.Count == 1) {
                    return new AvailableVariable() {
                        Id = available.Id,
                        MethodId = available.Methods.First().Id,
                        DescriptiveName = available.Name,
                        Description = available.Description,
                        Unit = available.Unit,
                        License = available.Methods.First().License,
                        LicenseUrl = available.Methods.First().LicenseUrl
                    };
                }
                return null;
            } else {
                var methodMatch = available.Methods.FirstOrDefault(m => m.Id == variableMethod);
                if (methodMatch == null) return null;
                return new AvailableVariable() {
                    Id = available.Id,
                    MethodId = available.Methods.First().Id,
                    DescriptiveName = available.Name,
                    Description = available.Description,
                    Unit = available.Unit,
                    License = available.Methods.First().License,
                    LicenseUrl = available.Methods.First().LicenseUrl
                };
            }
        }

        private async Task RefreshAvailableDatasets() {
            var available = await _connection.ListAvailableDatasets();
            var refreshed = available.Select(d => {
                return new Models.Variable() {
                    Id = d.Id,
                    Name = d.FriendlyName,
                    Description = d.Description,
                    Unit = d.Unit,
                    Methods = d.Methods.Select(m => {
                        return new Models.VariableMethod() {
                            Id = m.Id,
                            Name = m.Name,
                            License = m.License,
                            LicenseUrl = m.LicenseUrl,
                            TimesAvailable = new Models.MethodTime() {
                                ExtentMax = dateDto(m.TemporalExtent.MaxDate),
                                ExtentMin = dateDto(m.TemporalExtent.MinDate),
                                TimeSlices = m.TemporalExtent.Slices.Select(d => dateDto(d)).ToList()
                            }
                        };
                    }).ToList()
                };
            }).ToList();
            if (_options.Value != null) {
                refreshed = refreshed.Where(v => 
                    _options.Value.ProReportSections.FirstOrDefault(p => p.Name == v.Id) != null).ToList();
            }
            _availableDatasets = refreshed;
            _lastUpdated = DateTime.Now;
        }

        private Models.SimpleDate dateDto(Date eDate) {
            if (eDate == null) return null;
            if (eDate.Year.HasValue) {
                return new Models.SimpleDate() {
                    Day = eDate.Day,
                    Month = eDate.Month,
                    Year = eDate.Year.Value
                };
            }
            return null;
        }

    }
}