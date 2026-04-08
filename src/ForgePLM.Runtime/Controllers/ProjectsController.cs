using ForgePLM.Runtime.Services;
using Microsoft.AspNetCore.Mvc;

namespace ForgePLM.Runtime.Controllers
{
    [ApiController]
    [Route("api/projects")]
    public class ProjectsController : ControllerBase
    {
        private readonly IEcoService _ecoService;

        public ProjectsController(IEcoService ecoService)
        {
            _ecoService = ecoService;
        }

        [HttpGet("{projectId:int}/ecos")]
        public async Task<IActionResult> GetEcosByProject(int projectId)
        {
            var result = await _ecoService.GetEcosByProjectAsync(projectId);
            return Ok(result);
        }
    }
}