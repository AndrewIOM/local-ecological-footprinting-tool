using Ecoset.WebUI.Services.Abstract;
using Ecoset.WebUI.Models;
using System.Collections.Generic;
using Ecoset.WebUI.Data;
using System.Linq;
using System;
using Microsoft.EntityFrameworkCore;
using Ecoset.WebUI.Models.JobProcessor;
using Ecoset.WebUI.Enums;
using Microsoft.AspNetCore.Identity;
using Hangfire;
using Ecoset.WebUI.Options;
using Microsoft.Extensions.Options;

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
        private readonly PaymentOptions _payOptions;
        private readonly UserManager<ApplicationUser> _userManager;
        public JobService(ApplicationDbContext context, 
            IJobProcessor processor, 
            INotificationService notifyService, 
            IEmailSender emailSender, 
            IReportGenerator reportGenerator,
            IOutputPersistence outputPersistence,
            UserManager<ApplicationUser> userManager,
            IOptions<PaymentOptions> payOptions) {
            _context = context;
            _processor = processor;
            _notifyService = notifyService;
            _emailSender = emailSender;
            _outputPersistence = outputPersistence;
            _reportGenerator = reportGenerator;
            _userManager = userManager;
            _payOptions = payOptions.Value;
        }

        public IEnumerable<Job> GetAll()
        {
            var jobs = _context.Jobs.Include(m => m.CreatedBy).Include(m => m.ProActivation).ToList();
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
                result.Add(job);
            }

            _context.SaveChanges();
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

        public int? SubmitJob(Job job) {
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
            var processorReference = _processor.StartJob(request).Result; //TODO await-async
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
        
            Hangfire.RecurringJob.AddOrUpdate("jobstatus_" + savedJob.Id, () => UpdateJobStatus(savedJob.Id), Cron.Minutely);

            return savedJob.Id;
        }

        public void RefreshJobStatus(int jobId) {
            var job = _context.Jobs.FirstOrDefault(m => m.Id == jobId);
            if (job == null) return;
            Hangfire.RecurringJob.AddOrUpdate("jobstatus_" + job.Id, () => UpdateJobStatus(job.Id), Cron.Minutely);
        }

        public void UpdateJobStatus(int jobId) {
            Console.WriteLine("Updating job status for " + jobId);
            var result = _context.Jobs
                .Include(m => m.CreatedBy)
                .Include(m => m.Notifications)
                .Include(m => m.ProActivation)
                .FirstOrDefault(m => m.Id == jobId);
            if (result != null) {
                UpdateJobStatus(result);
            }
        }

        public void UpdateProStatus(int jobId) {
            Console.WriteLine("Updating pro status for " + jobId);
            var result = _context.Jobs
                .Include(m => m.CreatedBy)
                .Include(m => m.Notifications)
                .Include(m => m.ProActivation)
                .FirstOrDefault(m => m.Id == jobId);
            if (result != null) {
                if (result.ProActivation != null) {
                    UpdateProStatus(result);
                }
            }
        }

        private void UpdateJobStatus(Job job) {
            var oldStatus = job.Status;
            var newStatus = _processor.GetStatus(job.JobProcessorReference, oldStatus).Result;
            if (newStatus == oldStatus) return;

            job.Status = newStatus;
            if (newStatus == JobStatus.Completed)
            {
                _reportGenerator.GenerateReport(job);
                job.DateCompleted = DateTime.Now;
            }

            if (job.Status == JobStatus.Failed || job.Status == JobStatus.Completed) {
                Hangfire.RecurringJob.RemoveIfExists("jobstatus_" + job.Id);
            }

            _context.Update(job);

            _notifyService.AddJobNotification(NotificationLevel.Information, job.Id, "Analysis {0} is now {1}", new[] { job.Name, job.Status.ToString() });
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

        private void UpdateProStatus(Job job) {
            var oldStatus = job.ProActivation.ProcessingStatus;
            var newStatus = _processor.GetStatus(job.ProActivation.JobProcessorReference, oldStatus).Result; //TODO await-async
            if (newStatus == oldStatus) return;
            job.ProActivation.ProcessingStatus = newStatus;
            if (job.ProActivation.ProcessingStatus == JobStatus.Failed || job.ProActivation.ProcessingStatus == JobStatus.Completed) {
                Hangfire.RecurringJob.RemoveIfExists("prostatus_" + job.Id);
            }

            if (job.ProActivation.ProcessingStatus == JobStatus.Completed)
            {
                var files = _processor.GetReportFiles(job.ProActivation.JobProcessorReference).Result; //TODO await-async
                var items = files.Select(x => new ProDataItem { FileExtension = x.Item3, Contents = x.Item2, LayerName = x.Item1 }).ToList();
                _outputPersistence.PersistProData(job.Id, items);
            }

            _context.Update(job);
            if (job.ProActivation.ProcessingStatus == JobStatus.Completed) {
                _notifyService.AddJobNotification(NotificationLevel.Information, job.Id, "{0}: pro data ready for download", new[] { job.Name });
            }
        }

        public bool ActivateProFeatures(int jobId, string userId)
        {
            var job = _context.Jobs
                .Include(m => m.CreatedBy)
                .Include(m => m.ProActivation)
                .FirstOrDefault(m => m.Id == jobId);
            if (job == null) throw new ArgumentException("An invalid job id was passed to the service");

            // NB the user activating is not necessarily the job creator, but could be an admin.
            var user = _context.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null) throw new ArgumentException("An invalid user id was passed to the app service");

            var isAdmin = _userManager.IsInRoleAsync(user, "Admin").Result;
            if (!isAdmin && user.Credits == 0 && _payOptions.PaymentsEnabled) {
                return false; //"You must have at least one credit to complete a pro activation."
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
            var processorReference = _processor.StartProJob(request).Result; //TODO await-async

            if (!isAdmin && _payOptions.PaymentsEnabled) {
                user.Credits = user.Credits - 1;
            }

            var activation = new ProActivation()
            {
                Id = Guid.NewGuid(),
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
            Hangfire.RecurringJob.AddOrUpdate("prostatus_" + job.Id, () => UpdateProStatus(job.Id), Cron.Minutely);

            if (!isAdmin && _payOptions.PaymentsEnabled) {
                _notifyService.AddJobNotification(NotificationLevel.Success, job.Id, 
                    "You used 1 credit to upgrade '{0}' to premium.", new string[] { job.Name });

                if (user.Credits <= 3 && user.Credits > 0) {
                    _notifyService.AddUserNotification(NotificationLevel.Information, user.Id, 
                        "Your credits are running low. You only have enough for {0} more premium activations.", new string[] { user.Credits.ToString() });
                } else if (user.Credits == 0) {
                    _notifyService.AddUserNotification(NotificationLevel.Information, user.Id, 
                        "You're out of credits, and will not be able to activate premium datasets unless you top up.", new string[] {});
                }
            } else if (!_payOptions.PaymentsEnabled) {
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
    }
}
