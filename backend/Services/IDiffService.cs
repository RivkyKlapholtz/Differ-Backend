using DiffSpectrumView.DTOs;

namespace DiffSpectrumView.Services
{
    public interface IDiffService
    {
        Task<List<DiffDto>> GetAllDiffsAsync();
        Task<DiffDto?> GetDiffByIdAsync(int id);
        Task<List<DiffDto>> GetDiffsByJobIdAsync(int jobId);
        Task DeleteDiffAsync(int id);
        Task RestoreDiffAsync(int id);
        Task UpdateDiffCheckedStatusAsync(int id, bool isChecked);
    }
}
