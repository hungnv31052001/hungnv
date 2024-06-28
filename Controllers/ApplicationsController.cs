using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using JobWebApplicationvip.Data;
using JobWebApplicationvip.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

namespace JobWebApplicationvip.Controllers
{
    [Authorize]
    public class ApplicationsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<ApplicationsController> _logger;

        public ApplicationsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, ILogger<ApplicationsController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        [Authorize(Roles = "Employer")]
        public async Task<IActionResult> EmployerIndex()
        {
            var applications = await _context.Applications
                .Include(a => a.Job)
                .ToListAsync();

            return View("EmployerIndex", applications);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Employer")]
        public async Task<IActionResult> Approve(int id)
        {
            var application = await _context.Applications.FindAsync(id);
            if (application == null)
            {
                return NotFound();
            }

            application.Status = ApplicationStatus.Approved;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(EmployerIndex));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Employer")]
        public async Task<IActionResult> Reject(int id)
        {
            var application = await _context.Applications.FindAsync(id);
            if (application == null)
            {
                return NotFound();
            }

            application.Status = ApplicationStatus.Rejected;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(EmployerIndex));
        }

        [Authorize(Roles = "JobSeeker")]
        public async Task<IActionResult> Create(int? jobId)
        {
            _logger.LogInformation($"Starting Create action with jobId: {jobId}");
            if (jobId == null)
            {
                _logger.LogWarning("JobId is null");
                return NotFound();
            }

            var job = await _context.Jobs.Include(j => j.Category).Include(j => j.Employer).FirstOrDefaultAsync(m => m.Id == jobId);
            _logger.LogInformation($"Retrieved job: {job?.Id} - {job?.Title}");

            if (job == null)
            {
                _logger.LogWarning($"Job not found for id: {jobId}");
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            _logger.LogInformation($"User: {user?.Id} - {user?.UserName}");

            var jobSeeker = await _context.JobSeekers.FirstOrDefaultAsync(js => js.UserId == user.Id);
            _logger.LogInformation($"JobSeeker: {jobSeeker?.Id}");

            if (jobSeeker == null)
            {
                _logger.LogWarning($"JobSeeker not found for UserId: {user.Id}");
                return RedirectToAction("AccessDenied", "Account");
            }

            var application = new Application
            {
                JobId = job.Id,
                JobSeekerId = jobSeeker.Id,
                Job = job,
                JobSeeker = jobSeeker
            };
            _logger.LogInformation($"Created Application object: JobId={application.JobId}, JobSeekerId={application.JobSeekerId}");

            return View(application);
        }

        [Authorize(Roles = "JobSeeker")]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            _logger.LogInformation($"User: {user?.Id} - {user?.UserName}, IsInRole('JobSeeker'): {User.IsInRole("JobSeeker")}");

            var jobSeeker = await _context.JobSeekers.FirstOrDefaultAsync(js => js.UserId == user.Id);
            _logger.LogInformation($"JobSeeker: {jobSeeker?.Id}");

            if (jobSeeker == null)
            {
                _logger.LogWarning($"JobSeeker not found for UserId: {user.Id}");
                return RedirectToAction("AccessDenied", "Account");
            }

            var applications = await _context.Applications
                .Include(a => a.Job)
                .Where(a => a.JobSeekerId == jobSeeker.Id)
                .ToListAsync();

            return View(applications);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "JobSeeker")]
        public async Task<IActionResult> Create([Bind("JobId,Resume,CoverLetter,SelfIntroduction")] Application application)
        {
            _logger.LogInformation($"Starting Create POST action with JobId: {application.JobId}");

            var user = await _userManager.GetUserAsync(User);
            _logger.LogInformation($"User: {user?.Id} - {user?.UserName}, IsInRole('JobSeeker'): {User.IsInRole("JobSeeker")}");

            var jobSeeker = await _context.JobSeekers.FirstOrDefaultAsync(js => js.UserId == user.Id);
            _logger.LogInformation($"JobSeeker:{jobSeeker?.Id}");

            if (jobSeeker == null)
            {
                _logger.LogWarning($"JobSeeker not found for UserId: {user.Id}");
                return RedirectToAction("AccessDenied", "Account");
            }

            application.JobSeekerId = jobSeeker.Id;
            application.SubmissionDate = DateTime.Now;
            application.Status = ApplicationStatus.Pending;

            _logger.LogInformation($"Application data: JobId={application.JobId}, JobSeekerId={application.JobSeekerId}, Status={application.Status}");

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("ModelState is not valid");
                foreach (var state in ModelState)
                {
                    _logger.LogWarning($"Key: {state.Key}");
                    foreach (var error in state.Value.Errors)
                    {
                        _logger.LogWarning($"- ErrorMessage: {error.ErrorMessage}");
                        _logger.LogWarning($"- Exception: {error.Exception}");
                    }
                }
            }

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    _context.Add(application);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    _logger.LogInformation("Application saved successfully!");
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError($"Error saving application: {ex.Message}");
                    _logger.LogError($"Stack Trace: {ex.StackTrace}");
                }
            }

            var job = await _context.Jobs.FindAsync(application.JobId);
            _logger.LogInformation($"Job: {job?.Id} - {job?.Title}");

            application.Job = job;
            application.JobSeeker = jobSeeker;
            return View(application);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var application = await _context.Applications.FindAsync(id);
            if (application == null)
            {
                return NotFound();
            }

            _context.Applications.Remove(application);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}

