using System;
using System.Collections.Generic;

namespace Ecoset.WebUI.Models {

    public class Variable 
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Unit { get; set; }
        public List<VariableMethod> Methods { get; set; }
    }

    public class VariableMethod
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string License { get; set; }
        public string LicenseUrl { get; set; }
        public MethodTime TimesAvailable { get; set; }
    }

    public class MethodTime
    {
        public SimpleDate ExtentMax { get; set; }
        public SimpleDate ExtentMin { get; set; }
        public List<SimpleDate> TimeSlices { get; set; }
    }

    public class SimpleDate
    {
        public Nullable<int> Day { get; set; }
        public Nullable<int> Month { get; set; }
        public int Year { get; set; }
    }

    public class GeotemporalContext {
        public SimpleDate StartTime { get; set; }
        public SimpleDate EndTime { get; set; }
        public double? LatitudeDD { get; set; }
        public double? LongitudeDD { get; set; }
    }

}