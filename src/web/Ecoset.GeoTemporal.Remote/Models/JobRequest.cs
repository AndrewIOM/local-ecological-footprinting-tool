using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Ecoset.GeoTemporal.Remote
{
    public class JobSubmissionRequest
    {
        [JsonProperty("LatitudeNorth")]
        public double North { get; set; }
        [JsonProperty("LatitudeSouth")]
        public double South { get; set; }
        [JsonProperty("LongitudeEast")]
        public double East { get; set; }
        [JsonProperty("LongitudeWest")]
        public double West { get; set; }
        [JsonProperty("TimeMode")]
        public TimeMode TimeMode { get; set; }
        [JsonProperty("Variables")]
        public List<Variable> Variables { get; set; }
    }

    public class Variable
    {
        [JsonProperty("Name")]
        public string Name { get; set; }
        [JsonProperty("Method")]
        public string Method { get; set; }
        [JsonProperty("Options")]
        public object Options { get; set; }
    }

    public class TimeMode 
    {
        [JsonProperty("kind")]
        public string Kind { get; set; }
        [JsonProperty("date")]
        public Date Date { get; set; }
    }

    public class JobPollRequest
    {
        [JsonProperty("jobId")]
        public Guid JobId { get; set; }
    }

    public class JobFetchRequest
    {
        [JsonProperty("jobId")]
        public Guid JobId { get; set; }
    }
}