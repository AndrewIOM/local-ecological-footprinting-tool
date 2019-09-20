using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Ecoset.WebUI.Models;
using Ecoset.WebUI.Models.DashboardViewModels;

namespace Ecoset.WebUI.Areas.Interpretation.Controllers
{
    [Authorize]
    [Area("Interpretation")]
    public class DashboardController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public DashboardController(UserManager<ApplicationUser> userManager) {
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            var model = new IndexViewModel();
            var user = GetCurrentUserAsync();
            if (user == null) return BadRequest();
            model.UserName = user.FirstName + " " + user.Surname;
            return View(model);
        }
        
        public IActionResult DataPackages() {
            return View();
        }

        private ApplicationUser GetCurrentUserAsync() => _userManager.GetUserAsync(HttpContext.User).Result; //TODO Assess blocking
    }
}