using System.Threading.Tasks;

namespace Ecoset.GeoTemporal.Remote
{
    public interface IGeoSpatialConnection
    {
        Task<JobId> SubmitJobAsync(JobSubmissionRequest request);
        Task<JobStatus> GetJobStatusAsync(JobId id);
        Task<JobFetchResponse> FetchResultAsync(JobId id);
        Task<JobFetchResponse> TryFetchResultAsync(JobId id);
        Task<VariableListItem[]> ListAvailableDatasets();
    }
}