using ForgePLM.Contracts.Artifacts;

namespace ForgePLM.Runtime.Services;

public interface IArtifactBatchService
{
    Task<ArtifactBatchDto> GenerateArtifactBatchAsync(
        GenerateArtifactBatchRequest request,
        CancellationToken cancellationToken = default);


    Task<ArtifactBatchDto> GenerateArtifactBatchAsync(
        GenerateArtifactBatchRequest request,
        Guid jobId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ArtifactBatchDto>> GetArtifactBatchesByEcoAsync(
        int ecoId,
        CancellationToken cancellationToken = default);
}