using ForgePLM.Contracts.PartCategories;
using ForgePLM.Runtime.Services;
using Microsoft.AspNetCore.Mvc;

namespace ForgePLM.Runtime.Controllers
{
    [ApiController]
    [Route("api/part-categories")]
    public class PartCategoriesController : ControllerBase
    {
        private readonly IPartCategoryService _service;

        public PartCategoriesController(IPartCategoryService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<List<PartCategoryDto>>> GetPartCategories()
        {
            var categories = await _service.GetPartCategoriesAsync();
            return Ok(categories);
        }
    }
}