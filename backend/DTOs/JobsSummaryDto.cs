namespace DiffSpectrumView.DTOs
{
    public class JobsSummaryDto
    {
        public int TotalJobs { get; set; }
        public int SuccessfulJobs { get; set; }
        public int FailedJobs { get; set; }
        public int JobsWithDiffs { get; set; }
        public int JobsWithoutDiffs { get; set; }
        public List<JobDto> RecentJobs { get; set; } = new();
    }
}
