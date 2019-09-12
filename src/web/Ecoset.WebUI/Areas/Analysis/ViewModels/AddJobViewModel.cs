using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Ecoset.WebUI.Models.JobViewModels {
    public class AddJobViewModel : IValidatableObject
    {
        private const double _maximumLatitudinalExtent = 4.00;
        private const double _maximumLongitudinalExtent = 4.00;
        private const double _minimumLatitudinalExtent = 0.1;
        private const double _minimumLongitudinalExtent = 0.1;

        [Required]
        public string Name {get;set;}
        public string Description {get;set;}
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

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (LatitudeSouth.HasValue && LatitudeNorth.HasValue) {
                if (LatitudeNorth < LatitudeSouth) {
                    yield return new ValidationResult("Northern latitude cannot be less than southern latitude.", new[] {"LatitudeNorth", "LatitudeSouth"});
                }
                else if (LatitudeNorth - LatitudeSouth > _maximumLatitudinalExtent) {
                    yield return new ValidationResult("The maximum latitudinal range currently supported is " + _maximumLatitudinalExtent + " degrees.", new[] {"LatitudeNorth", "LatitudeSouth"});
                }
                else if (LatitudeNorth - LatitudeSouth < _minimumLatitudinalExtent) {
                    yield return new ValidationResult("The minimum latitudinal range currently supported is " + _minimumLatitudinalExtent + " degrees.", new[] {"LatitudeNorth", "LatitudeSouth"});
                }
            }
            if (LongitudeEast.HasValue && LongitudeWest.HasValue) {
                if (LongitudeEast < LongitudeWest) {
                    yield return new ValidationResult("Western longitude cannot be higher than eastern longitude.", new[] {"LongitudeEast", "LongitudeWest"});
                }
                else if (LongitudeEast - LongitudeWest > _maximumLongitudinalExtent) {
                    yield return new ValidationResult("The maximum longitudinal range currently supported is " + _maximumLongitudinalExtent + " degrees.", new[] {"LongitudeEast", "LongitudeWest"});
                }                
                else if (LongitudeEast - LongitudeWest < _minimumLongitudinalExtent) {
                    yield return new ValidationResult("The minimum longitudinal range currently supported is " + _minimumLongitudinalExtent + " degrees.", new[] {"LongitudeEast", "LongitudeWest"});
                }

            }
        }
    }
}