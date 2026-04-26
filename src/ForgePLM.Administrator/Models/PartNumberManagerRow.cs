namespace ForgePLM.Administrator.Models
{
    public class PartNumberManagerRow
    {
        public int PartId { get; set; }
        public int RevisionId { get; set; }
        public string PartNumber { get; set; } = string.Empty;
        public string RevisionCode { get; set; } = string.Empty;
        public string CompositeCode { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string DocumentType { get; set; } = string.Empty;
        public string RevisionState { get; set; } = string.Empty;
        public int RevisionFamily { get; set; }
        public string EcoNumber { get; set; } = string.Empty;
        public string ProjectCode { get; set; } = string.Empty;
        public string CustomerCode { get; set; } = string.Empty;
        public bool CanEditDescription { get; set; }
    }
}