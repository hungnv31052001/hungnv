using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JobWebApplicationvip.Models
{
    public class Job
    {
        public int Id { get; set; }  // Unique identifier for the job posting

        [Required]
        public int EmployerId { get; set; }  // Foreign key referencing the employer who posted the job

        [Required]
        public int CategoryId { get; set; }  // Foreign key referencing the job category

        [Required]
        public string Title { get; set; }  // Title of the job posting, required field

        public string Description { get; set; }  // Detailed description of the job posting

        public string RequiredQualifications { get; set; }  // Qualifications required for the job

        [DataType(DataType.Date)]
        public DateTime ApplicationDeadline { get; set; }  // Deadline for applications, specified as a date

        [NotMapped]
        public IFormFile? Image { get; set; }  // Image file for the job posting, not mapped to the database

        public string? ImagePath { get; set; }  // Path to the image file stored in the database

        public virtual Employer? Employer { get; set; }  // Navigation property for the related employer

        public virtual JobCategory? Category { get; set; }  // Navigation property for the related job category

        public virtual ICollection<Application>? Applications { get; set; }  // Collection of applications submitted for this job

    }
}