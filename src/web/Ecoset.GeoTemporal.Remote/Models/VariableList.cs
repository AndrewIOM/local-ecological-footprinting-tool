using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

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
        public TemporalExtent TemporalExtent { get; set; }
        //public SpatialDimension SpatialExtent { get; set; }
    }

    public class TemporalExtent
    {
        [JsonPropertyName("slices")]
        public Date[] Slices { get; set; }
        [JsonPropertyName("minDate")]
        public Date MinDate { get; set; }
        [JsonPropertyName("maxDate")]
        public Date MaxDate { get; set; }
        
        public TemporalExtent() {
            Slices = Array.Empty<Date>();
        }
    }

    public class Date
    {
        [JsonPropertyName("Day")]
        public Nullable<int> Day { get; set; }
        [JsonPropertyName("Month")]
        public Nullable<int> Month { get; set; }
        [JsonPropertyName("Year")]
        public int Year { get; set; }
    }

}