using System.Collections.Generic;
using Ecoset.WebUI.Models;

namespace Ecoset.WebUI.Services.Abstract
{
    public interface IJobService
    {
        int? SubmitJob(Job job);
        IEnumerable<Job> GetAll();
        IEnumerable<Job> GetAllForUser(string userId);
        Job GetById(int jobId);
        bool StopJob(int jobId);
        void RefreshJobStatus(int jobId);
        bool ActivateProFeatures(int jobId, string userId);
        ReportData GetReportData(int jobId);
    }
}
