using ForgePLM.Contracts.Projects;
using ForgePLM.Runtime.Services;
using Microsoft.AspNetCore.Mvc;

namespace ForgePLM.Runtime.Controllers
{
    [ApiController]
    [Route("api/projects")]
    public class ProjectsController : ControllerBase
    {
        private readonly IEcoService _ecoService;
        private readonly IProjectService _projectService;

        public ProjectsController(
            IEcoService ecoService,
            IProjectService projectService)
        {
            _ecoService = ecoService;
            _projectService = projectService;
        }

        [HttpGet("{projectId:int}/ecos")]
        public async Task<IActionResult> GetEcosByProject(int projectId)
        {
            var result = await _ecoService.GetEcosByProjectAsync(projectId);
            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<ProjectDto>> CreateProject([FromBody] CreateProjectRequest request)
        {
            var project = await _projectService.CreateProjectAsync(request);
            return Ok(project);
        }
    }
}