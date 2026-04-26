using ForgePLM.Contracts.Parts;
using ForgePLM.Contracts.Revisions;

namespace ForgePLM.Contracts.Responses
{
    public sealed class CreatePartEnvelope
    {
        public bool Success { get; set; }

        public PartRevisionItemDto Data { get; set; } = default!;

        public string TraceId { get; set; } = string.Empty;
    }
}