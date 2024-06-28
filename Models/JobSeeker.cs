using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace JobWebApplicationvip.Models
{
    public class JobSeeker
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string FullName { get; set; }

        public virtual ApplicationUser User { get; set; }
        public virtual ICollection<Application> Applications { get; set; }
        public virtual ICollection<Profile> Profiles { get; set; }
    }
}
