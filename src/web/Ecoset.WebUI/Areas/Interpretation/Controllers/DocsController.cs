using Microsoft.AspNetCore.Mvc;

namespace Ecoset.WebUI.Areas.Interpretation.Controllers
{
    [Area("Interpretation")]
    public class DocsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
