using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Ecoset.WebUI.Models {

    public class Subscription 
    {
        [Key]
        public Guid Id { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? Expires { get; set; }
        public bool Revoked { get; set; }
        public int? RateLimit { get; set; }
        public int? AnalysisCap { get; set; }
        public virtual ApplicationUser PrimaryContact { get; set; }
        public IEnumerable<GroupSubscription> GroupSubscriptions { get; set; }
    }

    public class GroupSubscription {
        public Guid GroupSubscriptionId { get; set; }
        public string GroupName { get; set; }
        public string EmailWildcard { get; set; }
        public virtual Subscription Subscription { get; set; }
    }

}