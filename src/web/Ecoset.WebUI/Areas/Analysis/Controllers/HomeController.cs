using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Ecoset.WebUI.Models;
using Ecoset.WebUI.Services.Abstract;
using System.Threading.Tasks;
using Ecoset.WebUI.Models.JobViewModels;

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
        public HomeController(
            IJobService jobService, 
            UserManager<ApplicationUser> userManager, 
            IReportGenerator reportGenerator,
            IOutputPersistence persistence,
            INotificationService notifyService) {
            _jobService = jobService;
            _userManager = userManager;
            _notifyService = notifyService;
            _persistence = persistence;
            _reportGenerator = reportGenerator;
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
        public IActionResult ActivateProData(int id)
        {
            var job = _jobService.GetById(id);
            if (job == null) return BadRequest();

            if (job.ProActivation != null)
            {
                return BadRequest("The analysis specified has already been activated with pro data.");
            }

            var user = GetCurrentUserAsync();
            if (job.CreatedBy.Id != user.Id) return BadRequest();

            var success = _jobService.ActivateProFeatures(id, user.Id);
            if (!success)
            {
                return BadRequest("The activiation was not successful. Do you have at least one credit?");
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
        public IActionResult Submit(AddJobViewModel result) {

            if (!ModelState.IsValid) {
                return View(result);
            }

            var businessModel = new Job() {
                        Name = result.Name,
                        Description = result.Description,
                        LatitudeSouth = result.LatitudeSouth.Value,
                        LatitudeNorth = result.LatitudeNorth.Value,
                        LongitudeEast = result.LongitudeEast.Value,
                        LongitudeWest = result.LongitudeWest.Value,
                        CreatedBy = GetCurrentUserAsync(),
                        DateAdded = DateTime.Now
            };
            var jobSuccess = _jobService.SubmitJob(businessModel);

            if (!jobSuccess.HasValue) {
                ModelState.AddModelError("Service", "There was an issue submitting your analysis. Please try again, or report an issue if the problem persists.");
                return View(result);
            }

            ViewData["Message"] = "Successfully submitted your analysis";
            return RedirectToAction("Index");
        }

        public IEnumerable<JobListItemViewModel> MyJobs() {
            var result = _jobService.GetAllForUser(GetCurrentUserAsync().Id)
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
            Console.WriteLine("Creating PDF Report for " + id);
            var data = _jobService.GetReportData(id);
            return View(data);
        }

        public async Task<IActionResult> Report(int id)
        {
            var job = _jobService.GetById(id);
            if (job == null) return BadRequest();
            if (job.Status != JobStatus.Completed) return BadRequest();

            var user = GetCurrentUserAsync();
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

            var user = GetCurrentUserAsync();
            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
            if (!isAdmin && job.CreatedBy.Id != user.Id) return BadRequest();

            var file = _persistence.GetProData(id);
            if (string.IsNullOrEmpty(file)) return NotFound();
            return PhysicalFile(file, "application/zip");
        }

        private ApplicationUser GetCurrentUserAsync() => _userManager.GetUserAsync(HttpContext.User).Result; //TODO Assess blocking       
    }
}
