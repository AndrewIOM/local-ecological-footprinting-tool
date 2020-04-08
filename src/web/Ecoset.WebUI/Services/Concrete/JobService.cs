using Ecoset.WebUI.Services.Abstract;
using Ecoset.WebUI.Models;
using System.Collections.Generic;
using Ecoset.WebUI.Data;
using System.Linq;
using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Ecoset.WebUI.Models.JobProcessor;
using Ecoset.WebUI.Enums;
using Microsoft.AspNetCore.Identity;
using Hangfire;
using Ecoset.WebUI.Options;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Ecoset.WebUI.Models.JobViewModels;

namespace Ecoset.WebUI.Services.Concrete
{
    public class JobService : IJobService
    {
        private readonly ApplicationDbContext _context;
        private IJobProcessor _processor;
        private INotificationService _notifyService;
        private IEmailSender _emailSender;
        private IReportGenerator _reportGenerator;
        private IOutputPersistence _outputPersistence;
        private readonly EcosetAppOptions _appOptions;
        private readonly UserManager<ApplicationUser> _userManager;
        private ILogger<JobService> _logger;
        private IDataRegistry _registry;
        public JobService(ApplicationDbContext context, 
            IJobProcessor processor, 
            INotificationService notifyService, 
            IEmailSender emailSender, 
            IReportGenerator reportGenerator,
            IOutputPersistence outputPersistence,
            UserManager<ApplicationUser> userManager,
            IOptions<EcosetAppOptions> appOptions,
            IDataRegistry registry,
            ILogger<JobService> logger) {
            _context = context;
            _processor = processor;
            _notifyService = notifyService;
            _emailSender = emailSender;
            _outputPersistence = outputPersistence;
            _reportGenerator = reportGenerator;
            _userManager = userManager;
            _appOptions = appOptions.Value;
            _registry = registry;
            _logger = logger;
        }

        public IEnumerable<Job> GetAll()
        {
            var jobs = _context.Jobs.Include(m => m.CreatedBy).Include(m => m.ProActivation).Where(m => !m.Hidden).ToList();
            var result = new List<Job>();
            foreach (var job in jobs) {
                result.Add(job);
            }

            _context.SaveChanges();
            return result;
        }

        public IEnumerable<Job> GetAllForUser(string userId)
        {
            var user = _context.Users.Include(m => m.Jobs).ThenInclude(n => n.CreatedBy).FirstOrDefault(m => m.Id == userId);
            var result = new List<Job>();
            if (user == null) return result;
            foreach (var job in user.Jobs) {
                if (!job.Hidden) result.Add(job);
            }
            return result;
        }

        public Job GetById(int jobId)
        {
            var result = _context.Jobs
                .Include(m => m.CreatedBy)
                .Include(m => m.Notifications)
                .Include(m => m.ProActivation)
                .FirstOrDefault(m => m.Id == jobId);
            _context.SaveChanges();            
            return result;
        }

        public bool StopJob(int jobId)
        {
            var job = _context.Jobs.FirstOrDefault(m => m.Id == jobId);
            if (job == null) return false;

            _processor.StopJob(job.JobProcessorReference);
            Hangfire.RecurringJob.RemoveIfExists("jobstatus_" + job.Id);
            return true;
        }

        public bool HideJob(int jobId)
        {
            var job = _context.Jobs.FirstOrDefault(m => m.Id == jobId);
            if (job == null) return false;
            job.Hidden = true;
            _context.Jobs.Update(job);
            _context.SaveChanges();
            return true;
        }

        public async Task<int?> SubmitJob(Job job) {
            var request = new JobSubmission() {
                Title = job.Name,
            	Description = job.Description,
            	UserName = job.CreatedBy.UserName,
                East = job.LongitudeEast,
                West = job.LongitudeWest,
                North = job.LatitudeNorth,
                South = job.LatitudeSouth,
                Priority = 1
            };
            var processorReference = await _processor.StartJob(request);
            if (String.IsNullOrEmpty(processorReference)) {
                return null;
            }

            job.JobProcessorReference = processorReference;
            if (job.Id != 0) {
                //Job is a resubmission. Use old database record
                job.Status = JobStatus.Submitted;
                _context.Update(job);
            } else {
                //New job submission
                _context.Jobs.Add(job);
            }
            _context.SaveChanges();

            var savedJob = _context.Jobs.First(j => j.JobProcessorReference == job.JobProcessorReference);
            _notifyService.AddJobNotification(NotificationLevel.Success, savedJob.Id, "Analysis {0} successfully queued for processing.", new[] { job.Name });
        
            Hangfire.RecurringJob.AddOrUpdate("jobstatus_" + savedJob.Id, () => UpdateJobStatusAsync(savedJob.Id), Cron.Minutely);

            return savedJob.Id;
        }

