using System;
using System.Collections.Generic;
using System.Text;

namespace ForgePLM.Contracts.Revisions
{
    public class RevisionDto
    {
        public int PartId { get; set; }
        public int RevisionId { get; set; }
        public int EcoId { get; set; }

        public string CategoryCode { get; set; } = string.Empty;
        public int PartNumberInt { get; set; }
        public string PartNumber { get; set; } = string.Empty;

        public string RevisionCode { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string EcoNumber { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;

        public string DocumentType { get; set; } = string.Empty;
    }
}
