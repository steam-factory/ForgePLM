using ForgePLM.Contracts.Eco;
using ForgePLM.Contracts.Revisions;

namespace ForgePLM.Runtime.Services
{
    public interface IEcoService
    {
        Task<List<EcoDto>> GetEcosByProjectAsync(int projectId);
        Task<List<RevisionDto>> GetEcoContentsAsync(int ecoId);

        Task<EcoDto> CreateEcoAsync(CreateEcoRequest request);
    }
}