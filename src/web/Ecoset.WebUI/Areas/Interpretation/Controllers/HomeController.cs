using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Ecoset.WebUI.Models.HomeViewModels;
using Ecoset.WebUI.Options;
using Ecoset.WebUI.Services.Abstract;

namespace Ecoset.WebUI.Areas.Interpretation.Controllers
{
    [Area("Interpretation")]
    public class HomeController : Controller
    {
        private IEmailSender _emailSender;
        private EmailOptions _options;
        public HomeController(IEmailSender emailSender, IOptions<EmailOptions> options) {
            _emailSender = emailSender;
            _options = options.Value;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult Contact()
        {
            var model = new ContactFormViewModel();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Contact(ContactFormViewModel result)
        {
            if (!ModelState.IsValid) return View(result);
            var messageContent = string.Format("Enquiry from the LEFT website, from {0} ({1}). {2}", result.Name, result.Email, result.Message);
            await _emailSender.SendEmailAsync(_options.FromAddress, "Enquiry from website: " + result.Name, messageContent);
            return View("ContactSuccess");
        }

        public IActionResult Errors(string id) 
        { 
            if (id == "500" | id == "404") 
            { 
                return View($"~/Areas/Interpretation/Views/Home/Error/{id}.cshtml"); 
            }

            return View("~/Views/Shared/Error.cshtml"); 
        }

        public IActionResult Pricing() {
            return View();
        }

        public IActionResult HowItWorks() {
            return View();
        }

        public IActionResult Terms() {
            return View();
        }

        public IActionResult Privacy() {
            return View();
        }
    }
}
