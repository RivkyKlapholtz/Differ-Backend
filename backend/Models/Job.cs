namespace DiffSpectrumView.Models
{
    public class Job
    {
        public int Id { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        
        public string Status { get; set; } = "Running"; // Success, Failed, Running
        
        public bool FoundDiff { get; set; } // Did this job find a diff?
        public int? DiffId { get; set; } // If diff found, link to it
        public string? ErrorMessage { get; set; } // Error message if job failed
        
        // Navigation property
        public Diff? Diff { get; set; }
    }
}
