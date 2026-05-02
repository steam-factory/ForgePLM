using ForgePLM.Contracts.Artifacts;

namespace ForgePLM.Runtime.Services
{
    public interface IArtifactGenerationJobStore
    {
        ArtifactGenerationJobDto CreateQueuedJob();
        ArtifactGenerationJobDto? GetJob(Guid jobId);
        void Update(Guid jobId, string status, int completed, int total, string message);
        void Complete(Guid jobId, ArtifactBatchDto result);
        void Fail(Guid jobId, string message);
    }
}
