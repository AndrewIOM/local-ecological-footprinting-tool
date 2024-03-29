using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Ecoset.WebUI.Models;
using Ecoset.WebUI.Services.Abstract;
using Ecoset.WebUI.Models.JobViewModels;
using Ecoset.WebUI.Options;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.Builder;

namespace Ecoset.WebUI.Areas.Analysis.Controllers
{
    [Authorize]
    [Area("Analysis")]
    public class HomeController : Controller
    {
        private readonly IJobService _jobService;
        private readonly UserManager<ApplicationUser> _userManager;
        private INotificationService _notifyService;
        private IOutputPersistence _persistence;
        private IReportGenerator _reportGenerator;
        private EcosetAppOptions _options;
        private ISubscriptionService _subService;
        private ILogger _logger;

        public HomeController(
            IJobService jobService, 
            UserManager<ApplicationUser> userManager, 
            IReportGenerator reportGenerator,
            IOutputPersistence persistence,
            IOptions<EcosetAppOptions> options,
            INotificationService notifyService,
            ISubscriptionService subService,
            ILogger<HomeController> logger) {
            _jobService = jobService;
            _userManager = userManager;
            _notifyService = notifyService;
            _persistence = persistence;
            _reportGenerator = reportGenerator;
            _options = options.Value;
            _subService = subService;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IEnumerable<JobListItemViewModel> Get() {
            var result = _jobService.GetAllForUser(_userManager.GetUserId(User))
                .Select(m => new JobListItemViewModel() {
                    Id = m.Id,
                    DateAdded = m.DateAdded,
                    DateCompleted = m.DateCompleted,
                    Name = m.Name,
                    Description = m.Description,
                    SubmittedBy = m.CreatedBy.FirstName + " " + m.CreatedBy.Surname,
                    Status = m.Status.ToString(),
                    LatitudeNorth = m.LatitudeNorth,
                    LatitudeSouth = m.LatitudeSouth,
                    LongitudeEast = m.LongitudeEast,
                    LongitudeWest = m.LongitudeWest
                }).ToList();
            return result;
        }

        public IActionResult Count(JobStatus status) {
            var userId = _userManager.GetUserId(HttpContext.User);
            var result = _jobService.GetAllForUser(userId)
                .Where(m => m.Status == status).Count();
            return Ok(result);
        }

        public IActionResult View(int id) 
        {
            var job = _jobService.GetById(id);
            if (job == null) return BadRequest();

            var userId = _userManager.GetUserId(HttpContext.User);
            if (job.CreatedBy.Id != userId) return BadRequest();
            
            return View(job);
        }

        [HttpPost]
        public IActionResult Hide(int id)
        {
            var job = _jobService.GetById(id);
            if (job == null) return BadRequest();

            var userId = _userManager.GetUserId(HttpContext.User);
            if (job.CreatedBy.Id != userId) return BadRequest();
            var success = _jobService.HideJob(job.Id);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> ActivateProData(int id)
        {
            var job = _jobService.GetById(id);
            if (job == null) return BadRequest();

            if (job.ProActivation != null)
            {
                return BadRequest("The analysis specified has already been activated with pro data.");
            }

            var user = await GetCurrentUserAsync();
            if (job.CreatedBy.Id != user.Id) return BadRequest();

            var success = await _jobService.ActivateProFeatures(id, user.Id);
            if (!success)
            {
                return BadRequest("The activiation was not successful.");
            }

            return RedirectToAction("View", "Home", new { area = "Analysis", id = id });
        }

        [HttpGet]
        public IActionResult Submit() {
            var model = new AddJobViewModel();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(AddJobViewModel result) {

            if (!ModelState.IsValid) {
                return View(result);
            }

            if (!String.IsNullOrEmpty(_options.ValidAreaGeoJsonFile)) {
                var fileInfo = Utils.Files.GetFileProvider(HttpContext.RequestServices).GetFileInfo(_options.ValidAreaGeoJsonFile);
                if (fileInfo.Exists && fileInfo.Name.EndsWith(".json")) {
                    var intersectsMask = Utils.GeoJson.BoxIntersects(fileInfo.PhysicalPath,
                        result.LatitudeNorth.Value, result.LatitudeSouth.Value, result.LongitudeEast.Value, result.LongitudeWest.Value);
                    if (!intersectsMask) {
                        ModelState.AddModelError("boundingbox", "Analyses are not available in the selected area");
                        return View(result);
                    }
                } else {
                    _logger.LogError("Specified geojson mask to validate analyses did not exist");
                }
            }

            var user = await GetCurrentUserAsync();
            if (!_subService.HasProcessingCapacity(user.Id)) {
                ModelState.AddModelError("subscription", "You have reached the limits of your current subscription");
                return View(result);
            }

            var businessModel = new Job() {
                        Name = result.Name,
                        Description = result.Description,
                        LatitudeSouth = result.LatitudeSouth.Value,
                        LatitudeNorth = result.LatitudeNorth.Value,
                        LongitudeEast = result.LongitudeEast.Value,
                        LongitudeWest = result.LongitudeWest.Value,
                        CreatedBy = user,
                        DateAdded = DateTime.Now
            };
            var jobSuccess = await _jobService.SubmitJob(businessModel);

            if (!jobSuccess.HasValue) {
                ModelState.AddModelError("Service", "There was an issue submitting your analysis. Please try again, or report an issue if the problem persists.");
                return View(result);
            }

            ViewData["Message"] = "Successfully submitted your analysis";
            return RedirectToAction("Index");
        }

        public async Task<IEnumerable<JobListItemViewModel>> MyJobs() {
            var user = await GetCurrentUserAsync();
            var result = _jobService.GetAllForUser(user.Id)
                .Select(m => new JobListItemViewModel() {
                    Name = m.Name,
                    LatitudeNorth = m.LatitudeNorth,
                    LatitudeSouth = m.LatitudeSouth,
                    LongitudeEast = m.LongitudeEast,
                    LongitudeWest = m.LongitudeWest,
                    Status = m.Status.ToString(),
                    Id = m.Id,
                    DateAdded = m.DateAdded
                }).OrderByDescending(m => m.DateAdded);
                return result;
        }

        [AllowAnonymous]
        public IActionResult GenerateReport(int id)
        {
            _logger.LogInformation("Executing razor view for PDF Report (#" + id + ")");
            var data = _jobService.GetReportData(id);
            return View(data);
        }

        public async Task<IActionResult> Report(int id)
        {
            var job = _jobService.GetById(id);
            if (job == null) return BadRequest();
            if (job.Status != JobStatus.Completed && job.Status != JobStatus.GeneratingOutput) return BadRequest();

            var user = await GetCurrentUserAsync();
            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
            if (!isAdmin && job.CreatedBy.Id != user.Id) return BadRequest();

            var reportFilename = _reportGenerator.GenerateReport(job);
            if (string.IsNullOrEmpty(reportFilename)) return NotFound();
            return PhysicalFile(reportFilename, "application/pdf");
        }

        public async Task<IActionResult> ProData(int id) 
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
