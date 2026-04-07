using ForgePLM.SolidWorks.Addin.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

public class ForgePlmApiClient
{
    private readonly HttpClient _http;

    public ForgePlmApiClient()
    {
        _http = new HttpClient
        {
            BaseAddress = new Uri("https://your-api")
        };
    }

    public async Task<List<RevisionDto>> GetEcoContents(int ecoId)
    {
        var response = await _http.GetAsync($"/api/eco/{ecoId}/contents");
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<List<RevisionDto>>(json);
    }
}