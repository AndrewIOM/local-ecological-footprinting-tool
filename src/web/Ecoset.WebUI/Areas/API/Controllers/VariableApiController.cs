using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Ecoset.WebUI.Models;
using Microsoft.Extensions.Options;
using Ecoset.WebUI.Options;
using Ecoset.WebUI.Services.Abstract;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace Ecoset.WebUI.Areas.API.Controllers {

    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Area("Api")]
    [Route("/api/v1/variable")]
    public class VariableApiController : Controller {

        private readonly UserManager<ApplicationUser> _userManager;
        private ReportContentOptions _reportOptions;
        private IJobProcessor _processor;
        private IDataRegistry _registry;

        public VariableApiController (UserManager<ApplicationUser> userManager, 
            IOptions<ReportContentOptions> reportOptions, 
            IJobProcessor processor,
            IDataRegistry registry) {
            _userManager = userManager;
            _reportOptions = reportOptions.Value;
            _processor = processor;
            _registry = registry;
        }

        /// <summary>
        /// Lists available variables from this web service.
        /// </summary>
        [HttpGet]
        [Route("list")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(List<Variable>), StatusCodes.Status200OK)]
        public async Task<IActionResult> List(GeotemporalContext context) {
            var variables = await _registry.GetAvailableVariables();
            return Json(variables);
        }

        /// <summary>
        /// Details of available methods and geo-temporal contexts.
        /// </summary>
        /// <param name="variableName">A unique variable name</param>
        /// <returns>Details of the available variable</returns>
        /// <response code="200">Details of the available variable</response>
        /// <response code="404">The variable is not available from this service</response>     
        [HttpGet("detail/{variableName}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(AvailableVariable), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Detail(string variableName) {
            var variable = await _registry.IsAvailable(variableName, "default");
            if (variable == null) {
                return NotFound("The requested variable " + variableName + " is not available");
            }
            return Json(variable);
        }

        private async Task<ApplicationUser> GetCurrentUserAsync() => await _userManager.GetUserAsync(HttpContext.User);   

    }

}