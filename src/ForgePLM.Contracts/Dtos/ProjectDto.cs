using System;
using System.Collections.Generic;
using System.Text;

namespace ForgePLM.Contracts.Dtos
{
    public class ProjectDto
    {
        public int ProjectId { get; set; }
        public int CustomerId { get; set; }
        public string ProjectCode { get; set; }
        public string ProjectName { get; set; }
    }
}