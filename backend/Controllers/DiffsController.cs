using Microsoft.AspNetCore.Mvc;
using DiffSpectrumView.Services;

namespace DiffSpectrumView.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DiffsController : ControllerBase
    {
        private readonly IDiffService _diffService;

        public DiffsController(IDiffService diffService)
        {
            _diffService = diffService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var diffs = await _diffService.GetAllDiffsAsync();
            return Ok(diffs);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var diff = await _diffService.GetDiffByIdAsync(id);
            if (diff == null)
                return NotFound();

            return Ok(diff);
        }

        [HttpGet("job/{jobId}")]
        public async Task<IActionResult> GetByJobId(int jobId)
        {
            var diffs = await _diffService.GetDiffsByJobIdAsync(jobId);
            return Ok(diffs);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _diffService.DeleteDiffAsync(id);
            return NoContent();
        }

        [HttpPost("{id}/restore")]
        public async Task<IActionResult> Restore(int id)
        {
            await _diffService.RestoreDiffAsync(id);
            return NoContent();
        }

        [HttpPatch("{id}/checked")]
        public async Task<IActionResult> UpdateCheckedStatus(int id, [FromBody] bool isChecked)
        {
            await _diffService.UpdateDiffCheckedStatusAsync(id, isChecked);
            return NoContent();
        }
    }
}
