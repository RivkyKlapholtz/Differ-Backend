using Microsoft.AspNetCore.Mvc;
using DiffSpectrumView.DTOs;
using DiffSpectrumView.Services;
using Hangfire;

namespace DiffSpectrumView.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ComparisonController : ControllerBase
    {
        private readonly IApiComparisonService _comparisonService;

        public ComparisonController(IApiComparisonService comparisonService)
        {
            _comparisonService = comparisonService;
        }

        /// <summary>
        /// Receives duplication requests from Flapi and queues them for processing
        /// </summary>
        [HttpPost]
        public IActionResult ProcessDuplicationRequest([FromBody] DuplicationRequestDto request)
        {
            if (request == null)
                return BadRequest("Request cannot be null");

            // Queue the comparison job in Hangfire (fire and forget)
            BackgroundJob.Enqueue(() => _comparisonService.ProcessFlapiRequestAsync(request));

            // Return immediately (Flapi doesn't wait for response)
            return Accepted();
        }
    }
}
