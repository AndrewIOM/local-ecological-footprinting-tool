using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Ecoset.WebUI.Models;
using Ecoset.WebUI.Models.JobViewModels;
using Ecoset.WebUI.Services.Abstract;

namespace Ecoset.WebUI.Areas.API.Controllers {

    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Area("Api")]
    [Route("/api/v1/data")]
    public class DataPackageApiController : Controller {

        private readonly IJobService _jobService;
        private readonly UserManager<ApplicationUser> _userManager;
        private IOutputPersistence _persistence;
        public DataPackageApiController (
            IJobService jobService, 
            UserManager<ApplicationUser> userManager, 
            IOutputPersistence persistence,
            INotificationService notifyService) {
            _jobService = jobService;
            _userManager = userManager;
            _persistence = persistence;
        }

        /// <summary>
        /// Create a data package for a given place and time.
        /// </summary>
        /// <param name="model"></param>        
        [HttpPost]
        [Route("submit")]
        public async Task<IActionResult> Submit(AddJobViewModel request) {

            if (!ModelState.IsValid) {
                return BadRequest(ModelState);
            }

            var user = await GetCurrentUserAsync();
            var businessModel = new Job() {
                        Name = request.Name,
                        Description = request.Description,
                        LatitudeSouth = request.LatitudeSouth.Value,
                        LatitudeNorth = request.LatitudeNorth.Value,
                        LongitudeEast = request.LongitudeEast.Value,
                        LongitudeWest = request.LongitudeWest.Value,
                        CreatedBy = user,
                        DateAdded = DateTime.Now
            };
            var jobId = _jobService.SubmitJob(businessModel);
            if (!jobId.HasValue) {
                return StatusCode(500, "There was an issue submitting your analysis. Please try again, or report an issue if the problem persists.");
            }
            var success = _jobService.ActivateProFeatures(jobId.Value, user.Id);
            return CreatedAtAction(nameof(DataPackage), new { id =  jobId.Value });
        }

        /// <summary>
        /// Gets the details of a data package, including the status of any
        /// data generation processes.
        /// </summary>
        /// <param name="model"></param>
        [HttpGet]
        [Route("package")]
        public IActionResult DataPackage(int id) {
            var job = _jobService.GetById(id);
            if (job == null) return BadRequest();
            var userId = _userManager.GetUserId(HttpContext.User);
            if (job.CreatedBy.Id != userId) return BadRequest();
            return Json(job);
        }

        /// <summary>
        /// Gets the details of a data package, including the status of any
        /// data generation processes.
        /// </summary>
        /// <param name="model"></param>      
        [HttpGet]
        [Route("retrieve")]
        public async Task<IActionResult> Download(int id) 
        {
            var job = _jobService.GetById(id);
            if (job == null) return BadRequest();
            if (job.Status != JobStatus.Completed) return BadRequest();

            var user = await GetCurrentUserAsync();
            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
            if (!isAdmin && job.CreatedBy.Id != user.Id) return BadRequest();

            var file = _persistence.GetProData(id);
            if (string.IsNullOrEmpty(file)) return NotFound();
            return PhysicalFile(file, "application/zip");
        }

        private async Task<ApplicationUser> GetCurrentUserAsync() => await _userManager.GetUserAsync(HttpContext.User);   

    }

}