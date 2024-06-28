namespace JobWebApplicationvip.Models
{
    public enum ApplicationStatus
    {
        Pending,
        Approved,
        Rejected
    }

    public class Application
    {
        public int Id { get; set; }
        public int JobId { get; set; }
        public int JobSeekerId { get; set; }
        public string Resume { get; set; }
        public string CoverLetter { get; set; }
        public string SelfIntroduction { get; set; }
        public DateTime SubmissionDate { get; set; }

        public ApplicationStatus Status { get; set; }  
        public virtual Job Job { get; set; }
        public virtual JobSeeker JobSeeker { get; set; }
    }
}
