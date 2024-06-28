using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using JobWebApplicationvip.Data;
using JobWebApplicationvip.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Authorization;
 

namespace JobWebApplicationvip.Controllers
{
    public class JobsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;
        private readonly UserManager<ApplicationUser> _userManager;

        // Constructor to initialize the controller with required dependencies
        public JobsController(ApplicationDbContext context, IWebHostEnvironment hostEnvironment, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _hostEnvironment = hostEnvironment;
            _userManager = userManager;
        }

        // GET: Jobs/Index
        // Retrieves and displays a list of jobs with their associated categories and employers
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Jobs.Include(j => j.Category).Include(j => j.Employer);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Jobs/Details/5
        // Retrieves details of a specific job by its ID, including if the current user (job seeker) has applied to it
        public async Task<IActionResult> Details(int? id)
        {
            // Check if job ID is provided
            if (id == null)
            {
                return NotFound();
            }

            // Retrieve the job details including category and employer information
            var job = await _context.Jobs
                .Include(j => j.Category)
                .Include(j => j.Employer)
                .FirstOrDefaultAsync(m => m.Id == id);

            // If job is not found, return NotFound
            if (job == null)
            {
                return NotFound();
            }

            // Check if the current user is authenticated and retrieve job seeker information
            var user = await _userManager.GetUserAsync(User);
            var jobSeeker = await _context.JobSeekers.FirstOrDefaultAsync(js => js.UserId == user.Id);

            // Initialize ViewBag to track if the job seeker has applied to this job
            ViewBag.HasApplied = false;

            // If job seeker exists, check if they have applied to this job
            if (jobSeeker != null)
            {
                ViewBag.HasApplied = await _context.Applications
                    .AnyAsync(a => a.JobId == job.Id && a.JobSeekerId == jobSeeker.Id);
            }

            // Return the job details view with relevant data
            return View(job);
        }

        // GET: Jobs/Create
        // Displays the form for creating a new job posting
        public async Task<IActionResult> Create()
        {
            // Retrieve the current user
            var user = await _userManager.GetUserAsync(User);

            // Check if user is authenticated and has the role of Employer
            if (user == null || !User.IsInRole("Employer"))
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            // Populate ViewData with job categories for selection in the create form
            ViewData["CategoryId"] = new SelectList(_context.JobCategories, "Id", "CategoryName");

            // Return the create view
            return View();
        }

