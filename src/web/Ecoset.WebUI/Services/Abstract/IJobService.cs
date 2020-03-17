using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ecoset.WebUI.Models;
using Ecoset.WebUI.Models.JobViewModels;

namespace Ecoset.WebUI.Services.Abstract
{
    public interface IJobService
    {
        Task<int?> SubmitJob(Job job);
        IEnumerable<Job> GetAll();
        IEnumerable<Job> GetAllForUser(string userId);
        Job GetById(int jobId);
        bool StopJob(int jobId);
        bool HideJob(int jobId);
        void RefreshJobStatus(int jobId);
        Task<bool> ActivateProFeatures(int jobId, string userId);
        ReportData GetReportData(int jobId);

        // Data Packages
        Task<Guid?> SubmitDataPackage(DataPackage package, List<AvailableVariable> variables);
        IEnumerable<DataPackage> GetAllDataPackagesForUser(string userId);
        Task<DataPackage> GetDataPackage(System.Guid dataPackageId);
        Task<ReportData> GetDataPackageData(System.Guid dataPackageId);
        Task<JobStatus> PollDataPackage(System.Guid dataPackageId);
    }
}
