namespace ForgePLM.Runtime.Models;

public sealed class ArtifactWorkItem
{
    public int EcoId { get; set; }
    public string EcoNumber { get; set; } = string.Empty;
    public string EcoState { get; set; } = string.Empty;

    public int ProjectId { get; set; }
    public string ProjectCode { get; set; } = string.Empty;
    public string ProjectName { get; set; } = string.Empty;

    public int PartId { get; set; }
    public string CategoryCode { get; set; } = string.Empty;
    public int PartNumberInt { get; set; }
    public string DisplayPartNumber => $"{CategoryCode}-{PartNumberInt:0000000}";

    public int RevisionId { get; set; }
    public int RevisionCode { get; set; }
    public string DisplayCompositeCode => $"{DisplayPartNumber}-{RevisionCode}";

    public string Description { get; set; } = string.Empty;
    public string RevisionState { get; set; } = string.Empty;
    public string DocumentType { get; set; } = string.Empty;
}

