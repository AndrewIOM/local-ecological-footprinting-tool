using System;

namespace Ecoset.WebUI.Models.AdminViewModels {

    public class AddSubscriptionViewModel
    {
        [Required]
        public int MasterUserId { get; set; }
        
        public DateTime? StartTime { get; set; }
        public DateTime? ExpiryTime { get; set; }
        public int? RateLimit { get; set; }
        public int? AnalysisCap { get; set; }
        public List<AddGroupSubsciptionViewModel> Groups { get; set; }
    
        public AddSubscriptionViewModel() {
            Groups = new List<AddGroupSubsciptionViewModel>();
        }
    }

    public class AddGroupSubsciptionViewModel
    {
        public string GroupName { get; set; }
        public string EmailWildcard { get; set; }
    }
}