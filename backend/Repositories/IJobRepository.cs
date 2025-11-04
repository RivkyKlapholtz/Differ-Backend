using DiffSpectrumView.Models;

namespace DiffSpectrumView.Repositories
{
    public interface IJobRepository
    {
        Task<List<Job>> GetAllAsync();
        Task<Job?> GetByIdAsync(int id);
        Task<int> CreateAsync(Job job);
        Task UpdateAsync(Job job);
        Task<int> GetTotalJobsAsync();
        Task<int> GetTotalDiffsAsync();
        Task<int> GetFailedDiffsAsync();
        Task<int> GetSucceededDiffsAsync();
    }
}