        public void RefreshJobStatus(int jobId) {
            var job = _context.Jobs.FirstOrDefault(m => m.Id == jobId);
            if (job == null) return;
            Hangfire.RecurringJob.AddOrUpdate("jobstatus_" + job.Id, () => UpdateJobStatusAsync(job.Id), Cron.Minutely);
        }

        public async Task UpdateJobStatusAsync(int jobId) {
            Console.WriteLine("Updating job status for " + jobId);
            var result = _context.Jobs
                .Include(m => m.CreatedBy)
                .Include(m => m.Notifications)
                .Include(m => m.ProActivation)
                .FirstOrDefault(m => m.Id == jobId);
            if (result != null) {
                await UpdateJobStatusAsync(result);
            }
        }

        public async Task UpdateProStatusAsync(int jobId) {
            Console.WriteLine("Updating pro status for " + jobId);
            var result = _context.Jobs
                .Include(m => m.CreatedBy)
                .Include(m => m.Notifications)
                .Include(m => m.ProActivation)
                .FirstOrDefault(m => m.Id == jobId);
            if (result != null) {
                if (result.ProActivation != null) {
                    await UpdateProStatusAsync(result);
                }
            }
        }

        public async Task UpdatePackageStatusAsync(Guid id) {
            _logger.LogInformation("Determining current status of data package: " + id);
            var result = _context.DataPackages
                .FirstOrDefault(m => m.Id == id);
            if (result != null) {
                await UpdatePackageStatusAsync(result);
            }
        }

        private async Task UpdatePackageStatusAsync(DataPackage package) {
            var newStatus = await _processor.GetStatus(package.JobProcessorReference, package.Status);
            if (newStatus != package.Status) {
                package.Status = newStatus;
                if (package.Status == JobStatus.Completed || package.Status == JobStatus.Failed) {
                    package.TimeCompleted = DateTime.Now;
                }
                _context.Update(package);
                _context.SaveChanges();
            }
            if (package.Status == JobStatus.Failed || package.Status == JobStatus.Completed) {
                Hangfire.RecurringJob.RemoveIfExists("prostatus_" + package.Id);
            }
        }

        private async Task UpdateJobStatusAsync(Job job) {
            
            if (job.Status == JobStatus.GeneratingOutput) {
                Console.WriteLine("Output still generating for: " + job.Id);
                return;
            }
            
            var newStatus = await _processor.GetStatus(job.JobProcessorReference, job.Status);

            // Outcome 1: Start generating report
            if (newStatus == JobStatus.Completed && job.Status != JobStatus.Completed) {
                job.Status = JobStatus.GeneratingOutput;
                _context.Update(job);
                _context.SaveChanges();
                _reportGenerator.GenerateReport(job);
                job.DateCompleted = DateTime.Now;
                job.Status = JobStatus.Completed;
                _context.Update(job);
                _context.SaveChanges();
                _notifyService.AddJobNotification(NotificationLevel.Information, job.Id, "Analysis {0} has {1}", new[] { job.Name, job.Status.ToString() });
            } else {
                if (job.Status != newStatus) {
                    job.Status = newStatus;
                    _context.Update(job);
                    _context.SaveChanges();
                    _notifyService.AddJobNotification(NotificationLevel.Information, job.Id, "Analysis {0} is now {1}", new[] { job.Name, job.Status.ToString() });
                }
            }

            if (job.Status == JobStatus.Failed || job.Status == JobStatus.Completed) {
                Hangfire.RecurringJob.RemoveIfExists("jobstatus_" + job.Id);
            }

            if (newStatus == JobStatus.Completed) 
            {
                var user = _context.Users.FirstOrDefault(u => u.Id == job.CreatedBy.Id);
                if (user != null) {
                    if (user.EmailOnJobCompletion) {
                        _emailSender.SendEmailAsync(job.CreatedBy.Email, $"Analysis {job.Name} complete",
                            $"Your analysis {job.Name} is complete. Please head to LEFT to view the results").Wait(); //TODO await-asycn
                    }
                }
            }
        }

