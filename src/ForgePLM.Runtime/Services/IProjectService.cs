using ForgePLM.Contracts.Dtos;

namespace ForgePLM.Runtime.Services
{
    public interface IProjectService
    {
        Task<List<ProjectDto>> GetProjectsByCustomerAsync(int customerId);
    }
}