using DiffSpectrumView.Models;

namespace DiffSpectrumView.Repositories
{
    public interface IDiffRepository
    {
        Task<List<Diff>> GetAllAsync();
        Task<Diff?> GetByIdAsync(int id);
        Task<List<Diff>> GetByJobIdAsync(int jobId);
        Task<int> CreateAsync(Diff diff);
        Task UpdateAsync(Diff diff);
        Task DeleteAsync(int id);
        Task RestoreAsync(int id);
    }
}
