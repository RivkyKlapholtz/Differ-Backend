using System.Text.Json;
using DiffSpectrumView.Models;
using DiffSpectrumView.Repositories;

namespace DiffSpectrumView.Services
{
    public class ApiComparisonService : IApiComparisonService
    {
        private readonly IConfiguration _configuration;
        private readonly IDiffRepository _diffRepository;
        private readonly IJobRepository _jobRepository;
        private readonly IResponseNormalizationService _normalizationService;
        private readonly HttpClient _httpClient;

        public ApiComparisonService(
            IConfiguration configuration,
            IDiffRepository diffRepository,
            IJobRepository jobRepository,
            IResponseNormalizationService normalizationService)
        {
            _configuration = configuration;
            _diffRepository = diffRepository;
            _jobRepository = jobRepository;
            _normalizationService = normalizationService;
            _httpClient = new HttpClient();
        }

        public async Task ProcessRequestAsync(string sourceRequest, string endpoint, string method)
        {
            var targetBaseUrl = _configuration["ApiComparison:TargetBaseUrl"];
            var targetUrl = $"{targetBaseUrl}{endpoint}";

            try
            {
                // Parse source request to extract response
                var sourceData = JsonDocument.Parse(sourceRequest);
                var sourceResponse = sourceData.RootElement.GetProperty("response").GetRawText();
                var sourceStatusCode = sourceData.RootElement.GetProperty("statusCode").GetInt32();
                var sourceHeaders = ParseHeaders(sourceData.RootElement.GetProperty("headers"));

                // Send request to target environment
                HttpResponseMessage targetResponse;
                if (method.ToUpper() == "GET")
                {
                    targetResponse = await _httpClient.GetAsync(targetUrl);
                }
                else if (method.ToUpper() == "POST")
                {
                    var body = sourceData.RootElement.GetProperty("body").GetRawText();
                    var content = new StringContent(body, System.Text.Encoding.UTF8, "application/json");
                    targetResponse = await _httpClient.PostAsync(targetUrl, content);
                }
                else
                {
                    throw new NotSupportedException($"HTTP method {method} not supported");
                }

                var targetContent = await targetResponse.Content.ReadAsStringAsync();
                var targetStatusCode = (int)targetResponse.StatusCode;
                var targetHeaders = targetResponse.Headers.ToDictionary(h => h.Key, h => h.Value);

                // Normalize responses
                var normalizedSourceResponse = _normalizationService.NormalizeJson(sourceResponse);
                var normalizedTargetResponse = _normalizationService.NormalizeJson(targetContent);

                // Compare and detect diffs
                var diffsFound = new List<(string diffType, string sourceValue, string targetValue)>();

                // Compare status codes
                if (sourceStatusCode != targetStatusCode)
                {
                    diffsFound.Add(("Status Code", sourceStatusCode.ToString(), targetStatusCode.ToString()));
                }

                // Compare JSON responses
                if (!_normalizationService.AreJsonResponsesEqual(sourceResponse, targetContent))
                {
                    diffsFound.Add(("JSON Response", normalizedSourceResponse, normalizedTargetResponse));
                }

                // Compare headers
                if (!_normalizationService.AreHeadersEqual(sourceHeaders, targetHeaders))
                {
                    var normalizedSourceHeaders = _normalizationService.NormalizeHeaders(sourceHeaders);
                    var normalizedTargetHeaders = _normalizationService.NormalizeHeaders(targetHeaders);
                    diffsFound.Add(("Headers", normalizedSourceHeaders, normalizedTargetHeaders));
                }

                // Create job record (successful regardless of diffs)
                var job = new Job
                {
                    Name = $"Request Comparison - {endpoint}",
                    StartTime = DateTime.UtcNow,
                    EndTime = DateTime.UtcNow,
                    Status = "Completed",
                    TotalRequestsProcessed = 1,
                    DiffsFound = diffsFound.Count
                };
                var jobId = await _jobRepository.CreateAsync(job);

                // Save diffs to database (only if diffs exist)
                foreach (var (diffType, sourceValue, targetValue) in diffsFound)
                {
                    var diff = new Diff
                    {
                        JobId = jobId,
                        SourceRequest = sourceRequest,
                        TargetRequest = BuildTargetRequest(targetUrl, method),
                        NormalizedSourceResponse = diffType == "JSON Response" ? sourceValue : normalizedSourceResponse,
                        NormalizedTargetResponse = diffType == "JSON Response" ? targetValue : normalizedTargetResponse,
                        SourceCompleteResponse = BuildCompleteResponse(sourceStatusCode, sourceHeaders, sourceResponse),
                        TargetCompleteResponse = BuildCompleteResponse(targetStatusCode, targetHeaders, targetContent),
                        DiffType = diffType,
                        Endpoint = endpoint,
                        Method = method,
                        Timestamp = DateTime.UtcNow,
                        IsDeleted = false,
                        IsChecked = false
                    };

                    await _diffRepository.CreateAsync(diff);
                }
            }
            catch (Exception ex)
            {
                // Job failed - create failed job record
                var job = new Job
                {
                    Name = $"Request Comparison - {endpoint}",
                    StartTime = DateTime.UtcNow,
                    EndTime = DateTime.UtcNow,
                    Status = "Failed",
                    TotalRequestsProcessed = 0,
                    DiffsFound = 0,
                    ErrorMessage = ex.Message
                };
                await _jobRepository.CreateAsync(job);
                
                throw;
            }
        }

        private Dictionary<string, IEnumerable<string>> ParseHeaders(JsonElement headersElement)
        {
            var headers = new Dictionary<string, IEnumerable<string>>();
            
            foreach (var property in headersElement.EnumerateObject())
            {
                var values = new List<string>();
                if (property.Value.ValueKind == JsonValueKind.Array)
                {
                    foreach (var item in property.Value.EnumerateArray())
                    {
                        values.Add(item.GetString() ?? string.Empty);
                    }
                }
                else
                {
                    values.Add(property.Value.GetString() ?? string.Empty);
                }
                headers[property.Name] = values;
            }
            
            return headers;
        }

        private string BuildTargetRequest(string url, string method)
        {
            return JsonSerializer.Serialize(new
            {
                url,
                method,
                timestamp = DateTime.UtcNow
            });
        }

        private string BuildCompleteResponse(int statusCode, Dictionary<string, IEnumerable<string>> headers, string body)
        {
            return JsonSerializer.Serialize(new
            {
                statusCode,
                headers,
                body
            });
        }
    }
}
