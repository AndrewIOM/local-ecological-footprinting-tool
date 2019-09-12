
using System;

namespace Ecoset.WebUI.Models {
    public class ProActivation {
        public Guid Id { get; set; }
        public DateTime TimeOfPurchase { get; set; }
        public string UserIdOfPurchaser { get; set; }
        public int CreditsSpent { get; set; }

        public string JobProcessorReference { get; set; }
        public JobStatus ProcessingStatus { get; set; }

        public int JobId { get; set; }
        public Job Job { get; set; }
    }
}