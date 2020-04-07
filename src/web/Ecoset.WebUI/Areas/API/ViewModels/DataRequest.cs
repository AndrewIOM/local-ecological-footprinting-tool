using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Ecoset.WebUI.Options;
using Microsoft.Extensions.Options;

namespace Ecoset.WebUI.Models.JobViewModels {

    public class AuthToken {
        /// <summary>
        /// An authentication token that may be added to HTTP headers
        /// </summary>
        [Required]
        public string Token { get; set; }
    }

    public class VariableRequest
    {
        /// <summary>
        /// The unique name of the variable
        /// </summary>
        [Required]
        public string Name { get; set; }
        /// <summary>
        /// The unique method id of the variable
        /// </summary>
        public string Method { get; set; }
    }

    public class DataPackageId {
        /// <summary>
        /// A unique identifier for a specific data package
        /// </summary>
        public Guid Id { get; set; }
    }

    /// <summary>
    /// A request for a data package, which bundles multiple
    /// variables (e.g. sea depth, air temperature) at a specified
    /// space and time.
    /// </summary>
    public class DataRequest : IValidatableObject
    {
        private double _maximumLatitudinalExtent = 2.00;
        private double _maximumLongitudinalExtent = 2.00;
        private const double _minimumLatitudinalExtent = 0.1;
        private const double _minimumLongitudinalExtent = 0.1;

        /// <summary>
        /// Space: The southern latitude in decimal degrees
        /// </summary>
        [Required]
        [Range(-90, 90)]
        public double? LatitudeSouth {get; set;}
        /// <summary>
        /// Space: The northern latitude in decimal degrees
        /// </summary>
        [Required]
        [Range(-90, 90)]
        public double? LatitudeNorth {get;set;}
        /// <summary>
        /// Space: The eastern longitude in decimal degrees
        /// </summary>
        [Required]
        [Range(-180, 180)]
        public double? LongitudeEast {get;set;}
        /// <summary>
        /// Space: The western longitude in decimal degrees
        /// </summary>
        [Required]
        [Range(-180, 180)]
        public double? LongitudeWest {get;set;}

        /// <summary>
        /// Time: Specify whether you wish to retrive the latest available datasets,
        /// data that represent state before a certain date; or data from an exact date
        /// </summary>
        [Required]
        public RequestedTime DateMode { get; set; }
        /// <summary>
        /// If you are requesting data from before a date or for a specific date (see DateMode),
        /// you must include the date here
        /// </summary>
        public SimpleDate Date { get; set; }
        /// <summary>
        /// A list of variables for which to obtain data. If you do not specify a method,
        /// and only one is available, this will be automatically selected. You must
        /// specify at least one variable.
        /// </summary>
        public List<VariableRequest> Variables { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var options = (IOptions<EcosetAppOptions>)validationContext.GetService(typeof(IOptions<EcosetAppOptions>));
            _maximumLatitudinalExtent = options.Value.MaximumAnalysisHeight;
            _maximumLongitudinalExtent = options.Value.MaximumAnalysisWidth;

            if (Variables == null) {
                yield return new ValidationResult("You must specify variables");
            } else if (Variables.Count == 0) {
                yield return new ValidationResult("You must specify at least one variable");
            }

            if (LatitudeSouth.HasValue && LatitudeNorth.HasValue) {
                if (LatitudeNorth < LatitudeSouth) {
                    yield return new ValidationResult("Northern latitude cannot be less than southern latitude.", new[] {"LatitudeNorth", "LatitudeSouth"});
                }
                else if (LatitudeNorth - LatitudeSouth > _maximumLatitudinalExtent) {
                    yield return new ValidationResult("The maximum latitudinal range supported is " + _maximumLatitudinalExtent + " degrees.", new[] {"LatitudeNorth", "LatitudeSouth"});
                }
                else if (LatitudeNorth - LatitudeSouth < _minimumLatitudinalExtent) {
                    yield return new ValidationResult("The minimum latitudinal range supported is " + _minimumLatitudinalExtent + " degrees.", new[] {"LatitudeNorth", "LatitudeSouth"});
                }
            }
            if (LongitudeEast.HasValue && LongitudeWest.HasValue) {
                if (LongitudeEast < LongitudeWest) {
                    yield return new ValidationResult("Western longitude cannot be higher than eastern longitude.", new[] {"LongitudeEast", "LongitudeWest"});
                }
                else if (LongitudeEast - LongitudeWest > _maximumLongitudinalExtent) {
                    yield return new ValidationResult("The maximum longitudinal range supported is " + _maximumLongitudinalExtent + " degrees.", new[] {"LongitudeEast", "LongitudeWest"});
                }                
                else if (LongitudeEast - LongitudeWest < _minimumLongitudinalExtent) {
                    yield return new ValidationResult("The minimum longitudinal range supported is " + _minimumLongitudinalExtent + " degrees.", new[] {"LongitudeEast", "LongitudeWest"});
                }
            }
            if (DateMode == RequestedTime.Before || DateMode == RequestedTime.Exact) {
                if (Date == null) yield return new ValidationResult("You must specify a date when using 'before' or 'exact' date mode.");
            }
        }
    }
}