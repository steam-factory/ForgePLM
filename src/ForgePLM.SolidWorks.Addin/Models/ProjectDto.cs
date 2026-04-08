namespace ForgePLM.SolidWorks.Addin.Models
{
    public class ProjectDto
    {
        public int ProjectId { get; set; }
        public string ProjectCode { get; set; }
        public string ProjectName { get; set; }

        public string DisplayName
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(ProjectCode))
                    return $"{ProjectCode} - {ProjectName}";
                return ProjectName ?? string.Empty;
            }
        }
    }
}