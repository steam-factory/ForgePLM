namespace ForgePLM.Contracts.Eco;

public sealed record UpdateEcoRequest(
    string EcoTitle,
    string? EcoDescription,
    int ReleaseLevel,
    string EcoState
);