using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using DiffSpectrumView.Models;
using DiffSpectrumView.Repositories;
using DiffSpectrumView.DTOs;

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
            _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
        }

        public async Task ProcessFlapiRequestAsync(DuplicationRequestDto request)
        {
            var jobStartTime = DateTime.UtcNow;
            
            try
            {
                // Parse the expected response from Flapi (source)
                var sourceResponse = JsonDocument.Parse(request.ExpectedResponse);
                var sourceStatusCode = sourceResponse.RootElement.TryGetProperty("statusCode", out var statusProp) 
                    ? statusProp.GetInt32() 
                    : 200;
                var sourceBody = sourceResponse.RootElement.TryGetProperty("body", out var bodyProp)
                    ? bodyProp.GetRawText()
                    : request.ExpectedResponse;

                // Send request to target environment
                HttpResponseMessage targetResponse;
                var requestContent = new StringContent(request.Content, System.Text.Encoding.UTF8, "application/json");
                
                // Extract method from URL or default to POST
                var method = ExtractHttpMethod(request.TestUrl);
                
                if (method == "GET")
                {
                    targetResponse = await _httpClient.GetAsync(request.TestUrl);
                }
                else if (method == "POST")
                {
                    targetResponse = await _httpClient.PostAsync(request.TestUrl, requestContent);
                }
                else if (method == "PUT")
                {
                    targetResponse = await _httpClient.PutAsync(request.TestUrl, requestContent);
                }
                else if (method == "DELETE")
                {
                    targetResponse = await _httpClient.DeleteAsync(request.TestUrl);
                }
                else
                {
                    targetResponse = await _httpClient.PostAsync(request.TestUrl, requestContent);
                }

                var targetBody = await targetResponse.Content.ReadAsStringAsync();
                var targetStatusCode = (int)targetResponse.StatusCode;

                // Normalize responses for comparison
                var normalizedSourceBody = _normalizationService.NormalizeJson(sourceBody);
                var normalizedTargetBody = _normalizationService.NormalizeJson(targetBody);

                // Detect diffs (only body and status code, NOT headers)
                var diffsFound = new List<(string diffType, string sourceValue, string targetValue)>();

                // Compare status codes
                if (sourceStatusCode != targetStatusCode)
                {
                    diffsFound.Add(("Status Code", sourceStatusCode.ToString(), targetStatusCode.ToString()));
                }

                // Compare JSON body responses
                if (!_normalizationService.AreJsonResponsesEqual(sourceBody, targetBody))
                {
                    diffsFound.Add(("JSON Response", normalizedSourceBody, normalizedTargetBody));
                }

                // Create job record (successful regardless of diffs found)
                var job = new Job
                {
                    Name = $"Flapi Comparison - {ExtractEndpoint(request.TestUrl)}",
                    StartTime = jobStartTime,
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
                        SourceRequest = BuildSourceRequest(request.SourceUrl, request.Content),
                        TargetRequest = BuildTargetRequest(request.TestUrl, request.Content),
                        NormalizedSourceResponse = diffType == "JSON Response" ? sourceValue : normalizedSourceBody,
                        NormalizedTargetResponse = diffType == "JSON Response" ? targetValue : normalizedTargetBody,
                        SourceCompleteResponse = BuildCompleteResponse(sourceStatusCode, sourceBody),
                        TargetCompleteResponse = BuildCompleteResponse(targetStatusCode, targetBody),
                        DiffType = diffType,
                        Endpoint = ExtractEndpoint(request.TestUrl),
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
                    Name = $"Flapi Comparison - {ExtractEndpoint(request.TestUrl)}",
                    StartTime = jobStartTime,
                    EndTime = DateTime.UtcNow,
                    Status = "Failed",
                    TotalRequestsProcessed = 0,
                    DiffsFound = 0,
                    ErrorMessage = ex.Message
                };
                await _jobRepository.CreateAsync(job);
            }
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

        private string ExtractHttpMethod(string url)
        {
            // Default to POST if not specified
            return "POST";
        }

        private string ExtractEndpoint(string url)
        {
            try
            {
                var uri = new Uri(url);
                return uri.PathAndQuery;
            }
            catch
            {
                return url;
            }
        }

        private string BuildSourceRequest(string url, string content)
        {
            return JsonSerializer.Serialize(new
            {
                url,
                content,
                timestamp = DateTime.UtcNow
            });
        }

        private string BuildCompleteResponse(int statusCode, string body)
        {
            return JsonSerializer.Serialize(new
            {
                statusCode,
                body
            });
        }
    }
}
