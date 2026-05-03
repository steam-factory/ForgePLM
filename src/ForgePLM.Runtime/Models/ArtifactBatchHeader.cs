namespace ForgePLM.Runtime.Models
{
    public sealed class ArtifactBatchHeader
    {
        public int ArtifactBatchId { get; set; }
        public long BatchNumber { get; set; }
        public string BatchCode { get; set; } = string.Empty;
        public int EcoId { get; set; }
        public string BatchDescription { get; set; } = string.Empty;
        public string BatchState { get; set; } = string.Empty;
        public string OutputRootPath { get; set; } = string.Empty;
        public string? ZipFilePath { get; set; }
    }
}
