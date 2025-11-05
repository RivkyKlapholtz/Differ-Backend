using DiffSpectrumView.DTOs;
using DiffSpectrumView.Repositories;

namespace DiffSpectrumView.Services
{
    public class JobService : IJobService
    {
        private readonly IJobRepository _jobRepository;
        private readonly IDiffRepository _diffRepository;

        public JobService(IJobRepository jobRepository, IDiffRepository diffRepository)
        {
            _jobRepository = jobRepository;
            _diffRepository = diffRepository;
        }

        public async Task<List<JobDto>> GetAllJobsAsync()
        {
            var jobs = await _jobRepository.GetAllAsync();
            return jobs.Select(j => new JobDto
            {
                Id = j.Id,
                StartTime = j.StartTime,
                EndTime = j.EndTime,
                Status = j.Status,
                FoundDiff = j.FoundDiff,
                DiffId = j.DiffId,
                ErrorMessage = j.ErrorMessage
            }).ToList();
        }

        public async Task<JobDto?> GetJobByIdAsync(int id)
        {
            var job = await _jobRepository.GetByIdAsync(id);
            if (job == null) return null;

            return new JobDto
            {
                Id = job.Id,
                StartTime = job.StartTime,
                EndTime = job.EndTime,
                Status = job.Status,
                FoundDiff = job.FoundDiff,
                DiffId = job.DiffId,
                ErrorMessage = job.ErrorMessage
            };
        }

        public async Task<JobsSummaryDto> GetJobsSummaryAsync()
        {
            var totalJobs = await _jobRepository.GetTotalJobsAsync();
            var successfulJobs = await _jobRepository.GetSuccessfulJobsAsync();
            var failedJobs = await _jobRepository.GetFailedJobsAsync();
            var jobsWithDiffs = await _jobRepository.GetJobsWithDiffsAsync();
            var recentJobs = await GetAllJobsAsync();

            return new JobsSummaryDto
            {
                TotalJobs = totalJobs,
                SuccessfulJobs = successfulJobs,
                FailedJobs = failedJobs,
                JobsWithDiffs = jobsWithDiffs,
                JobsWithoutDiffs = successfulJobs - jobsWithDiffs,
                RecentJobs = recentJobs.Take(5).ToList()
            };
        }
    }
}
