namespace ForgePLM.Contracts.Eco;

public sealed record CreateEcoRequest(
        string ProjectCode,
        string EcoTitle,
        string? EcoDescription,
        int ReleaseLevel
    );