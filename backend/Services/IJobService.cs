using DiffSpectrumView.DTOs;

namespace DiffSpectrumView.Services
{
    public interface IJobService
    {
        Task<List<JobDto>> GetAllJobsAsync();
        Task<JobDto?> GetJobByIdAsync(int id);
        Task<JobsSummaryDto> GetJobsSummaryAsync();
    }
}
