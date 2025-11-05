namespace DiffSpectrumView.DTOs
{
    public class DiffDto
    {
        public int Id { get; set; }
        public int JobId { get; set; }
        
        public string SourceRequest { get; set; } = string.Empty;
        public string TargetRequest { get; set; } = string.Empty;
        public string NormalizedSourceResponse { get; set; } = string.Empty;
        public string NormalizedTargetResponse { get; set; } = string.Empty;
        public string SourceCompleteResponse { get; set; } = string.Empty;
        public string TargetCompleteResponse { get; set; } = string.Empty;
        public string DiffType { get; set; } = string.Empty;
        
        public string Endpoint { get; set; } = string.Empty;
        public string Method { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsChecked { get; set; }
    }
}
