using System;
using System.Collections.Generic;
using System.Text;

namespace ForgePLM.Contracts.Revisions;

public sealed record PartRevisionItemDto(
    int PartId,
    int RevisionId,
    string CategoryCode,
    int PartNumberInt,
    int RevisionCode,
    int RevisionFamily,
    int RevisionSeq,
    string RevisionState,
    string CompositeCode,
    string Description,
    string EcoNumber,
    string DocumentType
)
{
    public string DisplayPartNumber => $"{CategoryCode}-{PartNumberInt:0000000}";
    public string DisplayCompositeCode => $"{CategoryCode}-{PartNumberInt:0000000}-{RevisionCode}";
};