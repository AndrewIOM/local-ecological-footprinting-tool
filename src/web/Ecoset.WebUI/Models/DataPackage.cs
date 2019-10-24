using System;
using System.Collections.Generic;

namespace Ecoset.WebUI.Models {
    public class DataPackage {
        public Guid Id { get; set; }
        public string JobProcessorReference { get; set; }
        public string Name {get;set;}
        public JobStatus Status { get; set;}
        public double LatitudeSouth { get; set; }
        public double LatitudeNorth { get; set; }
        public double LongitudeEast { get; set; }
        public double LongitudeWest { get; set; }
        public DateTime TimeRequested { get; set; }
        public DateTime? TimeCompleted { get; set; }

        // Contains a serialised job processor request
        // This may contain spatial and temporal information
        public string RequestComponents { get; set; }

        // Relations
        public ApplicationUser CreatedBy { get; set; }
    }

}