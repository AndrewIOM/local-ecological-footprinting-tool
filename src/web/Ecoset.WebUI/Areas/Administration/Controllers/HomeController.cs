using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ecoset.WebUI.Data;
using Ecoset.WebUI.Models;
using Ecoset.WebUI.Models.AdminViewModels;
using Ecoset.WebUI.Services.Abstract;
using Ecoset.WebUI.Enums;
using System;

namespace Ecoset.WebUI.Areas.Administration.Controllers
{
    [Area("Administration")]
    [Authorize(Policy = "AdminOnly")]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private IJobService _jobService;
        private INotificationService _notificationService;
        private IJobProcessor _processor;

        public HomeController(
            ApplicationDbContext context, 
            RoleManager<IdentityRole> roleMan,
            UserManager<ApplicationUser> userManager,
            IJobService jobService,
            INotificationService notificationService,
            IJobProcessor processor) {
            _context = context;
            _roleManager = roleMan;
            _userManager = userManager;
            _jobService= jobService;
            _notificationService = notificationService;
            _processor = processor;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult AddSubscription() {
            return View();
        }

        [HttpPost]
        public IActionResult AddSubscription(AddSubscriptionViewModel vm) {
            if (!ModelState.IsValid) {
                return View(vm);
            }

            var user = _context.Users.FirstOrDefault(m => m.Id == vm.MasterUserId);
            if (user == null) {
                ModelState.AddModelError("userId", "The specified master user does not exist");
                return View(vm);
            }

            var subscription = new Subscription() {
                StartDate = vm.StartTime.HasValue ? vm.StartTime.Value : DateTime.Now,
                Expires = vm.ExpiryTime,
                Revoked = false,
                RateLimit = vm.RateLimit,
                AnalysisCap = vm.AnalysisCap,
                PrimaryContact = user,
                GroupSubscriptions = new List<GroupSubscription>()
            };

            // Add subscription
            _context.Subscriptions.Add(subscription);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        public IActionResult AddCredits(string userId, int credits) {
            Contract.Requires(credits > 0 && credits < 50);

            var user = _context.Users.FirstOrDefault(m => m.Id == userId);
            if (user == null) return BadRequest();

            user.Credits += credits;
            _context.SaveChanges();

            _notificationService.AddUserNotification(NotificationLevel.Information, userId, 
                "You have recieved {0} credits.", new string[] { credits.ToString() });

            return Ok();
        }

        public IEnumerable<JobListItemViewModel> GetJobs() {
            var result = _jobService.GetAll();
            var model = result.Select(m => new JobListItemViewModel {
                    Id = m.Id,
                    Name = m.Name,
                    Description = m.Description,
                    SubmittedBy = m.CreatedBy.UserName,
                    Status = m.Status.ToString(),
                    ProStatus = m.ProActivation == null ? "N/A" : m.ProActivation.ProcessingStatus.ToString(),
                    DateAdded = m.DateAdded,
                    DateCompleted = m.DateCompleted
                }).OrderByDescending(m => m.DateAdded);
            return model;
        }

        /// <summary>
        /// Requests that the status for all free analyses is updated using the job service, or a specific job if specified
        /// </summary>
        public IActionResult StatusRefresh(int? jobId) {
            if (jobId.HasValue) {
                _jobService.RefreshJobStatus(jobId.Value);
                return Ok();
            }

            var jobs = _jobService.GetAll();
            foreach (var job in jobs) {
                _jobService.RefreshJobStatus(job.Id);
            }
            return Ok();
        }

        public async Task<IActionResult> RestartJob(int jobId) {
            var job = _jobService.GetById(jobId);
            if (job == null) return BadRequest();

            var submitSuccess = await _jobService.SubmitJob(job);
            if (submitSuccess.HasValue) return Json(new JobListItemViewModel() {
                    Id = job.Id,
                    Name = job.Name,
                    Description = job.Description,
                    SubmittedBy = job.CreatedBy.UserName,
                    Status = job.Status.ToString(),
                    ProStatus = job.ProActivation == null ? "N/A" : job.ProActivation.ProcessingStatus.ToString(),
                    DateAdded = job.DateAdded,
                    DateCompleted = job.DateCompleted
            });

            return BadRequest();
        }

        public async Task<IActionResult> RestartPro(int jobId) {
            var job = _jobService.GetById(jobId);
            if (job == null) return BadRequest();
            var user = GetCurrentUserAsync();

            var submitSuccess = await _jobService.ActivateProFeatures(job.Id, user.Id);
            if (submitSuccess) return Json(new JobListItemViewModel() {
                    Id = job.Id,
                    Name = job.Name,
                    Description = job.Description,
                    SubmittedBy = job.CreatedBy.UserName,
                    ProStatus = job.ProActivation == null ? "N/A" : job.ProActivation.ProcessingStatus.ToString(),
                    Status = job.Status.ToString(),
                    DateAdded = job.DateAdded,
                    DateCompleted = job.DateCompleted
            });

            return BadRequest();
        }

        private ApplicationUser GetCurrentUserAsync() => _userManager.GetUserAsync(HttpContext.User).Result; //TODO Assess blocking       

        public IActionResult StopJob(int jobId) {
            var job = _jobService.GetById(jobId);
            if (job == null) return BadRequest();

            var submitSuccess = _jobService.StopJob(job.Id);
            if (submitSuccess) return Json(new JobListItemViewModel() {
                    Id = job.Id,
                    Name = job.Name,
                    Description = job.Description,
                    SubmittedBy = job.CreatedBy.UserName,
                    ProStatus = job.ProActivation == null ? "N/A" : job.ProActivation.ProcessingStatus.ToString(),
                    Status = job.Status.ToString(),
                    DateAdded = job.DateAdded,
                    DateCompleted = job.DateCompleted
            });

            return BadRequest();
        }

        public IEnumerable<UserListItemViewModel> GetUsers() {
            var model = _context.Users.Include(m => m.Jobs)
                .Include(m => m.Roles)
                .Select(m => new UserListItemViewModel() {
                    Id = m.Id,
                    RegistrationDate = m.RegistrationDate,
                    UserName = m.UserName,
                    ActiveJobs = m.Jobs.Where(j => j.Status == JobStatus.Submitted || j.Status == JobStatus.Processing).Count(),
                    CompleteJobs = m.Jobs.Where(j => j.Status == JobStatus.Completed).Count(),
                    IsAdmin = m.Roles.FirstOrDefault(n => n.RoleId == "Admin") != null,
                    Credits = m.Credits,
                    Organisation = m.OrganisationName,
                    Name = m.FirstName + " " + m.Surname
                }).OrderByDescending(m => m.RegistrationDate).ToList();
            return model;
        }

        [HttpGet]
        public IActionResult PaymentStructure() {
            //Temp
            if (_context.PriceThresholds.Count() == 0) {
                _context.PriceThresholds.Add(new PriceThreshold() {
                    Units = 1,
                    UnitCost = 19.99M
                });
                _context.PriceThresholds.Add(new PriceThreshold() {
                    Units = 5,
                    UnitCost = 17.99M
                });
                _context.PriceThresholds.Add(new PriceThreshold() {
                    Units = 20,
                    UnitCost = 14.99M
                });
                _context.SaveChanges();
            }

            var model = _context.PriceThresholds;
            return View();
        }

        public IActionResult UserAdmin(string id, bool userIsAdmin)
        {

            if (id == _userManager.GetUserId(User)) return BadRequest();

            var user = _context.Users.FirstOrDefault(m => m.Id == id);
            if (user == null) return BadRequest();

            var exists = _roleManager.RoleExistsAsync("Admin").Result;
            if (!exists)
            {
                IdentityRole identityRole = new IdentityRole("Admin");
                IdentityResult identityResult = _roleManager.CreateAsync(identityRole).Result;
            }
            if (userIsAdmin)
            {
                var result = _userManager.AddToRoleAsync(user, "Admin").Result;
            }
            else
            {
                var result = _userManager.RemoveFromRoleAsync(user, "Admin").Result;
            }
            _context.SaveChanges();
            return Ok();
        }

        public IActionResult Subscriptions() {
            return Ok();
        }

    }
}
