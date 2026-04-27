using ForgePLM.Contracts.Customers;
using ForgePLM.Runtime.Services;
using Microsoft.AspNetCore.Mvc;

namespace ForgePLM.Runtime.Controllers
{
    [ApiController]
    [Route("api/customers")]
    public class CustomersController : ControllerBase
    {
        private readonly ICustomerService _customerService;
        private readonly IProjectService _projectService;

        public CustomersController(ICustomerService customerService, IProjectService projectService)
        {
            _customerService = customerService;
            _projectService = projectService;
        }

        [HttpGet]
        public async Task<IActionResult> GetCustomers()
        {
            var result = await _customerService.GetCustomersAsync();
            return Ok(result);
        }

        [HttpGet("{customerId:int}/projects")]
        public async Task<IActionResult> GetProjectsByCustomer(int customerId)
        {
            var result = await _projectService.GetProjectsByCustomerAsync(customerId);
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> CreateCustomer([FromBody] CustomerDto customer)
        {
            await _customerService.CreateCustomerAsync(customer);
            return Ok();
        }
    }
}