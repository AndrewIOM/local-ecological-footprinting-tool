using System;
using System.IO;
using System.Linq;
using Ecoset.WebUI.Data;
using Ecoset.WebUI.Models;
using Ecoset.WebUI.Services.Abstract;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Ecoset.WebUI.Services.Concrete
{
    public class SubscriptionService : ISubscriptionService
    {
        private readonly ApplicationDbContext _context;
        private ILogger<SubscriptionService> _logger;
        public SubscriptionService(ILogger<SubscriptionService> logger, ApplicationDbContext context) {
            _logger = logger;
            _context = context;
        }

        public Subscription GetActiveForUser(string userId) {
            var user = _context.Users.Include(m => m.Subscriptions).ThenInclude(s => s.GroupSubscriptions).FirstOrDefault(m => m.Id == userId);
            if (user == null) return null;
            
            // TODO Include wildcard subscriptions
            return user.Subscriptions.FirstOrDefault(s => s.StartDate >= DateTime.Now && (s.Expires.HasValue ? s.Expires.Value < DateTime.Now : true));
        }

        public void Revoke(string subscriptionId) {
            throw new NotImplementedException();
        }
    }

    // Can be in charge of one or many subscriptions
    // Can benefit from one - many wildcard (group) subscriptions

}
