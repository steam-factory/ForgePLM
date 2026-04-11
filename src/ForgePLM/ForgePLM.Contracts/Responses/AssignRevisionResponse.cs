namespace ForgePLM.Contracts.Responses
{
    public class AssignRevisionResponse
    {
        public int PartId { get; set; }

        public int RevisionId { get; set; }

        public string Guid { get; set; } = string.Empty;

        public string PartNumber { get; set; } = string.Empty;
        public string RevisionCode { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string EcoNumber { get; set; } = string.Empty;
    }
}