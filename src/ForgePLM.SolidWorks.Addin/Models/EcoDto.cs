namespace ForgePLM.SolidWorks.Addin.Models
{
    public class EcoDto
    {
        public int EcoId { get; set; }
        public string EcoNumber { get; set; }
        public string EcoState { get; set; }
        public int ReleaseLevel { get; set; }

        public string DisplayName
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(EcoState))
                    return $"{EcoNumber} [{EcoState}]";
                return EcoNumber ?? string.Empty;
            }
        }
    }
}