        private async Task UpdateProStatusAsync(Job job) {

            if (job.ProActivation.ProcessingStatus == JobStatus.GeneratingOutput) {
                Console.WriteLine("Data download archive still generating for: " + job.Id);
                return;
            }

            // Update status and begin generating outputs if required
            var newStatus = await _processor.GetStatus(job.ProActivation.JobProcessorReference, job.ProActivation.ProcessingStatus);
            if (newStatus == JobStatus.Completed && job.ProActivation.ProcessingStatus != JobStatus.Completed) {
                job.ProActivation.ProcessingStatus = JobStatus.GeneratingOutput;
                _context.Update(job);
                _context.SaveChanges();
                try {
                    var data = await _processor.GetReportData(job.JobProcessorReference);
                    _outputPersistence.PersistData(job.Id, data);
                    job.ProActivation.ProcessingStatus = JobStatus.Completed;
                    _context.Update(job);
                    _context.SaveChanges();
                } catch (Exception e) {
                    _logger.LogError("Could not persist data download. " + e.Message);
                    job.ProActivation.ProcessingStatus = JobStatus.Failed;
                    _context.Update(job);
                    _context.SaveChanges();
                }
            } else {
                if (job.ProActivation.ProcessingStatus != newStatus) {
                    job.ProActivation.ProcessingStatus = newStatus;
                    _context.Update(job);
                    _context.SaveChanges();
                }
            }

            if (job.ProActivation.ProcessingStatus == JobStatus.Failed || job.ProActivation.ProcessingStatus == JobStatus.Completed) {
                Hangfire.RecurringJob.RemoveIfExists("prostatus_" + job.Id);
            }

            if (job.ProActivation.ProcessingStatus == JobStatus.Completed) {
                _notifyService.AddJobNotification(NotificationLevel.Information, job.Id, "{0}: pro data ready for download", new[] { job.Name });
            }
        }

        public async Task<bool> ActivateProFeatures(int jobId, string userId)
        {
            var job = _context.Jobs
                .Include(m => m.CreatedBy)
                .Include(m => m.ProActivation)
                .FirstOrDefault(m => m.Id == jobId);
            if (job == null) throw new ArgumentException("An invalid job id was passed to the service");

            // NB the user activating is not necessarily the job creator, but could be an admin.
            var user = _context.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null) throw new ArgumentException("An invalid user id was passed to the app service");

            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
            if (!isAdmin && user.Credits == 0 && _appOptions.PaymentsEnabled) {
                return false;
            }

            // Allow reactivation by admin
            if (job.ProActivation != null && !isAdmin) throw new ArgumentException("The analysis has already been activated");

            // Submit processing for GeoTIFF output
            var request = new JobSubmission() {
                Title = job.Name,
            	Description = job.Description,
            	UserName = job.CreatedBy.UserName,
                East = job.LongitudeEast,
                West = job.LongitudeWest,
                North = job.LatitudeNorth,
                South = job.LatitudeSouth,
                Priority = 2
            };
            var processorReference = await _processor.StartProJob(request);
            if (String.IsNullOrEmpty(processorReference)) {
                return false;
            }

            if (!isAdmin && _appOptions.PaymentsEnabled) {
                user.Credits = user.Credits - 1;
            }

            var activation = new ProActivation()
            {
                //Id = Guid.NewGuid(),
                UserIdOfPurchaser = user.Id,
                TimeOfPurchase = DateTime.Now,
                CreditsSpent = 1,
                ProcessingStatus = JobStatus.Submitted,
                JobProcessorReference = processorReference
            };
            job.ProActivation = activation;
            _context.Update(job);
            _context.Update(user);
            _context.SaveChanges();
            Hangfire.RecurringJob.AddOrUpdate("prostatus_" + job.Id, () => UpdateProStatusAsync(job.Id), Cron.Minutely);

