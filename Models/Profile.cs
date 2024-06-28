namespace JobWebApplicationvip.Models
{
    public class Profile
    {
        public int Id { get; set; }
        public int JobSeekerId { get; set; }
        public string Resume { get; set; }
        public string ContactInformation { get; set; }

        public virtual JobSeeker JobSeeker { get; set; }
    }
}
