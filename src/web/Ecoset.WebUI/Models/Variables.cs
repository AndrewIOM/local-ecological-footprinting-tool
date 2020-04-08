using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Ecoset.WebUI.Models {

    public class Variable 
    {
        /// <summary>
        /// A short, unique identifier for this variable (used for requesting data packages)
        /// </summary>
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        /// <summary>
        /// The unit of meaure used in the data returned by this variable
        /// </summary>
        public string Unit { get; set; }
        /// <summary>
        /// Methods (or protocols) through which the variable has been measured
        /// </summary>
        public List<VariableMethod> Methods { get; set; }

        public Variable() {
            Methods = new List<VariableMethod>();
        }
    }

    public class VariableMethod
    {
        /// <summary>
        /// A short identifier for this method (which is used when requesting data packages)
        /// </summary>
        public string Id { get; set; }
        public string Name { get; set; }
        /// <summary>
        /// The specific license for the data that specifies owner, use rights etc.
        /// </summary>
        public string License { get; set; }
        /// <summary>
        /// A URL to the full text of the license conditions for this dataset
        /// </summary>
        public string LicenseUrl { get; set; }
        /// <summary>
        /// Specifies whether the data is available in specific time slices, or
        /// for a temporal extent.
        /// </summary>
        public MethodTime TimesAvailable { get; set; }
    }

    public class MethodTime
    {
        /// <summary>
        /// If not empty, indicates the earliest date for which data is available
        /// </summary>
        public SimpleDate ExtentMax { get; set; }
        /// <summary>
        /// If not empty, indicates the latest date for which data is available
        /// </summary>
        public SimpleDate ExtentMin { get; set; }
        /// <summary>
        /// If not empty, indicates specific dates for which the data is available
        /// </summary>
        public List<SimpleDate> TimeSlices { get; set; }

        public MethodTime() {
            TimeSlices = new List<SimpleDate>();
        }
    }

    /// <summary>
    /// A date of one of three temporal resolutions: year, month, or day
    /// </summary>
    public class SimpleDate
    {
        public Nullable<int> Day { get; set; }
        public Nullable<int> Month { get; set; }
        [Required]
        public int Year { get; set; }
    }

    public class GeotemporalContext {
        public SimpleDate StartTime { get; set; }
        public SimpleDate EndTime { get; set; }
        public double? LatitudeDD { get; set; }
        public double? LongitudeDD { get; set; }
    }

    /// Represents a variable and method that are available in the data registry.
    public class AvailableVariable
    {
        /// <summary>
        /// A short, unique identifier for this variable (used for requesting data packages)
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// A short identifier for this variable method (used for requesting data packages)
        /// </summary>
        public string MethodId { get; set; }
        public string DescriptiveName { get; set; }
        public string Description { get; set; }
        /// <summary>
        /// The unit of meaure used in the data returned by this variable
        /// </summary>
        public string Unit { get; set; }
        /// <summary>
        /// The specific license for the data that specifies owner, use rights etc.
        /// </summary>
        public string License { get; set; }
        /// <summary>
        /// A URL to the full text of the license conditions for this dataset
        /// </summary>
        public string LicenseUrl { get; set; }
    }

}