namespace DiffSpectrumView.Models
{
    public class Diff
    {
        public int Id { get; set; }
        public int JobId { get; set; }
        
        public string SourceRequest { get; set; } = string.Empty; // Request from Flapi
        public string TargetRequest { get; set; } = string.Empty; // Request sent to target environment
        
        public string NormalizedSourceResponse { get; set; } = string.Empty; // Normalized response from Flapi
        public string NormalizedTargetResponse { get; set; } = string.Empty; // Normalized response from target
        
        public string SourceCompleteResponse { get; set; } = string.Empty; // Complete response from Flapi
        public string TargetCompleteResponse { get; set; } = string.Empty; // Complete response from target
        
        public string DiffType { get; set; } = string.Empty; // JSON Response, Status Code, Headers, etc.
        
        public string Endpoint { get; set; } = string.Empty;
        public string Method { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsChecked { get; set; }
        
        // Navigation property
        public Job? Job { get; set; }
    }
}
