using Microsoft.AspNetCore.Identity;

namespace JobWebApplicationvip.Models
{
    public class ApplicationRole : IdentityRole
    {
        public const string AdminRole = "Admin";
        public const string EmployerRole = "Employer";
        public const string JobSeekerRole = "JobSeeker";
        public virtual ICollection<ApplicationUserRole> UserRoles { get; set; }
    }
}
