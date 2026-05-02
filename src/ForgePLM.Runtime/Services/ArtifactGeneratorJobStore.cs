using ForgePLM.Contracts.Artifacts;
using System.Collections.Concurrent;

namespace ForgePLM.Runtime.Services;

public class ArtifactGenerationJobStore : IArtifactGenerationJobStore
{
    private readonly ConcurrentDictionary<Guid, ArtifactGenerationJobDto> _jobs = new();

    public ArtifactGenerationJobDto CreateQueuedJob()
    {
        var job = new ArtifactGenerationJobDto(
            Guid.NewGuid(),
            "queued",
            0,
            0,
            "Queued.",
            null);

        _jobs[job.JobId] = job;
        return job;
    }

    public ArtifactGenerationJobDto? GetJob(Guid jobId)
        => _jobs.TryGetValue(jobId, out var job) ? job : null;

    public void Update(Guid jobId, string status, int completed, int total, string message)
    {
        _jobs[jobId] = new ArtifactGenerationJobDto(
            jobId, status, total, completed, message, null);
    }

    public void Complete(Guid jobId, ArtifactBatchDto result)
    {
        _jobs[jobId] = new ArtifactGenerationJobDto(
            jobId, "completed", result.Artifacts.Count, result.Artifacts.Count,
            $"Completed {result.BatchCode}.", result);
    }

    public void Fail(Guid jobId, string message)
    {
        _jobs[jobId] = new ArtifactGenerationJobDto(
            jobId, "failed", 0, 0, message, null);
    }
}