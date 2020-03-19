using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Ecoset.WebUI.Models.AdminViewModels {

    public class AddSubscriptionViewModel
    {
        [Required]
        public string MasterUserId { get; set; }
        
        public DateTime? StartTime { get; set; }
        public DateTime? ExpiryTime { get; set; }
        public int? RateLimit { get; set; }
        public int? AnalysisCap { get; set; }
        public List<GroupSubsciptionViewModel> Groups { get; set; }
    
        public AddSubscriptionViewModel() {
            Groups = new List<GroupSubsciptionViewModel>();
        }
    }

    public class GroupSubsciptionViewModel
    {
        public string GroupName { get; set; }
        public string EmailWildcard { get; set; }
    }

    public class SubscriptionListItemViewModel 
    {
        public Guid Id { get; set; }
        public string ContactUserName { get; set; }
        public List<GroupSubsciptionViewModel> Groups { get; set; }
        public int? RateLimit { get; set; }
        public int? AnalysisCap { get; set; }
        public bool Revoked { get; set; }
        public DateTime Start { get; set; }
        public DateTime? Expires { get; set; }
    }
}