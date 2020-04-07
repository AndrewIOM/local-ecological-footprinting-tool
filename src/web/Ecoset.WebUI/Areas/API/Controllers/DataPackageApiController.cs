using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Ecoset.WebUI.Models;
using Ecoset.WebUI.Models.JobViewModels;
using Ecoset.WebUI.Services.Abstract;
using System.Linq;
using Microsoft.Extensions.Options;
using Ecoset.WebUI.Options;
using System.Security.Claims;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace Ecoset.WebUI.Areas.API.Controllers {

    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Area("Api")]
    [Route("/api/v1/package")]
    public class DataPackageApiController : Controller {

        private readonly IJobService _jobService;
        private readonly UserManager<ApplicationUser> _userManager;
        private IOutputPersistence _persistence;
        private ReportContentOptions _reportOptions;
        private ISubscriptionService _subService;
        private ILogger<DataPackageApiController> _logger;
        private IDataRegistry _dataRegistry;
        private EcosetAppOptions _options;

        public DataPackageApiController (
            IJobService jobService, 
            UserManager<ApplicationUser> userManager, 
            IOptions<ReportContentOptions> reportOptions,
            IOutputPersistence persistence,
            INotificationService notifyService,
            ILogger<DataPackageApiController> logger,
            IDataRegistry dataRegistry,
            ISubscriptionService subService,
            IOptions<EcosetAppOptions> options) {
            _jobService = jobService;
            _userManager = userManager;
            _persistence = persistence;
            _reportOptions = reportOptions.Value;
            _subService = subService;
            _logger = logger;
            _dataRegistry = dataRegistry;
            _options = options.Value;
        }

        /// <summary>
        /// Create a data package for a given place and time.
        /// </summary>
        /// <param name="model"></param>        
        [HttpPost]
        [Route("submit")]
        public async Task<IActionResult> Submit([FromBody] DataRequest request) {
            var variablesToRun = new List<AvailableVariable>();
            foreach (var variable in request.Variables) {
                var available = await _dataRegistry.IsAvailable(variable.Name, variable.Method);
                if (available == null) {
                    ModelState.AddModelError("Variables", "The variable " + variable.Name + " is not available");
                } else {
                    variablesToRun.Add(available);
                }
            }
            if (!ModelState.IsValid) {
                return BadRequest(ModelState);
            }

            if (!String.IsNullOrEmpty(_options.ValidAreaGeoJsonFile)) {
                var fileInfo = Utils.Files.GetFileProvider(HttpContext.RequestServices).GetFileInfo(_options.ValidAreaGeoJsonFile);
                if (fileInfo.Exists && fileInfo.Name.EndsWith(".json")) {
                    var intersectsMask = Utils.GeoJson.BoxIntersects(fileInfo.PhysicalPath,
                        request.LatitudeNorth.Value, request.LatitudeSouth.Value, request.LongitudeEast.Value, request.LongitudeWest.Value);
                    if (!intersectsMask) {
                        ModelState.AddModelError("boundingbox", "Analyses are not available in the selected area");
                        return BadRequest(ModelState);
                    }
                } else {
                    _logger.LogError("Specified geojson mask to validate analyses did not exist");
                }
            }

            var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByNameAsync(userId.Value);
            if (!_subService.HasProcessingCapacity(user.Id)) {
                return BadRequest("You have reache the limited of your current subscription");
            }

            var businessModel = new DataPackage() {
                        LatitudeSouth = request.LatitudeSouth.Value,
                        LatitudeNorth = request.LatitudeNorth.Value,
                        LongitudeEast = request.LongitudeEast.Value,
                        LongitudeWest = request.LongitudeWest.Value,
                        CreatedBy = user,
                        TimeRequested = DateTime.UtcNow,
                        DataRequestedTime = request.DateMode,
                        Year = request.Date == null ? null : new Nullable<int>(request.Date.Year),
                        Month = request.Date == null ? null : request.Date.Month,
                        Day = request.Date == null ? null : request.Date.Month
            };
            var packageId = await _jobService.SubmitDataPackage(businessModel, variablesToRun);
            if (!packageId.HasValue) {
                return StatusCode(500);
            }
            return CreatedAtAction(nameof(DataPackage), new { id =  packageId });
        }

        /// <summary>
        /// Gets the details of a data package, including the status of any
        /// data generation processes.
        /// </summary>
        [HttpGet]
        [Route("status")]
        public async Task<IActionResult> Status(Guid id) {
            var dp = await _jobService.GetDataPackage(id);
            if (dp == null) return NotFound("The data package does not exist");
            var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            if (dp.CreatedBy.Id != userId) return NotFound("The data package does not exist");
            return Json(dp.Status);
        }

        /// <summary>
        /// Stream the contents of the data package.
        /// </summary>
        [HttpGet]
        [Route("fetch")]
        public async Task<IActionResult> Fetch(Guid id) 
        {
            var dp = await _jobService.GetDataPackage(id);
            if (dp == null) return NotFound("The data package does not exist");

            var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByNameAsync(userId.Value);
            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
            if (!isAdmin && dp.CreatedBy.Id != user.Id) return NotFound("The data package does not exist");
            if (dp.Status != JobStatus.Completed) return BadRequest("This analysis has not yet completed");

            var contents = await _jobService.GetDataPackageData(id);
            return Json(contents);
        }

    }

}