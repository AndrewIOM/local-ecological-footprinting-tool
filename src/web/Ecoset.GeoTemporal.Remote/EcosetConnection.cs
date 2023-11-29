using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Ecoset.GeoTemporal.Remote
{
    public class EcosetConnection : IGeoSpatialConnection
    {
        private readonly string _endpoint;
        private readonly JsonSerializerOptions _options;

        public EcosetConnection(string endpoint)
        {
            _endpoint = endpoint;
            _options = new JsonSerializerOptions();
            _options.Converters.Add(new JsonStringEnumConverter());
        }

        public async Task<JobFetchResponse> FetchResultAsync(JobId id)
        {
            var result = await Get<JobFetchResponse>("api/v1/Data/fetch/" + id.Id.ToString());
            return result;
        }

        public Task<JobFetchResponse> TryFetchResultAsync(JobId id)
        {
            throw new NotImplementedException();
        }

        public async Task<VariableListItem[]> ListAvailableDatasets()
        {
            var result = await Get<VariableListItem[]>("api/v1/Data/list");
            return result;
        }

        public async Task<JobStatus> GetJobStatusAsync(JobId id)
        {
            var result = await Get<JobPollResponse>("api/v1/Data/status/" + id.Id.ToString());
            return result.JobStatus;
        }

        public async Task<JobId> SubmitJobAsync(JobSubmissionRequest request)
        {
            var result = await Post<JobSubmissionResponse>("api/v1/Data/submit", request);
            return new JobId(result.JobId);
        }

        private async Task<T> Get<T>(string endpoint) 
        {
            using var client = new HttpClient();
            client.BaseAddress = new Uri(_endpoint);
            var response = await client.GetAsync(endpoint);
            if (response.IsSuccessStatusCode)
            {
                using (Stream contentStream = await response.Content.ReadAsStreamAsync())
                {
                    try
                    {
                        var result = await JsonSerializer.DeserializeAsync<T>(contentStream, _options);
                        return result;
                    }
                    catch (Exception e)
                    {
                        throw new Exception("Ecoset returned an unexpected response: " + e.Message);
                    }
                }
            }
            else
            {
                throw new HttpRequestException("The request was not successful: " + response.StatusCode);
            }
        }

        private async Task<T> Post<T>(string endpoint, object request)
        {
            using var client = new HttpClient();
            client.BaseAddress = new Uri(_endpoint);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var json = JsonSerializer.Serialize(request);
            var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

            var req = new HttpRequestMessage(HttpMethod.Post, endpoint);
            req.Content = httpContent;

            var response = await client.SendAsync(req, HttpCompletionOption.ResponseHeadersRead);
            Console.WriteLine(response);

            if (response.IsSuccessStatusCode)
            {
                using Stream responseStream = await response.Content.ReadAsStreamAsync();
                T responseContent = await JsonSerializer.DeserializeAsync<T>(responseStream);
                return responseContent;
            }
            else
            {
                throw new HttpRequestException("The request was not successful: " + response.StatusCode);
            }
        }
    }
}
