using Ecoset.WebUI.Services.Abstract;
using Ecoset.WebUI.Enums;
using System;
using Ecoset.WebUI.Data;
using System.Linq;
using Microsoft.Extensions.Logging;
using Ecoset.WebUI.Models;
using Microsoft.EntityFrameworkCore;

namespace Ecoset.WebUI.Services.Concrete
{
    public class NotificationService : INotificationService
    {
        private readonly ApplicationDbContext _context;
        private ILogger<NotificationService> _logger;

        public NotificationService(ApplicationDbContext context, ILogger<NotificationService> logger) {
            _context = context;
            _logger = logger;
        }

        public void AddJobNotification(NotificationLevel level, int jobId, string text, string[] textValues)
        {
            if (!ValidMessage(text, textValues)) {
                _logger.LogError("Attempted to add invalid notification. " + text);
                return;
            }

            var job = _context.Jobs.Include(m => m.CreatedBy).FirstOrDefault(j => j.Id == jobId);
            if (job == null) {
                _logger.LogError("Attempted to add a notification to a job that doesn't exist. Job id: " + jobId);
                return;
            }

            var notification = new Notification() 
            {
                Hidden = false,
                Job = job,
                Level = level,
                Text = text,
                TextValues = ConvertValuesToStore(textValues),
                User = job.CreatedBy,
                Time = DateTime.UtcNow,
            };
            _context.Add(notification);
            _context.SaveChanges();
        }

        public void AddUserNotification(NotificationLevel level, string userId, string text, string[] textValues)
        {
            if (!ValidMessage(text, textValues)) {
                _logger.LogError("Attempted to add invalid notification. " + text);
                return;
            }

            var user = _context.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null) {
                _logger.LogError("Attempted to add a notification to a user that doesn't exist. User id: " + userId);
                return;
            }

            var notification = new Notification() 
            {
                Hidden = false,
                Job = null,
                Level = level,
                Text = text,
                TextValues = ConvertValuesToStore(textValues),
                User = user,
                Time = DateTime.UtcNow
            };
            _context.Add(notification);
            _context.SaveChanges();
        }
        
        public void MarkAsRead(Guid notificationId)
        {
            var notification = _context.Notifications.FirstOrDefault(n => n.Id == notificationId);
            if (notification == null) {
                _logger.LogError("Notification " + notificationId + " does not exist, and cannot be marked as read.");
                return;
            }
            notification.Hidden = true;
            _context.Update(notification);
            _context.SaveChanges();
        }

        private bool ValidMessage(string text, object[] values) {
            try {
                string.Format(text, values);
                return true;
            } catch {
                return false;
            }
        }

        private string ConvertValuesToStore(string[] values) {
            var result = "";
            if (values.Length == 0) return result;
            foreach (var v in values) 
            {
                result = result + v + "|";
            }
            result = result.Substring(0, result.Length - 1);
            return result;
        }

        private string[] ConvertStoreToValues(string storeValue) {
            return storeValue.Split('|');
        }
    }
}
