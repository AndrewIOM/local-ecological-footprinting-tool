using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ecoset.WebUI.Data;
using Ecoset.WebUI.Models;
using Humanizer;

namespace Ecoset.WebUI.Areas.Interpretation.Controllers
{
    [Authorize]
    [Area("Interpretation")]
    public class NotificationController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        public NotificationController(UserManager<ApplicationUser> userManager, ApplicationDbContext context) 
        {
            _userManager = userManager;
            _context = context;
        }

        public IActionResult Index() {
            return View();
        }

        public IEnumerable<NotificationViewModel> GetRecent(int count) 
        {
            var userId = _userManager.GetUserId(User);
            var notifications = _context.Notifications
                .Include(m => m.User)
                .Include(m => m.Job)
                .Where(m => m.User.Id == userId)
                .OrderByDescending(m => m.Time)
                .ToList();

            var model = notifications.Take(count).Select(n => new NotificationViewModel() {
                Level = n.Level.ToString(),
                Message = string.Format(n.Text, n.TextValues.Split('|')),
                LinkUrl = GetLinkUrl(n),
                FriendlyTime = n.Time.Humanize(),
                Time = n.Time.ToString("{0:g}")
            });
            return model;
        }

        private string GetLinkUrl(Notification n) {
            if (n.Job != null) {
                string url = Url.Action("View", "Job", new { id = n.Job.Id });
                return url;
            }
            return null;
        }
    }

    public class NotificationViewModel {
        public string Level { get; set; }
        public string Message { get; set; }
        public string LinkUrl { get; set; }
        public string FriendlyTime { get; set; }
        public string Time { get; set; }
    }
}
