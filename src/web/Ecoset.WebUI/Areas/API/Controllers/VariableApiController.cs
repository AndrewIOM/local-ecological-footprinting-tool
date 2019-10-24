using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Ecoset.WebUI.Models;
using Microsoft.Extensions.Options;
using Ecoset.WebUI.Options;

namespace Ecoset.WebUI.Areas.API.Controllers {

    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Area("Api")]
    [Route("/api/v1/variable")]
    public class VariableApiController : Controller {

        private readonly UserManager<ApplicationUser> _userManager;
        private ReportContentOptions _reportOptions;
        public VariableApiController (UserManager<ApplicationUser> userManager, IOptions<ReportContentOptions> reportOptions) {
            _userManager = userManager;
            _reportOptions = reportOptions.Value;
        }

        /// <summary>
        /// Lists available variables from this ecoset node.
        /// </summary>
        [HttpGet]
        [Route("list")]
        public IActionResult List(GeotemporalContext context) {
            var sampleVariables = new List<Variable>() {
                new Variable() { Name = "Snow Cover", Description = "Cool" },
                new Variable() { Name = "Air Temperature", Description = "Hot" }};
            return Json(sampleVariables);
        }

        /// <summary>
        /// Lists available variables from this ecoset node.
        /// </summary>
        [HttpGet("/detail/{variableName}")]
        public IActionResult Detail(string variableName) {
            return Json("Some detail about this variable");
        }

        private async Task<ApplicationUser> GetCurrentUserAsync() => await _userManager.GetUserAsync(HttpContext.User);   

    }

}