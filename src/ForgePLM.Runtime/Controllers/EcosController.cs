using ForgePLM.Contracts.Eco;
using ForgePLM.Runtime.Services;
using Microsoft.AspNetCore.Mvc;

namespace ForgePLM.Runtime.Controllers
{
    [ApiController]
    [Route("api/ecos")]
    public class EcosController : ControllerBase
    {
        private readonly IEcoService _ecoService;

        public EcosController(IEcoService ecoService)
        {
            _ecoService = ecoService;
        }

        [HttpGet("{ecoId:int}/contents")]
        public async Task<IActionResult> GetEcoContents(int ecoId)
        {
            var result = await _ecoService.GetEcoContentsAsync(ecoId);
            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<EcoDto>> CreateEco([FromBody] CreateEcoRequest request)
        {
            var eco = await _ecoService.CreateEcoAsync(request);
            return Ok(eco);
        }
    }
}