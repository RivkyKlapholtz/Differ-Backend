namespace DiffSpectrumView.DTOs
{
    public class DuplicationRequestDto
    {
        public string TestUrl { get; set; } = string.Empty;
        public string SourceUrl { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string ExpectedResponse { get; set; } = string.Empty;
        public DuplicationOptionsDto Options { get; set; } = new();
    }

    public class DuplicationOptionsDto
    {
        public bool AllowSemanticCompresion { get; set; }
    }
}
