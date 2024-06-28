using Microsoft.AspNetCore.Identity;

namespace JobWebApplicationvip.Models
{
    public class ApplicationUser : IdentityUser
    {
        public virtual Employer Employer { get; set; }
        public virtual JobSeeker JobSeeker { get; set; }
        public virtual ICollection<ApplicationUserRole> UserRoles { get; set; }
    }
}
