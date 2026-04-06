using ForgePLM.Contracts.Customers;
using ForgePLM.Contracts.PartCategories;
using ForgePLM.Contracts.Parts;
using ForgePLM.Contracts.Projects;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows;
using ForgePLM.Contracts.Eco;

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

        
        //Project Helpers
        public async Task<IReadOnlyList<ProjectDto>> GetProjectsByCustomerAsync(
            int customerId,
            CancellationToken cancellationToken = default)
        {
            var url = $"api/projects/by-customer/{customerId}";
            var projects = await _httpClient.GetFromJsonAsync<List<ProjectDto>>(
                $"api/projects/by-customer/{customerId}",
                cancellationToken);
            return projects ?? new List<ProjectDto>();
        }

        public async Task<ProjectDto> CreateProjectAsync(
            CreateProjectRequest request,
            CancellationToken cancellationToken = default)
        {
            using var response = await _httpClient.PostAsJsonAsync(
                "api/projects",
                request,
                cancellationToken);

            response.EnsureSuccessStatusCode();

            var project = await response.Content.ReadFromJsonAsync<ProjectDto>(
                cancellationToken: cancellationToken);

            return project ?? throw new InvalidOperationException("Project API returned no body.");
        }

        public async Task<ProjectDto> UpdateProjectAsync(
            int projectId,
            UpdateProjectRequest request,
            CancellationToken cancellationToken = default)
        {
            using var response = await _httpClient.PutAsJsonAsync(
                $"/api/projects/{projectId}",
                request,
                cancellationToken);

            response.EnsureSuccessStatusCode();

            var project = await response.Content.ReadFromJsonAsync<ProjectDto>(
                cancellationToken: cancellationToken);

            return project ?? throw new InvalidOperationException("Project API returned no body.");
        }

        public async Task<IReadOnlyList<EcoDto>> GetEcosByProjectAsync(
    int projectId,
    CancellationToken cancellationToken = default)
        {
            var response = await _httpClient.GetFromJsonAsync<List<EcoDto>>(
                $"/api/eco/by-project/{projectId}",
                cancellationToken);

            return response ?? new List<EcoDto>();
        }

        public async Task<EcoDto> CreateEcoAsync(
            CreateEcoRequest request,
            CancellationToken cancellationToken = default)
        {
            using var response = await _httpClient.PostAsJsonAsync(
                "/api/eco",
                request,
                cancellationToken);

            response.EnsureSuccessStatusCode();

            var eco = await response.Content.ReadFromJsonAsync<EcoDto>(
                cancellationToken: cancellationToken);

            return eco ?? throw new InvalidOperationException("ECO API returned no body.");
        }

        public async Task<EcoDto> UpdateEcoAsync(
            int ecoId,
            UpdateEcoRequest request,
            CancellationToken cancellationToken = default)
        {
            using var response = await _httpClient.PutAsJsonAsync(
                $"/api/eco/{ecoId}",
                request,
                cancellationToken);

            response.EnsureSuccessStatusCode();

            var eco = await response.Content.ReadFromJsonAsync<EcoDto>(
                cancellationToken: cancellationToken);

            return eco ?? throw new InvalidOperationException("ECO API returned no body.");
        }

        public async Task<PartRevisionItemDto> CreatePartUnderEcoAsync(
            ForgePLM.Contracts.Parts.CreatePartRequest request,
            CancellationToken cancellationToken = default)
        {
            using var response = await _httpClient.PostAsJsonAsync(
                "/api/parts",
                request,
                cancellationToken);

            var raw = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
                throw new InvalidOperationException($"HTTP {(int)response.StatusCode}: {raw}");

            var envelope = await response.Content.ReadFromJsonAsync<CreatePartEnvelope>(
                cancellationToken: cancellationToken);

            return envelope?.Data ?? throw new InvalidOperationException("Part API returned no body.");
        }

        public async Task<IReadOnlyList<PartRevisionItemDto>> GetEcoContentsAsync(
            int ecoId,
            CancellationToken cancellationToken = default)
        {
            var response = await _httpClient.GetFromJsonAsync<List<PartRevisionItemDto>>(
                $"/api/parts/by-eco/{ecoId}",
                cancellationToken);

            return response ?? new List<PartRevisionItemDto>();
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
            PartRevisionItemDto Data,
            string TraceId
        );


    }




}