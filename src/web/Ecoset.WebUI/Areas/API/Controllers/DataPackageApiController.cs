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

namespace Ecoset.WebUI.Areas.API.Controllers {

    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Area("Api")]
    [Route("/api/v1/data")]
    public class DataPackageApiController : Controller {

        private readonly IJobService _jobService;
        private readonly UserManager<ApplicationUser> _userManager;
        private IOutputPersistence _persistence;
        private ReportContentOptions _reportOptions;
        public DataPackageApiController (
            IJobService jobService, 
            UserManager<ApplicationUser> userManager, 
            IOptions<ReportContentOptions> reportOptions,
            IOutputPersistence persistence,
            INotificationService notifyService) {
            _jobService = jobService;
            _userManager = userManager;
            _persistence = persistence;
            _reportOptions = reportOptions.Value;
        }

        /// <summary>
        /// Create a data package for a given place and time.
        /// </summary>
        /// <param name="model"></param>        
        [HttpPost]
        [Route("submit")]
        public async Task<IActionResult> Submit(DataRequest request) {

            foreach (var variable in request.Variables) {
                if (_reportOptions.ProReportSections.FirstOrDefault(m => m.Name == variable) == null) {
                    ModelState.AddModelError("Variables", "The variable " + variable + " is not available");
                }
            }

            if (!ModelState.IsValid) {
                return BadRequest(ModelState);
            }

            var user = await GetCurrentUserAsync();
            var activePackages = _jobService.GetAllDataPackagesForUser(user.Id).ToList();
            if (activePackages.Count > 0) {
                return BadRequest("Your account is limited to one data package computation at a time");
            }

            var businessModel = new DataPackage() {
                        Name = request.Name,
                        LatitudeSouth = request.LatitudeSouth.Value,
                        LatitudeNorth = request.LatitudeNorth.Value,
                        LongitudeEast = request.LongitudeEast.Value,
                        LongitudeWest = request.LongitudeWest.Value,
                        CreatedBy = user,
                        TimeRequested = DateTime.UtcNow
            };
            var packageId = await _jobService.SubmitDataPackage(businessModel, request.Variables);
            return CreatedAtAction(nameof(DataPackage), new { id =  packageId });
        }

        /// <summary>
        /// Gets the details of a data package, including the status of any
        /// data generation processes.
        /// </summary>
        /// <param name="model"></param>
        [HttpGet]
        [Route("package")]
        public async Task<IActionResult> DataPackage(Guid id) {
            var dp = await _jobService.GetDataPackage(id);
            if (dp == null) return NotFound("The data package does not exist");
            var userId = _userManager.GetUserId(HttpContext.User);
            if (dp.CreatedBy.Id != userId) return NotFound("The data package does not exist");
            return Json(dp); // TODO create a DTO for here.
        }

        /// <summary>
        /// Stream the contents of the data package.
        /// </summary>
        /// <param name="model"></param>      
        [HttpGet]
        [Route("retrieve")]
        public async Task<IActionResult> Retrieve(Guid id) 
        {
            var job = await _jobService.GetDataPackage(id);
            if (job == null) return NotFound("The data package does not exist");

            var user = await GetCurrentUserAsync();
            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
            if (!isAdmin && job.CreatedBy.Id != user.Id) return NotFound("The data package does not exist");
            if (job.Status != JobStatus.Completed) return BadRequest("This analysis has not yet completed");

            var contents = _jobService.GetDataPackageData(id);
            return Json(contents);
        }

        /// <summary>
        /// Retrieve a zipped archive of the data package.
        /// </summary>
        /// <param name="id">A Data Package identifier</param>      
        [HttpGet]
        [Route("download")]
        public IActionResult Download(Guid id) 
        {
            return BadRequest("This function has not been implemented in the preview");
            // var job = _jobService.GetById(id);
            // if (job == null) return BadRequest();
            // if (job.Status != JobStatus.Completed) return BadRequest();

            // var user = await GetCurrentUserAsync();
            // var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
            // if (!isAdmin && job.CreatedBy.Id != user.Id) return BadRequest();

            // var file = _persistence.GetProData(id);
            // if (string.IsNullOrEmpty(file)) return NotFound();
            // return PhysicalFile(file, "application/zip");
        }

        private async Task<ApplicationUser> GetCurrentUserAsync() => await _userManager.GetUserAsync(HttpContext.User);   

    }

}