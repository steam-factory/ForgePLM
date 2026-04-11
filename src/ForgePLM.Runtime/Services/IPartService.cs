using ForgePLM.Contracts.Parts;

namespace ForgePLM.Runtime.Services
{
    public interface IPartService
    {
        Task<List<ProjectPartCurrentDto>> GetProjectPartsCurrentAsync(
            int projectId,
            CancellationToken cancellationToken = default);
    }
}