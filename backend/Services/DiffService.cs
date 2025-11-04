using DiffSpectrumView.DTOs;
using DiffSpectrumView.Repositories;

namespace DiffSpectrumView.Services
{
    public class DiffService : IDiffService
    {
        private readonly IDiffRepository _diffRepository;

        public DiffService(IDiffRepository diffRepository)
        {
            _diffRepository = diffRepository;
        }

        public async Task<List<DiffDto>> GetAllDiffsAsync()
        {
            var diffs = await _diffRepository.GetAllAsync();
            return diffs.Select(d => new DiffDto
            {
                Id = d.Id,
                JobId = d.JobId,
                Category = d.Category,
                Endpoint = d.Endpoint,
                Method = d.Method,
                ProductionResponse = d.ProductionResponse,
                IntegrationResponse = d.IntegrationResponse,
                ProductionCurl = d.ProductionCurl,
                IntegrationCurl = d.IntegrationCurl,
                Timestamp = d.Timestamp,
                IsDeleted = d.IsDeleted,
                IsChecked = d.IsChecked
            }).ToList();
        }

        public async Task<DiffDto?> GetDiffByIdAsync(int id)
        {
            var diff = await _diffRepository.GetByIdAsync(id);
            if (diff == null) return null;

            return new DiffDto
            {
                Id = diff.Id,
                JobId = diff.JobId,
                Category = diff.Category,
                Endpoint = diff.Endpoint,
                Method = diff.Method,
                ProductionResponse = diff.ProductionResponse,
                IntegrationResponse = diff.IntegrationResponse,
                ProductionCurl = diff.ProductionCurl,
                IntegrationCurl = diff.IntegrationCurl,
                Timestamp = diff.Timestamp,
                IsDeleted = diff.IsDeleted,
                IsChecked = diff.IsChecked
            };
        }

        public async Task<List<DiffDto>> GetDiffsByJobIdAsync(int jobId)
        {
            var diffs = await _diffRepository.GetByJobIdAsync(jobId);
            return diffs.Select(d => new DiffDto
            {
                Id = d.Id,
                JobId = d.JobId,
                Category = d.Category,
                Endpoint = d.Endpoint,
                Method = d.Method,
                ProductionResponse = d.ProductionResponse,
                IntegrationResponse = d.IntegrationResponse,
                ProductionCurl = d.ProductionCurl,
                IntegrationCurl = d.IntegrationCurl,
                Timestamp = d.Timestamp,
                IsDeleted = d.IsDeleted,
                IsChecked = d.IsChecked
            }).ToList();
        }

        public async Task DeleteDiffAsync(int id)
        {
            await _diffRepository.DeleteAsync(id);
        }

        public async Task RestoreDiffAsync(int id)
        {
            await _diffRepository.RestoreAsync(id);
        }

        public async Task UpdateDiffCheckedStatusAsync(int id, bool isChecked)
        {
            var diff = await _diffRepository.GetByIdAsync(id);
            if (diff != null)
            {
                diff.IsChecked = isChecked;
                await _diffRepository.UpdateAsync(diff);
            }
        }
    }
}
