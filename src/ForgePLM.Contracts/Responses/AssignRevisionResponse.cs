using System;
using System.Collections.Generic;
using System.Text;

namespace ForgePLM.Contracts.Responses
{
    public class AssignRevisionResponse
    {
        public string Guid { get; set; }
        public int PartId { get; set; }
        public int RevisionId { get; set; }
        public string PartNumber { get; set; }
        public string RevisionCode { get; set; }
        public string Description { get; set; }
        public string EcoNumber { get; set; }
    }
}