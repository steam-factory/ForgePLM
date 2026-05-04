using ForgePLM.Contracts.Artifacts;
using ForgePLM.Runtime.Services;
using Microsoft.AspNetCore.Mvc;

namespace ForgePLM.Runtime.Controllers;

[ApiController]
[Route("api/artifact-batches")]
public class ArtifactBatchesController : ControllerBase
{
    private readonly IArtifactBatchService _artifactBatchService;
    private readonly IArtifactGenerationJobStore _jobStore;

    public ArtifactBatchesController(
        IArtifactBatchService artifactBatchService,
        IArtifactGenerationJobStore jobStore)
    {
        _artifactBatchService = artifactBatchService;
        _jobStore = jobStore;
    }

    [HttpPost("generate")]
    public async Task<ActionResult<ArtifactBatchDto>> Generate(
        [FromBody] GenerateArtifactBatchRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _artifactBatchService.GenerateArtifactBatchAsync(
            request,
            cancellationToken);

        return Ok(result);
    }

    [HttpPost("jobs")]
    public ActionResult<ArtifactGenerationJobDto> StartJob(
        [FromBody] GenerateArtifactBatchRequest request)
    {
        var job = _jobStore.CreateQueuedJob();

        _ = Task.Run(async () =>
        {
            try
            {
                var result = await _artifactBatchService.GenerateArtifactBatchAsync(
                    request,
                    job.JobId,
                    CancellationToken.None);

                _jobStore.Complete(job.JobId, result);
            }
            catch (Exception ex)
            {
                _jobStore.Fail(job.JobId, ex.Message);
            }
        });

        return Ok(job);
    }

    [HttpGet("jobs/{jobId:guid}")]
    public ActionResult<ArtifactGenerationJobDto> GetJob(Guid jobId)
    {
        var job = _jobStore.GetJob(jobId);

        if (job == null)
            return NotFound();

        return Ok(job);
    }

    [HttpGet("artifacts/by-revision/{revisionId:int}")]
    public async Task<ActionResult<IReadOnlyList<ArtifactDto>>> GetArtifactsByRevision(
    int revisionId,
    CancellationToken cancellationToken)
    {
        var result = await _artifactBatchService.GetArtifactsByRevisionAsync(
            revisionId,
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("by-eco/{ecoId:int}")]
    public async Task<ActionResult<IReadOnlyList<ArtifactBatchDto>>> GetByEco(
    int ecoId,
    CancellationToken cancellationToken)
    {
        var result = await _artifactBatchService.GetArtifactBatchesByEcoAsync(
            ecoId,
            cancellationToken);

        return Ok(result);
    }


}