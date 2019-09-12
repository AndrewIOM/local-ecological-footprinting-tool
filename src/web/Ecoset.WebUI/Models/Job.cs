using System;
using System.Collections.Generic;

namespace Ecoset.WebUI.Models {
    public class Job {
        public int Id {get;set;}
        public string JobProcessorReference { get; set; }
        public string Name {get;set;}
        public string Description { get; set;}
        public JobStatus Status { get; set;}
        public double LatitudeSouth {get; set;}
        public double LatitudeNorth {get;set;}
        public double LongitudeEast {get;set;}
        public double LongitudeWest {get;set;}
        public DateTime DateAdded {get;set;}
        public DateTime? DateCompleted {get;set;}

        //Relations
        public ApplicationUser CreatedBy {get;set;}
        public List<Notification> Notifications { get; set; }
        public ProActivation ProActivation { get; set; }
    }

    public enum JobStatus {
        Submitted = 0,
        Processing = 1,
        Completed = 3,        
        Failed = 4,
        GeneratingOutput = 5,
        Queued = 6
    }
}