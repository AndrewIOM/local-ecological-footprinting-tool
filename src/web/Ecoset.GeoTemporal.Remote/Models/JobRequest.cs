using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Ecoset.GeoTemporal.Remote
{
    public class JobSubmissionRequest
    {
        [JsonPropertyName("LatitudeNorth")]
        public double North { get; set; }
        [JsonPropertyName("LatitudeSouth")]
        public double South { get; set; }
        [JsonPropertyName("LongitudeEast")]
        public double East { get; set; }
        [JsonPropertyName("LongitudeWest")]
        public double West { get; set; }
        [JsonPropertyName("TimeMode")]
        public TimeMode TimeMode { get; set; }
        [JsonPropertyName("Variables")]
        public List<Variable> Variables { get; set; }
    }

    public class Variable
    {
        [JsonPropertyName("Name")]
        public string Name { get; set; }
        [JsonPropertyName("Method")]
        public string Method { get; set; }
        [JsonPropertyName("Options")]
        public object Options { get; set; }
    }

    public class TimeMode 
    {
        [JsonPropertyName("kind")]
        public string Kind { get; set; }
        [JsonPropertyName("date")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Date Date { get; set; }
    }

    public class JobPollRequest
    {
        [JsonPropertyName("jobId")]
        public Guid JobId { get; set; }
    }

    public class JobFetchRequest
    {
        [JsonPropertyName("jobId")]
        public Guid JobId { get; set; }
    }
}