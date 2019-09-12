using System;
using Ecoset.WebUI.Enums;

namespace Ecoset.WebUI.Services.Abstract
{
    public interface INotificationService
    {
        void AddJobNotification(NotificationLevel level, int jobId, string text, string[] textValues);
        void AddUserNotification(NotificationLevel level, string userId, string text, string[] textValues);
        void MarkAsRead(Guid notificationId);
    }
}