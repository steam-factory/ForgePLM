using ForgePLM.Contracts.Projects;

namespace ForgePLM.Runtime.Services
{
    public interface IVaultPathResolver
    {
        string GetProjectFolder(ProjectDto project);
        string GetDevelopmentFilePath(ProjectDto project, string displayPartNumber, string extension);
        string GetProductionFolder(ProjectDto project, string ecoNumber);
    }
}
