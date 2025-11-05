namespace DiffSpectrumView.DTOs
{
    public class JobDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string Status { get; set; } = string.Empty;
        
        public int TotalRequestsProcessed { get; set; }
        public int DiffsFound { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
