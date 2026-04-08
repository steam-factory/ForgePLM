using System;
using System.Collections.Generic;
using System.Text;

namespace ForgePLM.Contracts.Dtos
{
    public class RevisionDto
    {
        public int PartId { get; set; }
        public int RevisionId { get; set; }
        public int EcoId { get; set; }

        public string CategoryCode { get; set; }
        public int PartNumberInt { get; set; }
        public string PartNumber { get; set; }

        public string RevisionCode { get; set; }
        public string Description { get; set; }
        public string EcoNumber { get; set; }
        public string FilePath { get; set; }
    }
}
