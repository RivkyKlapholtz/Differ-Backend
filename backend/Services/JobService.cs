using DiffSpectrumView.DTOs;
using DiffSpectrumView.Repositories;

namespace DiffSpectrumView.Services
{
    public class JobService : IJobService
    {
        private readonly IJobRepository _jobRepository;

        public JobService(IJobRepository jobRepository)
        {
            _jobRepository = jobRepository;
        }

        public async Task<List<JobDto>> GetAllJobsAsync()
        {
            var jobs = await _jobRepository.GetAllAsync();
            return jobs.Select(j => new JobDto
            {
                Id = j.Id,
                Name = j.Name,
                StartTime = j.StartTime,
                EndTime = j.EndTime,
                Status = j.Status,
                TotalDiffs = j.TotalDiffs,
                FailedDiffs = j.FailedDiffs,
                SucceededDiffs = j.SucceededDiffs
            }).ToList();
        }

        public async Task<JobDto?> GetJobByIdAsync(int id)
        {
            var job = await _jobRepository.GetByIdAsync(id);
            if (job == null) return null;

            return new JobDto
            {
                Id = job.Id,
                Name = job.Name,
                StartTime = job.StartTime,
                EndTime = job.EndTime,
                Status = job.Status,
                TotalDiffs = job.TotalDiffs,
                FailedDiffs = job.FailedDiffs,
                SucceededDiffs = job.SucceededDiffs
            };
        }

        public async Task<JobsSummaryDto> GetJobsSummaryAsync()
        {
            var totalJobs = await _jobRepository.GetTotalJobsAsync();
            var totalDiffs = await _jobRepository.GetTotalDiffsAsync();
            var failedDiffs = await _jobRepository.GetFailedDiffsAsync();
            var succeededDiffs = await _jobRepository.GetSucceededDiffsAsync();
            var recentJobs = await GetAllJobsAsync();

            return new JobsSummaryDto
            {
                TotalJobs = totalJobs,
                TotalDiffs = totalDiffs,
                FailedDiffs = failedDiffs,
                SucceededDiffs = succeededDiffs,
                RecentJobs = recentJobs.Take(5).ToList()
            };
        }
    }
}