        // POST: Jobs/Create
        // Handles form submission for creating a new job posting
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CategoryId,Title,Description,RequiredQualifications,ApplicationDeadline,Image")] Job job)
        {
            // Retrieve the current user
            var user = await _userManager.GetUserAsync(User);

            // Check if user is authenticated and has the role of Employer
            if (user == null || !User.IsInRole("Employer"))
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            // Retrieve the employer associated with the current user
            var employer = await _context.Employers.FirstOrDefaultAsync(e => e.UserId == user.Id);

            // If employer is not found, log an error and redirect to access denied page
            if (employer == null)
            {
                Console.WriteLine($"Employer not found for UserId: {user.Id}");
                return RedirectToAction("AccessDenied", "Account");
            }

            // Assign the current employer ID to the job
            job.EmployerId = employer.Id;

            // If model state is valid, handle image upload and save job to database
            if (ModelState.IsValid)
            {
                if (job.Image != null && job.Image.Length > 0)
                {
                    string uniqueFileName = UploadedFile(job);
                    job.ImagePath = uniqueFileName;
                }
                _context.Add(job);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // If model state is not valid, reload the create view with existing data and errors
            ViewData["CategoryId"] = new SelectList(_context.JobCategories, "Id", "CategoryName", job.CategoryId);
            return View(job);
        }

        // GET: Jobs/Edit/5
        // Displays the form for editing a specific job posting
        public async Task<IActionResult> Edit(int? id)
        {
            // Check if job ID is provided
            if (id == null)
            {
                return NotFound();
            }

            // Retrieve the job details by ID
            var job = await _context.Jobs.FindAsync(id);

            // If job is not found, return NotFound
            if (job == null)
            {
                return NotFound();
            }

            // Retrieve the current user
            var user = await _userManager.GetUserAsync(User);

            // Check if user is authenticated and has the role of Employer
            if (user == null || !User.IsInRole("Employer"))
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            // Retrieve the employer associated with the current user
            var employer = await _context.Employers.FirstOrDefaultAsync(e => e.UserId == user.Id);

            // If employer is not found or does not match the job's employer, log an error and redirect to access denied page
            if (employer == null || job.EmployerId != employer.Id)
            {
                Console.WriteLine($"Employer not found or does not match for UserId: {user.Id}");
                return RedirectToAction("AccessDenied", "Account");
            }

            // Populate ViewData with job categories for selection in the edit form
            ViewData["CategoryId"] = new SelectList(_context.JobCategories, "Id", "CategoryName", job.CategoryId);

            // Return the edit view with the job details
            return View(job);
        }

        // POST: Jobs/Edit/5
        // Handles form submission for editing a specific job posting
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,CategoryId,Title,Description,RequiredQualifications,ApplicationDeadline,Image")] Job job)
        {
            // Retrieve the current user
            var user = await _userManager.GetUserAsync(User);

            // Check if user is authenticated and has the role of Employer
            if (user == null || !User.IsInRole("Employer"))
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            // Retrieve the employer associated with the current user
            var employer = await _context.Employers.FirstOrDefaultAsync(e => e.UserId == user.Id);

            // If employer is not found, redirect to access denied page
            if (employer == null)
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            // Check if submitted job ID matches the provided ID
            if (id != job.Id)
            {
                return NotFound();
            }

            // Assign the current employer ID to the job
            job.EmployerId = employer.Id;

            // If model state is valid, update job details in database
            if (ModelState.IsValid)
            {
                try
                {
                    // Retrieve the original job details to manage changes in image file
                    var oldJob = await _context.Jobs.AsNoTracking().FirstOrDefaultAsync(j => j.Id == id);

                    // If new image is uploaded, manage deletion of old image file and save new image
                    if (job.Image != null && job.Image.Length > 0)
                    {
                        if (!string.IsNullOrEmpty(oldJob.ImagePath))
                        {
                            var oldImagePath = Path.Combine(_hostEnvironment.WebRootPath, "images", oldJob.ImagePath);
                            if (System.IO.File.Exists(oldImagePath))
                            {
                                System.IO.File.Delete(oldImagePath);
                            }
                        }
                        string uniqueFileName = UploadedFile(job);
                        job.ImagePath = uniqueFileName;
                    }
                    else
                    {
                        job.ImagePath = oldJob.ImagePath; // Keep the original image path if no new image is uploaded
                    }

                    // Keep the original EmployerId unchanged
                    job.EmployerId = oldJob.EmployerId;

                    // Update job details in database
                    _context.Update(job);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!JobExists(job.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            // If model state is not valid, reload the edit view with existing data and errors
            ViewData["CategoryId"] = new SelectList(_context.JobCategories, "Id", "CategoryName", job.CategoryId);
            return View(job);
        }

        // GET: Jobs/Delete/5
        // Displays confirmation page for deleting a specific job posting
        public async Task<IActionResult> Delete(int? id)
        {
            // Check if job ID is provided
            if (id == null)
            {
                return NotFound();
            }

            // Retrieve the job details including category and employer information
            var job = await _context.Jobs
                .Include(j => j.Category)
                .Include(j => j.Employer)
                .FirstOrDefaultAsync(m => m.Id == id);

            // If job is not found, return NotFound
            if (job == null)
            {
                return NotFound();
            }

            // Return the delete view with job details for confirmation
            return View(job);
        }

        // POST: Jobs/Delete/5
        // Handles form submission for deleting a specific job posting
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Retrieve the job details by ID
            var job = await _context.Jobs.FindAsync(id);

            // If job exists, delete associated image file (if any) and remove job from database
            if (job != null)
            {
                if (!string.IsNullOrEmpty(job.ImagePath))
                {
                    var imagePath = Path.Combine(_hostEnvironment.WebRootPath, "images", job.ImagePath);
                    if (System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath);
                    }
                }
                _context.Jobs.Remove(job);
            }

            // Save changes to database and redirect to job index page
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // Helper method to handle file upload for job image
        private string UploadedFile(Job model)
        {
            string uniqueFileName = null;
            if (model.Image != null)
            {
                try
                {
                    string uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "images");
                    Directory.CreateDirectory(uploadsFolder); // Create directory if it doesn't exist
                    uniqueFileName = Guid.NewGuid().ToString() + "_" + model.Image.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        model.Image.CopyTo(fileStream);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error uploading file: {ex.Message}");
                }
            }
            return uniqueFileName;
        }

        // Helper method to check if a job exists by ID
        private bool JobExists(int id)
        {
            return _context.Jobs.Any(e => e.Id == id);
        }
    }

}
