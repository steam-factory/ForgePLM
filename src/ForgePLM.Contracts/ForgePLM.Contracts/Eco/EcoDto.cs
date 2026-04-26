using System;
namespace ForgePLM.Contracts.Eco;

public sealed record EcoDto(
    int EcoId,
    int ProjectId,
    int EcoNumberInt,
    string EcoNumber,
    string EcoTitle,
    string? EcoDescription,
    int ReleaseLevel,
    string EcoState,
    DateTime CreatedAt
);