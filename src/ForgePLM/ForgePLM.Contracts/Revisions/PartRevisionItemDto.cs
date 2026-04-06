using System;
using System.Collections.Generic;
using System.Text;

namespace ForgePLM.Contracts.Parts;

public sealed record PartRevisionItemDto(
    int PartId,
    string CategoryCode,
    int PartNumberInt,
    string PartNumber,
    int RevisionId,
    int RevisionCode,
    int RevisionFamily,
    int RevisionSeq,
    string RevisionState,
    string CompositeCode,
    string Description
);