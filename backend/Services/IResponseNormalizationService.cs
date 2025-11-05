namespace DiffSpectrumView.Services
{
    public interface IResponseNormalizationService
    {
        string NormalizeJson(string json);
        string NormalizeHeaders(Dictionary<string, IEnumerable<string>> headers);
        bool AreJsonResponsesEqual(string json1, string json2);
        bool AreHeadersEqual(Dictionary<string, IEnumerable<string>> headers1, Dictionary<string, IEnumerable<string>> headers2);
    }
}
