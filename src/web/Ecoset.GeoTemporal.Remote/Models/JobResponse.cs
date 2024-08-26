using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Ecoset.GeoTemporal.Remote
{
    public class JobPollResponse
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }
        [JsonPropertyName("message")]
        public string Message { get; set; }
        [JsonPropertyName("jobState")]
        public JobStatus JobStatus { get; set; }
    }

    public class JobFetchResponse
    {
        [JsonPropertyName("north")]
        public float North { get; set; }

        [JsonPropertyName("south")]
        public float South { get; set; }

        [JsonPropertyName("east")]
        public float East { get; set; }

        [JsonPropertyName("west")]
        public float West { get; set; }

        [JsonPropertyName("output")]
        public IEnumerable<EcosetOutput> Outputs { get; set; }

        public JobFetchResponse() {
            Outputs = new List<EcosetOutput>();
        }
    }

    public class JobSubmissionResponse
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }
        [JsonPropertyName("message")]
        public string Message { get; set; }
        [JsonPropertyName("jobId")]
        public Guid JobId { get; set; }
    }

    public class EcosetOutput
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("method_used")]
        public string MethodUsed { get; set; }
        [JsonPropertyName("output_format")]
        public string OutputFormat { get; set; }
        [JsonPropertyName("data")]
        [JsonConverter(typeof(DataFieldTypeConverter))]
        public IDataResult Data { get; set; }
    }

    public class Statistics
    {
        [JsonPropertyName("Minimum")]
        public float? Min { get; set; }
        [JsonPropertyName("Maximum")]
        public float? Max { get; set; }
        [JsonPropertyName("Mean")]
        public float? Mean { get; set; }
        [JsonPropertyName("StDev")]
        public float? StdDev { get; set; }
    }

    public class Dimensions
    {
        [JsonPropertyName("north")]
        public float North { get; set; }
        [JsonPropertyName("south")]
        public float South { get; set; }
        [JsonPropertyName("east")]
        public float East { get; set; }
        [JsonPropertyName("west")]
        public float West { get; set; }
        [JsonPropertyName("year")]
        public int Year { get; set; }
        [JsonPropertyName("month")]
        public int? Month { get; set; }
        [JsonPropertyName("day")]
        public int? Day { get; set; }
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
        [JsonConverter(typeof(DataCubeTypeConverter))]
        public double?[,] DataCube { get; set; }
        public int Columns { get; set; }
        public int Rows { get; set; }
    }

    public class DataTableListResult : IDataResult
    {
        public List<Dictionary<string,string>> Rows { get; set; }
    }

    public class DataTableStatsResult : IDataResult
    {
        public Statistics Stats { get; set; }
    }

}