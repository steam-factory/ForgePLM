using ForgePLM.Contracts.Dtos;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace ForgePLM.SolidWorks.Addin.Services
{
    public class ForgePlmApiClient
    {
        private readonly HttpClient _http;

        public ForgePlmApiClient()
        {
            _http = new HttpClient
            {
                BaseAddress = new Uri("http://localhost:5269")
            };
        }

        public async Task<List<CustomerDto>> GetCustomersAsync()
        {

            var response = await _http.GetAsync("/api/customers");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<CustomerDto>>(json) ?? new List<CustomerDto>();
        }

        public async Task<List<ProjectDto>> GetProjectsByCustomerAsync(int customerId)
        {
            var response = await _http.GetAsync($"/api/customers/{customerId}/projects");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<ProjectDto>>(json) ?? new List<ProjectDto>();
        }

        public async Task<List<EcoDto>> GetEcosByProjectAsync(int projectId)
        {
            var response = await _http.GetAsync($"/api/projects/{projectId}/ecos");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<EcoDto>>(json) ?? new List<EcoDto>();
        }

        public async Task<List<RevisionDto>> GetEcoContentsAsync(int ecoId)
        {
            var response = await _http.GetAsync($"/api/ecos/{ecoId}/contents");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<RevisionDto>>(json) ?? new List<RevisionDto>();
        }
    }
}