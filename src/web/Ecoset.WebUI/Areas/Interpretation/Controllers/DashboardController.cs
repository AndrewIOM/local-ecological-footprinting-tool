using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Ecoset.WebUI.Models;
using Ecoset.WebUI.Models.DashboardViewModels;
using Ecoset.WebUI.Services.Abstract;
using System.Linq;
using System;

namespace Ecoset.WebUI.Areas.Interpretation.Controllers
{
    [Authorize]
    [Area("Interpretation")]
    public class DashboardController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private IJobService _jobService;
        private ISubscriptionService _subService;

        public DashboardController(UserManager<ApplicationUser> userManager, IJobService jobService, ISubscriptionService subService) {
            _userManager = userManager;
            _jobService = jobService;
            _subService = subService;
        }

        public IActionResult Index()
        {
            var model = new IndexViewModel();
            var user = GetCurrentUserAsync();
            if (user == null) return BadRequest();
            model.UserName = user.FirstName + " " + user.Surname;
            model.Subscription = _subService.GetActiveForUser(user.Id);
            model.DataPackageCount = _jobService.GetAllDataPackagesForUser(user.Id).Where(d => DateTime.Now - d.TimeRequested < TimeSpan.FromDays(7)).Count();
            return View(model);
        }
        
        public IActionResult DataPackages() {
            var user = GetCurrentUserAsync();
            if (user == null) return BadRequest();
            var dps = _jobService.GetAllDataPackagesForUser(user.Id).ToList();
            return View(dps);
        }

        private ApplicationUser GetCurrentUserAsync() => _userManager.GetUserAsync(HttpContext.User).Result; //TODO Assess blocking
    }
}