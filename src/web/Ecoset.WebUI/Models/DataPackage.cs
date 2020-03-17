using System;

namespace Ecoset.WebUI.Models {
    public class DataPackage {
        public Guid Id { get; set; }
        public string JobProcessorReference { get; set; }
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
        
        public RequestedTime DataRequestedTime { get; set; }
        public int? Year { get; set; }
        public int? Month { get; set; }
        public int? Day { get; set; }

        // Relations
        public virtual ApplicationUser CreatedBy { get; set; }
    }

    public enum RequestedTime {
        Latest,
        Before,
        Exact
    }

}