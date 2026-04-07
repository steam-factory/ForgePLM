using System;
using System.Collections.Generic;
using System.Text;

namespace ForgePLM.Contracts.Parts;

public sealed record ProjectPartCurrentDto(
    int PartId,
    string CategoryCode,
    int PartNumberInt,
    string PartNumber,
    int CurrentRevisionId,
    int RevisionCode,
    int RevisionFamily,
    string RevisionState,
    int EcoId,
    string EcoNumber,
    string EcoState,
    string Description,
    string CompositeCode,
    bool CanSelect,
    string? AvailabilityReason
);