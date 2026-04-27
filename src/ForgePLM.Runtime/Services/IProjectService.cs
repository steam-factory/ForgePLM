using ForgePLM.Contracts.Projects;

namespace ForgePLM.Runtime.Services
{
    public interface IProjectService
    {
        Task<List<ProjectDto>> GetProjectsByCustomerAsync(int customerId);
        Task<ProjectDto> CreateProjectAsync(CreateProjectRequest request);
    }
}