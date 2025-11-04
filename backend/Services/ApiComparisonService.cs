using System.Text;
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
        private readonly HttpClient _httpClient;

        public ApiComparisonService(
            IConfiguration configuration,
            IDiffRepository diffRepository,
            IJobRepository jobRepository)
        {
            _configuration = configuration;
            _diffRepository = diffRepository;
            _jobRepository = jobRepository;
            _httpClient = new HttpClient();
        }

        public async Task CompareApisAsync()
        {
            var productionBaseUrl = _configuration["ApiComparison:ProductionBaseUrl"];
            var integrationBaseUrl = _configuration["ApiComparison:IntegrationBaseUrl"];
            var endpoints = _configuration.GetSection("ApiComparison:Endpoints").Get<List<string>>() ?? new List<string>();

            // Create a new job
            var job = new Job
            {
                Name = $"API Comparison - {DateTime.UtcNow:yyyy-MM-dd HH:mm}",
                StartTime = DateTime.UtcNow,
                Status = "Running"
            };
            var jobId = await _jobRepository.CreateAsync(job);

            int totalDiffs = 0;
            int failedDiffs = 0;
            int succeededDiffs = 0;

            foreach (var endpoint in endpoints)
            {
                try
                {
                    var prodUrl = $"{productionBaseUrl}{endpoint}";
                    var intUrl = $"{integrationBaseUrl}{endpoint}";

                    var prodResponse = await _httpClient.GetAsync(prodUrl);
                    var intResponse = await _httpClient.GetAsync(intUrl);

                    var prodContent = await prodResponse.Content.ReadAsStringAsync();
                    var intContent = await intResponse.Content.ReadAsStringAsync();

                    // Compare status codes
                    if (prodResponse.StatusCode != intResponse.StatusCode)
                    {
                        await CreateDiff(jobId, "Status Code", endpoint, "GET", 
                            prodResponse.StatusCode.ToString(), 
                            intResponse.StatusCode.ToString(),
                            prodUrl, intUrl);
                        totalDiffs++;
                        failedDiffs++;
                    }

                    // Compare response bodies
                    if (prodContent != intContent)
                    {
                        await CreateDiff(jobId, "JSON Response", endpoint, "GET", 
                            prodContent, intContent, prodUrl, intUrl);
                        totalDiffs++;
                        failedDiffs++;
                    }
                    else
                    {
                        succeededDiffs++;
                    }

                    // Compare headers
                    var prodHeaders = string.Join("\n", prodResponse.Headers.Select(h => $"{h.Key}: {string.Join(", ", h.Value)}"));
                    var intHeaders = string.Join("\n", intResponse.Headers.Select(h => $"{h.Key}: {string.Join(", ", h.Value)}"));
                    
                    if (prodHeaders != intHeaders)
                    {
                        await CreateDiff(jobId, "Headers", endpoint, "GET", 
                            prodHeaders, intHeaders, prodUrl, intUrl);
                        totalDiffs++;
                        failedDiffs++;
                    }
                }
                catch (Exception ex)
                {
                    // Log error and continue
                    Console.WriteLine($"Error comparing endpoint {endpoint}: {ex.Message}");
                }
            }

            // Update job
            job.Id = jobId;
            job.EndTime = DateTime.UtcNow;
            job.Status = "Completed";
            job.TotalDiffs = totalDiffs;
            job.FailedDiffs = failedDiffs;
            job.SucceededDiffs = succeededDiffs;
            await _jobRepository.UpdateAsync(job);
        }

        private async Task CreateDiff(int jobId, string category, string endpoint, string method,
            string prodResponse, string intResponse, string prodUrl, string intUrl)
        {
            var diff = new Diff
            {
                JobId = jobId,
                Category = category,
                Endpoint = endpoint,
                Method = method,
                ProductionResponse = prodResponse,
                IntegrationResponse = intResponse,
                ProductionCurl = GenerateCurl(prodUrl, method),
                IntegrationCurl = GenerateCurl(intUrl, method),
                Timestamp = DateTime.UtcNow,
                IsDeleted = false,
                IsChecked = false
            };

            await _diffRepository.CreateAsync(diff);
        }

        private static string GenerateCurl(string url, string method)
        {
            return $"curl -X {method} '{url}'";
        }
    }
}
