


namespace ForgePLM.Administrator.Views
{
    public class ArtifactPartRowViewModel
    {
        public bool IsSelected { get; set; }
        public int PartId { get; set; }
        public int RevisionId { get; set; }
        public int EcoId { get; set; }
        public string DisplayCompositeCode { get; set; } = string.Empty;
        public string PartNumber { get; set; } = string.Empty;
        public string Revision { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string RevisionState { get; set; } = string.Empty;
        public string DocumentType { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;

        public string SourceFilePath { get; set; } = string.Empty;
    }
}