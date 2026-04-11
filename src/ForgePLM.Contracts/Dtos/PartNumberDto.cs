using System;

namespace ForgePLM.Contracts.Dtos
{
    public class PartNumberDto
    {
        public int PartId { get; set; }

        public string CategoryCode { get; set; }

        public int PartNumberInt { get; set; }

        public string PartNumber { get; set; }  // computed display (CM-0000123)

        public string DocumentType { get; set; }  // PART / ASSEMBLY / DRAWING
    }
}