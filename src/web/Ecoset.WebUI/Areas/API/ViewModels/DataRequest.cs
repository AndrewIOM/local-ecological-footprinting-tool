using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Ecoset.WebUI.Options;
using Microsoft.Extensions.Options;

namespace Ecoset.WebUI.Models.JobViewModels {

    public class VariableRequest
    {
        [Required]
        public string Name { get; set; }
        public string Method { get; set; }
    }

    public class DataRequest : IValidatableObject
    {
        private double _maximumLatitudinalExtent = 2.00;
        private double _maximumLongitudinalExtent = 2.00;
        private const double _minimumLatitudinalExtent = 0.1;
        private const double _minimumLongitudinalExtent = 0.1;

        [Required]
        [Range(-90, 90)]
        public double? LatitudeSouth {get; set;}
        [Required]
        [Range(-90, 90)]
        public double? LatitudeNorth {get;set;}
        [Required]
        [Range(-180, 180)]
        public double? LongitudeEast {get;set;}
        [Required]
        [Range(-180, 180)]
        public double? LongitudeWest {get;set;}
        [Required]
        public RequestedTime DateMode { get; set; }
        public SimpleDate Date { get; set; }
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