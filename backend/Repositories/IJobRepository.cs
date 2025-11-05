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
        Task<int> GetSuccessfulJobsAsync();
        Task<int> GetFailedJobsAsync();
        Task<int> GetJobsWithDiffsAsync();
    }
}
