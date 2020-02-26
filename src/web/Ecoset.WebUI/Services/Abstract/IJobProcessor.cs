
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
        Task<string> StartProJob(JobSubmission job);
        void StopJob(string processorJobId);
        Task<ReportData> GetReportData(string processorJobId);
        Task<List<Tuple<string,string,string>>> GetReportFiles(string processorJobId);

        Task<List<Variable>> ListVariables();
        Task<string> StartDataPackage(JobSubmission job, List<string> customVariables);
    }
}
