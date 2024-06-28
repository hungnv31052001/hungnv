namespace JobWebApplicationvip.Models
{
    public class JobCategory
    {
        // Unique identifier for the job category
        public int Id { get; set; }

        // Name of the job category
        public string CategoryName { get; set; }

        // Flag indicating whether the job category is approved or not
        public bool IsApproved { get; set; }

        // Navigation property to represent the collection of jobs associated with this category
        public virtual ICollection<Job> Jobs { get; set; }
    }

}
