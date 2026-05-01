using System;
using System.Collections.Generic;
using System.Text;

namespace ForgePLM.Contracts.Eco
{
    public class EcoContentDto
    {
        public int EcoId { get; set; }
        public int PartId { get; set; }
        public int RevisionId { get; set; }

        public string PartNumber { get; set; } = string.Empty;
        public string Revision { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string RevisionState { get; set; } = string.Empty;
        public string DocumentType { get; set; } = string.Empty;
    }
}