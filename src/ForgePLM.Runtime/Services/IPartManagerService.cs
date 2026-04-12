using ForgePLM.Contracts.Parts;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ForgePLM.Runtime.Services
{
    public interface IPartManagerService
    {
        Task<List<PartNumberManagerItemDto>> GetPartNumberManagerItemsAsync(CancellationToken cancellationToken = default);

        Task<PartNumberManagerItemDto> UpdateRevisionDescriptionAsync(
            int revisionId,
            string description,
            CancellationToken cancellationToken = default);
    }
}