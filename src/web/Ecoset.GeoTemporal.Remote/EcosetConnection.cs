﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Ecoset.GeoTemporal.Remote
{
    public class EcosetConnection : IGeoSpatialConnection
    {
        private string _endpoint;

        public EcosetConnection(string endpoint)
        {
            _endpoint = endpoint;
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
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(_endpoint);
                var response = await client.GetAsync(endpoint);
                if (response.IsSuccessStatusCode)
                 {
                    using (Stream responseStream = await response.Content.ReadAsStreamAsync())
                    {
                        var serialiser = JsonSerializer.Create();
                        using (var sr = new StreamReader(responseStream))
                        using (var jsonTextReader = new JsonTextReader(sr)) {
                            try {
                                T responseContent = serialiser.Deserialize<T>(jsonTextReader);
                                return responseContent;
                            } catch {
                                throw new Exception("Ecoset returned an unexpected response");
                            }
                        }
                    }
                }
                else {
                    throw new HttpRequestException("The request was not successful: " + response.StatusCode);
                }
            }
        }

        private async Task<T> Post<T>(string endpoint, object request)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(_endpoint);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var json = JsonConvert.SerializeObject(request);
                var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

                var req = new HttpRequestMessage(HttpMethod.Post, endpoint);
                req.Content = httpContent;

                var response = await client.SendAsync(req, HttpCompletionOption.ResponseHeadersRead);
                Console.WriteLine(response);

                if (response.IsSuccessStatusCode)
                {
                    string jsonMessage;
                    using (Stream responseStream = await response.Content.ReadAsStreamAsync())
                    {
                        jsonMessage = new StreamReader(responseStream).ReadToEnd();
                    }

                    T responseContent = (T)JsonConvert.DeserializeObject(jsonMessage, typeof(T));
                    return responseContent;
                }
                else {
                    throw new HttpRequestException("The request was not successful: " + response.StatusCode);
                }
            }
        }
    }

    public static class JsonReaderExtensions
    {
        public static IEnumerable<T> SelectTokensWithRegex<T>(this JsonReader jsonReader, Regex regex)
        {
            JsonSerializer serializer = new JsonSerializer();
            while (jsonReader.Read())
            {
                if (regex.IsMatch(jsonReader.Path) && jsonReader.TokenType != JsonToken.PropertyName)
                {
                    yield return serializer.Deserialize<T>(jsonReader);
                }
            }
        }
    }

}
