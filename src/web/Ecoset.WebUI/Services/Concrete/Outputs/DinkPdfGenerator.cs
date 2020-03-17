using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using DinkToPdf;
using DinkToPdf.Contracts;
using Ecoset.WebUI.Models;
using Ecoset.WebUI.Options;
using Ecoset.WebUI.Services.Abstract;
using Microsoft.Extensions.Logging;

namespace Ecoset.WebUI.Services.Concrete
{
    public class DinkReportGenerator : IReportGenerator
    {
        private IOutputPersistence _persistence;
        private IHttpContextAccessor _context;
        private SynchronizedConverter _converter;
        private EcosetAppOptions _options;
        private ILogger<DinkReportGenerator> _logger;

        public DinkReportGenerator(IOutputPersistence persistence, 
            IHttpContextAccessor httpContextAccessor, 
            IOptions<EcosetAppOptions> options,
            ILogger<DinkReportGenerator> logger)
        {
            _persistence = persistence;
            _context = httpContextAccessor;
            _converter = new SynchronizedConverter(new PdfTools());
            _options = options.Value;
            _logger = logger;
            _converter.PhaseChanged += LogPhaseChanged;
            _converter.ProgressChanged += LogProgressChanged;
            _converter.Warning += LogWarning;
            _converter.Error += LogError;
        }

        private void LogPhaseChanged(object sender, DinkToPdf.EventDefinitions.PhaseChangedArgs e) {
            _logger.LogInformation(1, "Phase change: " + e.CurrentPhase + " - " + e.Description);
        }

        private void LogProgressChanged(object sender, DinkToPdf.EventDefinitions.ProgressChangedArgs e) {
            _logger.LogInformation(1, "PDF Generation: " + e.Description);
        }

        private void LogWarning(object sender, DinkToPdf.EventDefinitions.WarningArgs e) {
            _logger.LogWarning(1, "PDF Generation: " + e.Message);
        }

        private void LogError(object sender, DinkToPdf.EventDefinitions.ErrorArgs e) {
            _logger.LogError(1, "PDF Generation: " + e.Message);
        }

        public string GenerateReport(Job job)
        {
            var fileName = _persistence.GetReport(job.Id);
            if (string.IsNullOrEmpty(fileName)) 
            {
                var scratchFile = System.IO.Path.Combine(_options.ScratchDirectory, Guid.NewGuid() + ".pdf");
                Console.WriteLine("Creating scratch PDF at " + scratchFile);
                var doc = new HtmlToPdfDocument()
                {
                    GlobalSettings = {
                        ColorMode = ColorMode.Color,
                        Orientation = Orientation.Landscape,
                        PaperSize = PaperKind.A4,
                        Margins = new MarginSettings() { Top = 10, Bottom = 10, Left = 5, Right = 5 },
                        Out = scratchFile
                    },
                    Objects = {
                        new ObjectSettings()
                        {
                            Page = "http://localhost:" + _options.Port + "/Analysis/Home/GenerateReport?id=" + job.Id,
                            PagesCount = true,
                            FooterSettings = {FontName = "Arial", FontSize = 9, Right = "Page [sitepage] of [sitepages]"},
                            HeaderSettings = { FontName = "Arial", FontSize = 9, Left = _options.InstanceName },
                            WebSettings = {
                                PrintMediaType = true
                            },
                            LoadSettings = {
                                JSDelay = 60000 * 3,
                                StopSlowScript = false,
                                DebugJavascript = true
                            }
                        },
                    }
                };
                _converter.Convert(doc);
                fileName = _persistence.PersistReport(job.Id, scratchFile);
            }
            return fileName;
        }
    }
}
