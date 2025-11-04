namespace DiffSpectrumView.DTOs
{
    public class JobDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string Status { get; set; } = string.Empty;
        public int TotalDiffs { get; set; }
        public int FailedDiffs { get; set; }
        public int SucceededDiffs { get; set; }
    }
}
