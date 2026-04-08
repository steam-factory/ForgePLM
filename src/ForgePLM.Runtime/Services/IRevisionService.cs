using ForgePLM.Contracts.Requests;
using ForgePLM.Contracts.Responses;

namespace ForgePLM.Runtime.Services
{
    public interface IRevisionService
    {
        Task<AssignRevisionResponse> AssignRevisionAsync(AssignRevisionRequest request);
        Task<OpenRevisionResponse> GetOpenInfoAsync(int revisionId);
    }
}