using ForgePLM.Contracts.Dtos;

namespace ForgePLM.Runtime.Services
{
    public interface ICustomerService
    {
        Task<List<CustomerDto>> GetCustomersAsync();
    }
}