using ForgePLM.Contracts.Parts;
using ForgePLM.Contracts.Revisions;

namespace ForgePLM.Runtime.Services
{
    public interface IPartService
    {
        Task<List<ProjectPartCurrentDto>> GetProjectPartsCurrentAsync(
            int projectId,
            CancellationToken cancellationToken = default);

        Task<PartRevisionItemDto> CreatePartAsync(
            CreatePartRequest request,
            CancellationToken cancellationToken = default);
    }
}