using System;
using System.IO;
using Ecoset.WebUI.Services.Abstract;

namespace Ecoset.WebUI.Services.Concrete
{
    public class SubscriptionService : ISubscriptionService
    {
        private readonly ApplicationDbContext _context;
        private ILogger<SubscriptionService> _logger;
        public AuthMessageSender(ILogger<SubscriptionService> logger, ApplicationDbContext context) {
            _logger = logger;
            _context = context;
        }

        public Subscription GetActiveForUser(string userId) {
            var user = _context.Users.Include(m => m.Subscriptions).ThenInclude(s => s.GroupSubscription).FirstOrDefault(m => m.Id == userId);
            if (user == null) return null;
            
            // TODO Include wildcard subscriptions

            return user.Subscriptions.Where(s => s.StartDate >= DateTime.Now && s.EndDate < DateTime.Now);
        }

        public void Revoke(string subscriptionId) {
            throw new NotImplementedException();
        }
    }

    // Can be in charge of one or many subscriptions
    // Can benefit from one - many wildcard (group) subscriptions

}
