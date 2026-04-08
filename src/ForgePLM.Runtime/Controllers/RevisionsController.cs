using ForgePLM.Contracts.Requests;
using ForgePLM.Runtime.Services;
using Microsoft.AspNetCore.Mvc;

namespace ForgePLM.Runtime.Controllers
{
    [ApiController]
    [Route("api/revisions")]
    public class RevisionsController : ControllerBase
    {
        private readonly IRevisionService _revisionService;

        public RevisionsController(IRevisionService revisionService)
        {
            _revisionService = revisionService;
        }

        [HttpPost("assign")]
        public async Task<IActionResult> Assign([FromBody] AssignRevisionRequest request)
        {
            var result = await _revisionService.AssignRevisionAsync(request);
            return Ok(result);
        }

        [HttpGet("{revisionId:int}/open-info")]
        public async Task<IActionResult> GetOpenInfo(int revisionId)
        {
            var result = await _revisionService.GetOpenInfoAsync(revisionId);
            return Ok(result);
        }
    }
}