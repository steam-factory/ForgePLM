using ForgePLM.Contracts.Parts;
using ForgePLM.Runtime.Services;
using Microsoft.AspNetCore.Mvc;

namespace ForgePLM.Runtime.Controllers
{
    [ApiController]
    [Route("api/parts")]
    public class PartsController : ControllerBase
    {
        private readonly IPartService _partService;

        public PartsController(IPartService partService)
        {
            _partService = partService;
        }

        [HttpGet("by-project/{projectId:int}")]
        public async Task<ActionResult<List<ProjectPartCurrentDto>>> GetByProject(
            int projectId,
            CancellationToken cancellationToken)
        {
            var results = await _partService.GetProjectPartsCurrentAsync(projectId, cancellationToken);
            return Ok(results);
        }
    }
}