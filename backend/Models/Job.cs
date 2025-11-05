namespace DiffSpectrumView.Models
{
    public class Job
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        
        public string Status { get; set; } = "Running"; // Running, Completed, Failed
        
        public int TotalRequestsProcessed { get; set; } // Total requests processed by this job
        public int DiffsFound { get; set; } // Number of diffs found (saved to DB)
        public string? ErrorMessage { get; set; } // Error message if job failed
        
        // Navigation property
        public ICollection<Diff> Diffs { get; set; } = new List<Diff>();
    }
}
