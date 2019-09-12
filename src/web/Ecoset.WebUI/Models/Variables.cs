using System;

namespace Ecoset.WebUI.Models {

    public class Variable 
    {
        public string Name { get; set; }
        public string Description { get; set; }
    }

    public class GeotemporalContext {
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public double? LatitudeDD { get; set; }
        public double? LongitudeDD { get; set; }
    }

}