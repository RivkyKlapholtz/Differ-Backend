using Microsoft.AspNetCore.Mvc;
using DiffSpectrumView.Services;

namespace DiffSpectrumView.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class JobsController : ControllerBase
    {
        private readonly IJobService _jobService;

        public JobsController(IJobService jobService)
        {
            _jobService = jobService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var jobs = await _jobService.GetAllJobsAsync();
            return Ok(jobs);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var job = await _jobService.GetJobByIdAsync(id);
            if (job == null)
                return NotFound();

            return Ok(job);
        }

        [HttpGet("summary")]
        public async Task<IActionResult> GetSummary()
        {
            var summary = await _jobService.GetJobsSummaryAsync();
            return Ok(summary);
        }
    }
}
