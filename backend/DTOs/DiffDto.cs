namespace DiffSpectrumView.DTOs
{
    public class DiffDto
    {
        public int Id { get; set; }
        public int JobId { get; set; }
        public string Category { get; set; } = string.Empty;
        public string Endpoint { get; set; } = string.Empty;
        public string Method { get; set; } = string.Empty;
        public string ProductionResponse { get; set; } = string.Empty;
        public string IntegrationResponse { get; set; } = string.Empty;
        public string ProductionCurl { get; set; } = string.Empty;
        public string IntegrationCurl { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsChecked { get; set; }
    }
}
