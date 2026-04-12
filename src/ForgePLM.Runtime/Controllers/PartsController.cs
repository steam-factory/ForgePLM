using ForgePLM.Contracts.Parts;
using ForgePLM.Contracts.Requests;
using ForgePLM.Runtime.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ForgePLM.Runtime.Controllers
{
    [ApiController]
    [Route("api/parts")]
    public class PartsController : ControllerBase
    {
        private readonly IPartService _partService;
        private readonly IPartManagerService _partManagerService;

        public PartsController(
            IPartService partService,
            IPartManagerService partManagerService)
        {
            _partService = partService;
            _partManagerService = partManagerService;
        }

        [HttpGet]
        public async Task<ActionResult<List<PartNumberManagerItemDto>>> GetAll(CancellationToken cancellationToken)
        {
            var results = await _partManagerService.GetPartNumberManagerItemsAsync(cancellationToken);
            return Ok(results);
        }

        [HttpGet("by-project/{projectId:int}")]
        public async Task<ActionResult<List<ProjectPartCurrentDto>>> GetByProject(
            int projectId,
            CancellationToken cancellationToken)
        {
            var results = await _partService.GetProjectPartsCurrentAsync(projectId, cancellationToken);
            return Ok(results);
        }

        [HttpPut("/api/revisions/{revisionId:int}/description")]
        public async Task<ActionResult<PartNumberManagerItemDto>> UpdateDescription(
            int revisionId,
            [FromBody] UpdateRevisionDescriptionRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _partManagerService.UpdateRevisionDescriptionAsync(
                revisionId,
                request.Description,
                cancellationToken);

            return Ok(result);
        }
    }
}