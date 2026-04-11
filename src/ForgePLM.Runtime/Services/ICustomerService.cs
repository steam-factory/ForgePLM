using ForgePLM.Contracts.Customers;

namespace ForgePLM.Runtime.Services
{
    public interface ICustomerService
    {
        Task<List<CustomerDto>> GetCustomersAsync();
    }
}