using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Ecoset.GeoTemporal.Remote
{
    public class VariableListItem
    {
        public string Id { get; set; }
        public string FriendlyName { get; set; }
        public string Description { get; set; }
        public string Unit { get; set; }
        public List<MethodListItem> Methods { get; set; }

        public VariableListItem() {
            Methods = new List<MethodListItem>();
        }
    }

    public class MethodListItem
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string License { get; set; }
        public string LicenseUrl { get; set; }
        public TemporalDimension TemporalExtent { get; set; }
        //public SpatialDimension SpatialExtent { get; set; }
    }

    public class TemporalDimension
    {
        [JsonProperty("slices")]
        public SimpleDate[] Slices { get; set; }
        [JsonProperty("minDate")]
        public Date MinDate { get; set; }
        [JsonProperty("maxDate")]
        public Date MaxDate { get; set; }
        
        public TemporalDimension() {
            Slices = Array.Empty<SimpleDate>();
        }
    }

    public class Date
    {
        [JsonProperty("day")]
        public Nullable<int> Day { get; set; }
        [JsonProperty("month")]
        public Nullable<int> Month { get; set; }
        [JsonProperty("year")]
        public int Year { get; set; }
    }

}