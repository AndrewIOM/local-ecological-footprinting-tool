using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Ecoset.WebUI.Data;
using Ecoset.WebUI.Models;
using Ecoset.WebUI.Options;
using Ecoset.WebUI.Services.Abstract;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Ecoset.WebUI.Services.Concrete
{
    public class SubscriptionService : ISubscriptionService
    {
        private readonly ApplicationDbContext _context;
        private readonly EcosetAppOptions _appOptions;
        private ILogger<SubscriptionService> _logger;
        public SubscriptionService(ILogger<SubscriptionService> logger, ApplicationDbContext context, IOptions<EcosetAppOptions> appOptions) {
            _logger = logger;
            _context = context;
            _appOptions = appOptions.Value;
        }

        public ActiveSubscription GetActiveForUser(string userId) {
            var user = _context.Users.Include(m => m.Subscriptions).ThenInclude(s => s.GroupSubscriptions).FirstOrDefault(m => m.Id == userId);
            if (user == null) return null;
            var personalSub = user.Subscriptions.AsEnumerable().FirstOrDefault(s => IsActiveSubscription(s));
            if (personalSub != null) {
                return ConvertPersonal(personalSub);
            }
            var glob = _context.GroupSubscriptions.Include(s => s.Subscription)
                .AsEnumerable()
                .FirstOrDefault(m => Regex.IsMatch(user.Email, m.EmailWildcard) && IsActiveSubscription(m.Subscription));
            return ConvertGroup(glob);
        }

        public void Revoke(Guid subscriptionId) {
            var subscription = _context.Subscriptions.FirstOrDefault(m => m.Id == subscriptionId);
            if (subscription != null) {
                subscription.Revoked = true;
                _context.Update(subscription);
                _context.SaveChanges();
            }
        }

        private bool IsActiveSubscription(Subscription s) {
            return s.StartDate >= DateTime.Now 
                && (s.Expires.HasValue ? s.Expires.Value < DateTime.Now : true)
                && (!s.Revoked);
        }

        private ActiveSubscription ConvertGroup(GroupSubscription sub) {
            if (sub == null) return DefaultSubscription();
            return new ActiveSubscription() {
                GroupName = sub.GroupName,
                RateLimit = sub.Subscription.RateLimit,
                AnalysisCap = sub.Subscription.AnalysisCap,
                Expires = sub.Subscription.Expires,
                IsDefault = false
            };
        }

        private ActiveSubscription ConvertPersonal(Subscription sub) {
            return new ActiveSubscription() {
                GroupName = null,
                IsDefault = false,
                RateLimit = sub.RateLimit,
                AnalysisCap = sub.AnalysisCap,
                Expires = sub.Expires
            };
        }

        private ActiveSubscription DefaultSubscription() {
            return new ActiveSubscription() {
                IsDefault = true,
                RateLimit = _appOptions.GlobalRateLimit,
                AnalysisCap = _appOptions.GlobalAnalysisCap,
                Expires = null,
                GroupName = null
            };
        }
    }

}
