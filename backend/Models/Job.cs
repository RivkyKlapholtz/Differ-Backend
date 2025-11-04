namespace DiffSpectrumView.Models
{
    public class Job
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string Status { get; set; } = "Running"; // Running, Completed, Failed
        public int TotalDiffs { get; set; }
        public int FailedDiffs { get; set; }
        public int SucceededDiffs { get; set; }
        
        // Navigation property
        public ICollection<Diff> Diffs { get; set; } = new List<Diff>();
    }
}
