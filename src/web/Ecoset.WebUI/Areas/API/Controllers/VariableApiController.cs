using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Ecoset.WebUI.Models;
using Microsoft.Extensions.Options;
using Ecoset.WebUI.Options;
using Ecoset.WebUI.Services.Abstract;

namespace Ecoset.WebUI.Areas.API.Controllers {

    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Area("Api")]
    [Route("/api/v1/variable")]
    public class VariableApiController : Controller {

        private readonly UserManager<ApplicationUser> _userManager;
        private ReportContentOptions _reportOptions;
        private IJobProcessor _processor;
        public VariableApiController (UserManager<ApplicationUser> userManager, IOptions<ReportContentOptions> reportOptions, IJobProcessor processor) {
            _userManager = userManager;
            _reportOptions = reportOptions.Value;
            _processor = processor;
        }

        /// <summary>
        /// Lists available variables from this ecoset node.
        /// </summary>
        [HttpGet]
        [Route("list")]
        public async Task<IActionResult> List(GeotemporalContext context) {
            var variables = await _processor.ListVariables();
            return Json(variables);
        }

        /// <summary>
        /// Details of available methods and geo-temporal contexts.
        /// </summary>
        [HttpGet("/detail/{variableName}")]
        public IActionResult Detail(string variableName) {
            return Json("Some detail about this variable");
        }

        private async Task<ApplicationUser> GetCurrentUserAsync() => await _userManager.GetUserAsync(HttpContext.User);   

    }

}