using System.Text.Json;
using System.Text.RegularExpressions;

namespace DiffSpectrumView.Services
{
    public class ResponseNormalizationService : IResponseNormalizationService
    {
        public string NormalizeJson(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return string.Empty;

            try
            {
                // Parse and re-serialize to normalize formatting
                var jsonDoc = JsonDocument.Parse(json);
                var options = new JsonSerializerOptions
                {
                    WriteIndented = false,
                    PropertyNamingPolicy = null
                };
                
                var normalized = JsonSerializer.Serialize(jsonDoc, options);
                
                // Remove dynamic fields that might differ (timestamps, IDs, etc.)
                normalized = RemoveDynamicFields(normalized);
                
                return normalized;
            }
            catch
            {
                // If not valid JSON, return as-is
                return json.Trim();
            }
        }

        public string NormalizeHeaders(Dictionary<string, IEnumerable<string>> headers)
        {
            // Filter out headers that are expected to differ
            var excludedHeaders = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "Date",
                "X-Request-Id",
                "Request-Id",
                "X-Correlation-Id",
                "Set-Cookie",
                "Age",
                "X-Cache",
                "X-Timer"
            };

            var normalizedHeaders = headers
                .Where(h => !excludedHeaders.Contains(h.Key))
                .OrderBy(h => h.Key)
                .Select(h => $"{h.Key}: {string.Join(", ", h.Value.OrderBy(v => v))}")
                .ToList();

            return string.Join("\n", normalizedHeaders);
        }

        public bool AreJsonResponsesEqual(string json1, string json2)
        {
            var normalized1 = NormalizeJson(json1);
            var normalized2 = NormalizeJson(json2);
            
            return normalized1 == normalized2;
        }

        public bool AreHeadersEqual(Dictionary<string, IEnumerable<string>> headers1, Dictionary<string, IEnumerable<string>> headers2)
        {
            var normalized1 = NormalizeHeaders(headers1);
            var normalized2 = NormalizeHeaders(headers2);
            
            return normalized1 == normalized2;
        }

        private string RemoveDynamicFields(string json)
        {
            // Remove common dynamic fields that might differ between environments
            // This is a simple implementation - you can expand based on your needs
            
            try
            {
                var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;
                
                // For now, just return the normalized JSON
                // You can add more sophisticated logic here to remove specific fields
                return json;
            }
            catch
            {
                return json;
            }
        }
    }
}
