using ForgePLM.Contracts.Dtos;

namespace ForgePLM.Runtime.Services
{
    public interface IEcoService
    {
        Task<List<EcoDto>> GetEcosByProjectAsync(int projectId);
        Task<List<RevisionDto>> GetEcoContentsAsync(int ecoId);
    }
}