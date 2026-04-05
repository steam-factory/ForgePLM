using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net.Http.Json;
using ForgePLM.Contracts.Parts;
using ForgePLM.Contracts.PartCategories;
using ForgePLM.Contracts.Customers;

namespace ForgePLM.Administrator.Services
{
   
    public class ForgePlmAdminApiClient
    {
        private readonly HttpClient _httpClient;

        public ForgePlmAdminApiClient()
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri("http://127.0.0.1:5217")
            };
        }

        public async Task<string> GetHealthAsync()
        {
            var response = await _httpClient.GetAsync("/api/health");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<CreatePartResponse> CreatePartAsync(CreatePartRequest request)
        {
            var response = await _httpClient.PostAsJsonAsync("/api/parts", request);
            response.EnsureSuccessStatusCode();

            var payload = await response.Content.ReadFromJsonAsync<CreatePartEnvelope>();

            if (payload?.Data is null)
                throw new InvalidOperationException("Service returned no part data.");

            return payload.Data;
        }

        public async Task<IReadOnlyList<PartCategoryDto>> GetPartCategoriesAsync()
        {
            var response = await _httpClient.GetAsync("/api/part-categories");
            response.EnsureSuccessStatusCode();

            var payload = await response.Content.ReadFromJsonAsync<PartCategoryEnvelope>();

            return payload?.Data ?? new List<PartCategoryDto>();
        }

        public async Task<List<CustomerDto>> GetCustomersAsync()
        {
            var response = await _httpClient.GetFromJsonAsync<List<CustomerDto>>("/api/customers");
            return response ?? new List<CustomerDto>();
        }

        public async Task CreateCustomerAsync(CustomerDto customer)
        {
            var response = await _httpClient.PostAsJsonAsync("/api/customers", customer);
            response.EnsureSuccessStatusCode();
        }

        public async Task UpdateCustomerAsync(CustomerDto customer)
        {
            var response = await _httpClient.PutAsJsonAsync($"/api/customers/{customer.CustomerId}", customer);
            response.EnsureSuccessStatusCode();
        }


        private sealed record CustomerEnvelope(
            bool Success,
            List<CustomerDto> Data,
            string TraceId
        );

        private sealed record CreateCustomerEnvelope(
            bool Success,
            CustomerDto Data,
            string TraceId
        );

        private sealed record PartCategoryEnvelope(
            bool Success,
            List<PartCategoryDto> Data,
            string TraceId
        );

        private sealed record CreatePartEnvelope(
            bool Success,
            CreatePartResponse Data,
            string TraceId
        );
    }




}