            if (!isAdmin && _appOptions.PaymentsEnabled) {
                _notifyService.AddJobNotification(NotificationLevel.Success, job.Id, 
                    "You used 1 credit to upgrade '{0}' to premium.", new string[] { job.Name });

                if (user.Credits <= 3 && user.Credits > 0) {
                    _notifyService.AddUserNotification(NotificationLevel.Information, user.Id, 
                        "Your credits are running low. You only have enough for {0} more premium activations.", new string[] { user.Credits.ToString() });
                } else if (user.Credits == 0) {
                    _notifyService.AddUserNotification(NotificationLevel.Information, user.Id, 
                        "You're out of credits, and will not be able to activate premium datasets unless you top up.", new string[] {});
                }
            } else if (!_appOptions.PaymentsEnabled) {
                _notifyService.AddJobNotification(NotificationLevel.Success, job.Id, 
                    "Your request for a high-resolution data package for '{0}' has entered the queue.", new string[] { job.Name });
            }
            return true;
        }

        public ReportData GetReportData(int jobId)
        {
            var job = _context.Jobs.FirstOrDefault(m => m.Id == jobId);
            if (job == null) throw new Exception("Job does not exist");
            var data = _processor.GetReportData(job.JobProcessorReference).Result;
            data.Title = job.Name;
            data.Description = job.Description;
            return data;
        }

        public async Task<Guid?> SubmitDataPackage(DataPackage package, List<AvailableVariable> variables)
        {
            var request = new JobSubmission() {
                Title = "Data Package API Request",
            	Description = "",
            	UserName = package.CreatedBy.UserName,
                East = package.LongitudeEast,
                West = package.LongitudeWest,
                North = package.LatitudeNorth,
                South = package.LatitudeSouth,
                Priority = 2
            };

            var processorReference = await _processor.StartDataPackage(request, package.DataRequestedTime, package.Year, package.Month, package.Day, variables.Select(v => v.Id).ToList());
            if (String.IsNullOrEmpty(processorReference)) {
                return null;
            }
            package.JobProcessorReference = processorReference;
            package.RequestComponents = System.Text.Json.JsonSerializer.Serialize(variables);
            _context.DataPackages.Add(package);
            _context.SaveChanges();

            var savedPackage = _context.DataPackages.First(j => j.JobProcessorReference == package.JobProcessorReference);
            Hangfire.RecurringJob.AddOrUpdate("pkgstatus" + savedPackage.Id, () => UpdatePackageStatusAsync(package.Id), Cron.Minutely);

            _notifyService.AddUserNotification(NotificationLevel.Success, package.CreatedBy.Id, "Requested a data package using the API: {0}.", new[] { package.Id.ToString() });
            return savedPackage.Id;
        }

        public IEnumerable<DataPackage> GetAllDataPackagesForUser(string userId)
        {
            return _context.DataPackages.Include(m => m.CreatedBy).Where(m => m.CreatedBy.Id == userId);
        }

        public async Task<DataPackage> GetDataPackage(Guid dataPackageId)
        {
            var package = _context.DataPackages.Include(m => m.CreatedBy).FirstOrDefault(m => m.Id == dataPackageId);
            var oldStatus = package.Status;
            var newStatus = await _processor.GetStatus(package.JobProcessorReference, oldStatus);
            if (newStatus == oldStatus) return package;

            package.Status = newStatus;
            if (newStatus == JobStatus.Completed)
            {
                package.TimeCompleted = DateTime.UtcNow;
            }

            _context.Update(package);
            await _context.SaveChangesAsync();
            return package;
        }

        public async Task<ReportData> GetDataPackageData(Guid dataPackageId)
        {
            var package = _context.DataPackages.Include(m => m.CreatedBy).FirstOrDefault(m => m.Id == dataPackageId);
            if (package == null) throw new Exception("Data package does not exist");
            var data = await _processor.GetReportData(package.JobProcessorReference);
            return data;
        }

        public async Task<JobStatus> PollDataPackage(Guid dataPackageId)
        {
            var package = _context.DataPackages.FirstOrDefault(m => m.Id == dataPackageId);
            var oldStatus = package.Status;
            var newStatus = await _processor.GetStatus(package.JobProcessorReference, oldStatus);
            if (newStatus == oldStatus) return oldStatus;

            package.Status = newStatus;
            if (newStatus == JobStatus.Completed)
            {
                package.TimeCompleted = DateTime.UtcNow;
            }

            _context.Update(package);
            await _context.SaveChangesAsync();
            return newStatus;
        }
    }
}
