using ForgePLM.Contracts.System;
using ForgePLM.Contracts.Projects;
using Microsoft.Extensions.Options;

namespace ForgePLM.Runtime.Services;

public class VaultPathResolver : IVaultPathResolver
{
    private readonly VaultPathSettings _settings;

    public VaultPathResolver(IOptions<VaultPathSettings> options)
    {
        _settings = options.Value;
    }

    public string GetProjectFolder(ProjectDto project)
    {
        string projectFolder = $"{project.ProjectCode} - {project.ProjectName}";

        return Path.Combine(
            _settings.RootPath,
            _settings.ProjectsFolder,
            projectFolder
        );
    }

    public string GetDevelopmentFilePath(ProjectDto project, string displayPartNumber, string extension)
    {
        return Path.Combine(
            GetProjectFolder(project),
            _settings.DevelopmentFolder,
            $"{displayPartNumber}.{extension}"
        );
    }

    public string GetProductionFolder(ProjectDto project, string ecoNumber)
    {
        return Path.Combine(
            GetProjectFolder(project),
            _settings.ProductionFolder,
            ecoNumber
        );
    }
}