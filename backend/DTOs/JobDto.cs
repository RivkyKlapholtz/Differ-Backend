namespace DiffSpectrumView.DTOs
{
    public class JobDto
    {
        public int Id { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string Status { get; set; } = string.Empty;
        
        public bool FoundDiff { get; set; }
        public int? DiffId { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
