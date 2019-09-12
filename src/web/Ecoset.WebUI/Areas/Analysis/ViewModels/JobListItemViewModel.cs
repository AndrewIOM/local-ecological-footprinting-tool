
using System;

namespace Ecoset.WebUI.Models.JobViewModels {
    public class JobListItemViewModel
    {
        public int Id {get;set;}
        public DateTime DateAdded {get;set;}
        public DateTime? DateCompleted {get;set;}
        public string Name {get;set;}
        public string Description {get;set;}
        public string SubmittedBy {get;set;}
        public string Status {get;set;}
        public double? LatitudeSouth {get; set;}
        public double? LatitudeNorth {get;set;}
        public double? LongitudeEast {get;set;}
        public double? LongitudeWest {get;set;}
    }
}