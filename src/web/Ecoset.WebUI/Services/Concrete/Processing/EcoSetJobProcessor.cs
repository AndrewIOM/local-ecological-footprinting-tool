using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ecoset.WebUI.Models;
using Ecoset.WebUI.Models.JobProcessor;
using Ecoset.WebUI.Options;
using Ecoset.WebUI.Services.Abstract;
using Ecoset.GeoTemporal.Remote;

namespace Ecoset.WebUI.Services.Concrete
{
    public class EcoSetJobProcessor : IJobProcessor
    {
        private IGeoSpatialConnection _connection;
        private IOptions<ReportContentOptions> _options;
        private readonly ILogger<EcoSetJobProcessor> _logger;

        public EcoSetJobProcessor(IGeoSpatialConnection connection, IOptions<ReportContentOptions> options, ILogger<EcoSetJobProcessor> logger) {
            _connection = connection;
            _logger = logger;
            _options = options;
        }

        public async Task<ReportData> GetReportData(string processorJobId)
        {
            Guid guidId;
            var success = Guid.TryParse(processorJobId, out guidId);
            if (!success) throw new Exception("ID was not valid");
            var jobId = new JobId(guidId);
            var fetchData = await _connection.FetchResultAsync(jobId);

            var reportData = new ReportData();
            reportData.North = fetchData.North;
            reportData.South = fetchData.South;
            reportData.East = fetchData.East;
            reportData.West = fetchData.West;
            reportData.TableListResults = new List<TableList>();
            reportData.TableStatsResults = new List<TableStats>();
            reportData.RawResults = new List<RawData>();

            foreach (var output in fetchData.Outputs)
            {
                _logger.LogInformation("Retrieved output from " + output.Name + " / " + output.MethodUsed);

                if (output.Data is RawDataResult rdr) 
                {
                    reportData.RawResults.Add(new RawData() {
                        Name = output.Name,
                        Data = rdr
                    });
                } else if (output.Data is DataTableListResult tbl) {
                    reportData.TableListResults.Add(new TableList() {
                        Name = output.Name,
                        Data = tbl
                    });
                } else if (output.Data is DataTableStatsResult stbl) {
                    reportData.TableStatsResults.Add(new TableStats() {
                        Name = output.Name,
                        Data = stbl
                    });
                }
                // Others are unknown and discarded
            }

            return reportData;
        }

        public async Task<Models.JobStatus> GetStatus(string processorJobId, Models.JobStatus localStatus)
        {
            Guid guidId;
            var success = Guid.TryParse(processorJobId, out guidId);
            if (!success) {
                _logger.LogCritical("Cannot retrieve job status for job: " + processorJobId);
                return localStatus;
            }
            var jobId = new JobId(guidId);
            var status = await _connection.GetJobStatusAsync(jobId);
            return ToLeftStatus(status);
        }

        public async Task<string> StartDataPackage(JobSubmission job, RequestedTime dateMode, int? year, int? month, int? day, List<string> variables)
        {
            var time = new TimeMode();
            if (dateMode == RequestedTime.Before) {
                time.Kind = "before";
                time.Date = new Date() { Year = year.Value, Month = month, Day = day };
            } else if (dateMode == RequestedTime.Exact) {
                time.Kind = "exact";
                time.Date = new Date() { Year = year.Value, Month = month, Day = day };
            } else {
                time.Kind = "latest";
            }

            var command = new JobSubmissionRequest()
            {
                East = job.East,
                West = job.West,
                North = job.North,
                South = job.South,
                Variables = GetCustomRequest(variables),
                TimeMode = time
            };
            try {
                var result = await _connection.SubmitJobAsync(command);
                return result.Id.ToString();
            } catch (Exception e)
            {
                _logger.LogCritical("Job could not be submitted to the geotemporal engine. The error was: " + e.Message);
                return null;
            }
        }

        public async Task<string> StartJob(JobSubmission job)
        {
            var command = new JobSubmissionRequest()
            {
                East = job.East,
                West = job.West,
                North = job.North,
                South = job.South,
                Variables = GetReportRequest(false),
                TimeMode = new TimeMode { Kind = "latest", Date = null }
            };

            try {
                var result = await _connection.SubmitJobAsync(command);
                return result.Id.ToString();
            } catch (Exception e)
            {
                _logger.LogCritical("Job could not be submitted to the geotemporal engine. The error was: " + e.Message);
                return null;
            }
        }

        public async Task<string> StartProJob(JobSubmission job)
        {
            var command = new JobSubmissionRequest()
            {
                East = job.East,
                West = job.West,
                North = job.North,
                South = job.South,
                Variables = GetReportRequest(true),
                TimeMode = new TimeMode { Kind = "latest", Date = null }
            };

            try {
                var result = await _connection.SubmitJobAsync(command);
                return result.Id.ToString();
            } catch (Exception)
            {
                _logger.LogCritical("Job could not be submitted to EcoSet.");
                return "";
            }
        }

        public void StopJob(string processorJobId)
        {
            throw new NotImplementedException();
        }

        private List<Ecoset.GeoTemporal.Remote.Variable> GetReportRequest(bool isPro) 
        {
            var exesList = _options.Value.ProReportSections;
            if(!isPro) {
                exesList = _options.Value.FreeReportSections.Select(m => {
                    if (!m.Options.ContainsKey("maxresolution") && _options.Value.MaxMapPixelSize.HasValue) m.Options.Add("maxresolution",_options.Value.MaxMapPixelSize.ToString());
                    if (!m.Options.ContainsKey("resolution") && _options.Value.MapPixelSize.HasValue) m.Options.Add("resolution",_options.Value.MapPixelSize.ToString());
                    return m;
                }).ToList();
            }
            else {
                // Temporary limit on pro jobs / data packages
                exesList = _options.Value.ProReportSections.Select(m => {
                    if (!m.Options.ContainsKey("maxresolution")) m.Options.Add("maxresolution", 1000.ToString());
                    return m;
                }).ToList();
            }

            return exesList.Select(m => 
                new Ecoset.GeoTemporal.Remote.Variable() {
                    Name = m.Name,
                    Method = m.Method,
                    Options = m.Options
                }
            ).ToList();
        }

        private List<Ecoset.GeoTemporal.Remote.Variable> GetCustomRequest(List<string> customVariables) 
        {
            var toProcess = _options.Value.ProReportSections.Select(m => 
                new Ecoset.GeoTemporal.Remote.Variable() {
                    Name = m.Name,
                    Method = m.Method,
                    Options = m.Options
                }
            ).Where(n => customVariables.Contains(n.Name)).ToList();
            return toProcess;
        }

        private Models.JobStatus ToLeftStatus(GeoTemporal.Remote.JobStatus status) {
            if (status == GeoTemporal.Remote.JobStatus.Failed) { return Models.JobStatus.Failed; }
            else if (status == GeoTemporal.Remote.JobStatus.Queued) { return Models.JobStatus.Queued; }
            else if (status == GeoTemporal.Remote.JobStatus.NonExistent) { return Models.JobStatus.Failed; }
            else if (status == GeoTemporal.Remote.JobStatus.Processing) { return Models.JobStatus.Processing; }
            else if (status == GeoTemporal.Remote.JobStatus.Ready) { return Models.JobStatus.Completed; }
            else return Models.JobStatus.Submitted;
        }

    }
}