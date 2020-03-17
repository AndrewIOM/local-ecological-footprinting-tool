using System;
using Ecoset.WebUI.Models;

namespace Ecoset.WebUI.Services.Abstract
{
    public interface ISubscriptionService
    {
        ActiveSubscription GetActiveForUser(string userId);
        void Revoke(System.Guid subsciptionId);
    }

    public class ActiveSubscription 
    {
        public bool IsDefault { get; set; }
        public string GroupName { get; set; }
        public int? RateLimit { get; set; }
        public int? AnalysisCap { get; set; }
        public DateTime? Expires { get; set; }
    }

}