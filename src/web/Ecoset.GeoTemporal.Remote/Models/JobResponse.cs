using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Ecoset.GeoTemporal.Remote
{
    public class JobPollResponse
    {
        [JsonProperty("success")]
        public bool Success { get; set; }
        [JsonProperty("message")]
        public string Message { get; set; }
        [JsonProperty("jobState")]
        public JobStatus JobStatus { get; set; }
    }

    public class JobFetchResponse
    {
        [JsonProperty("north")]
        public float North { get; set; }

        [JsonProperty("south")]
        public float South { get; set; }

        [JsonProperty("east")]
        public float East { get; set; }

        [JsonProperty("west")]
        public float West { get; set; }

        [JsonProperty("output")]
        public IEnumerable<EcosetOutput> Outputs { get; set; }

        public JobFetchResponse() {
            Outputs = new List<EcosetOutput>();
        }
    }

    public class JobSubmissionResponse
    {
        [JsonProperty("success")]
        public bool Success { get; set; }
        [JsonProperty("message")]
        public string Message { get; set; }
        [JsonProperty("jobId")]
        public Guid JobId { get; set; }
    }

    public class EcosetOutput
    {
        public string Name { get; set; }
        [JsonProperty("method_used")]
        public string MethodUsed { get; set; }
        [JsonProperty("output_format")]
        public string OutputFormat { get; set; }
        [JsonProperty("data")]
        public JToken Data { get; set; }
    }

    public class Statistics
    {
        [JsonProperty("Minimum")]
        public float Min { get; set; }
        [JsonProperty("Maximum")]
        public float Max { get; set; }
        [JsonProperty("Mean")]
        public float Mean { get; set; }
        [JsonProperty("StDev")]
        public float StdDev { get; set; }
    }

    public class ExecutableResult
    {
        public string Name { get; set; }
        public string MethodUsed { get; set; }
        public string OutputFormat { get; set; }
        public IDataResult RawData { get; set; }
    }

    public interface IDataResult { }

    public class RawDataResult : IDataResult
    {
        public Statistics Stats { get; set; }
        public double[,] DataCube { get; set; }
        public double NoDataValue { get; set; }
        public double Columns { get; set; }
        public double Rows { get; set; }
    }

    public class DataTableListResult : IDataResult
    {
        public List<Dictionary<string,string>> Rows { get; set; }
    }

    public class DataTableStatsResult : IDataResult
    {
        public Statistics Stats { get; set; }
    }

    public class Base64FileResult : IDataResult {
        public string Base64Data { get; set; }
        public string FileFormat { get; set; }
    }

    public class CsvFileResult : IDataResult {
        public string CsvData { get; set; }
    }
}