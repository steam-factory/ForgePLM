namespace ForgePLM.Contracts.Parts
{
    public class PartNumberDto
    {
        public int PartId { get; set; }

        public string CategoryCode { get; set; } = string.Empty;

        public int PartNumberInt { get; set; }

        public string PartNumber { get; set; } = string.Empty;

        public string DocumentType { get; set; } = string.Empty;
    }
}