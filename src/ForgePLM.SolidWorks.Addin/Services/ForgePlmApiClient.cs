using ForgePLM.Contracts.Revisions;
using ForgePLM.Contracts.Requests;
using ForgePLM.Contracts.Responses;
using ForgePLM.Contracts.Customers;
using ForgePLM.Contracts.Projects;
using ForgePLM.Contracts.Eco;
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

        public async Task<bool> IsRuntimeAvailableAsync()
        {
            try
            {
                var response = await _http.GetAsync("/health");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<AssignRevisionResponse> AssignRevisionAsync(AssignRevisionRequest request)
        {
            var jsonContent = JsonConvert.SerializeObject(request);

            var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

            var response = await _http.PostAsync("/api/revisions/assign", content);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();

            //var result = JsonConvert.DeserializeObject<AssignRevisionResponse>(json);

            return JsonConvert.DeserializeObject<AssignRevisionResponse>(json)
       ?? throw new InvalidOperationException("Failed to deserialize AssignRevisionResponse.");
        }

        public async Task<OpenRevisionResponse> GetOpenInfoAsync(int revisionId)
        {
            var response = await _http.GetAsync($"/api/revisions/{revisionId}/open-info");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();

            //var result = JsonConvert.DeserializeObject<OpenRevisionResponse>(json);

            return JsonConvert.DeserializeObject<OpenRevisionResponse>(json)
        ?? throw new InvalidOperationException("Failed to deserialize OpenRevisionResponse.");
        }


    }
}