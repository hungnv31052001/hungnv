namespace JobWebApplicationvip.Models
{
    public class Employer
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string CompanyName { get; set; }

        public virtual ApplicationUser User { get; set; }
        public virtual ICollection<Job> Jobs { get; set; }
    }
}
