using System;

namespace ForgePLM.Contracts.Parts
{
    public sealed record PartNumberManagerItemDto(
        int PartId,
        int RevisionId,
        string PartNumber,
        string RevisionCode,
        string CompositeCode,
        string Description,
        string DocumentType,
        string RevisionState,
        int RevisionFamily,
        int EcoId,
        string EcoNumber,
        string EcoState,
        int ProjectId,
        string ProjectCode,
        string ProjectName,
        int CustomerId,
        string CustomerCode,
        string CustomerName,
        bool CanEditDescription
    );
}