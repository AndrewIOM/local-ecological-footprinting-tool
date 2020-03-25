
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ecoset.WebUI.Models;
using Ecoset.WebUI.Models.JobProcessor;

namespace Ecoset.WebUI.Services.Abstract
{
    public interface IJobProcessor
    {
        Task<JobStatus> GetStatus(string processorJobId, JobStatus localStatus);
        Task<string> StartJob(JobSubmission job);
        void StopJob(string processorJobId);
        Task<ReportData> GetReportData(string processorJobId);
        Task<string> StartProJob(JobSubmission job);
        Task<string> StartDataPackage(JobSubmission job, RequestedTime dateMode, int? year, int? month, int? day, List<string> customVariables);
    }
}
