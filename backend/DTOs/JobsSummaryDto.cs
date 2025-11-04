namespace DiffSpectrumView.DTOs
{
    public class JobsSummaryDto
    {
        public int TotalJobs { get; set; }
        public int TotalDiffs { get; set; }
        public int FailedDiffs { get; set; }
        public int SucceededDiffs { get; set; }
        public List<JobDto> RecentJobs { get; set; } = new();
    }
}
