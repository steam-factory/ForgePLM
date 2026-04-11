using System;
using System.Collections.Generic;
using System.Text;

namespace ForgePLM.Contracts.Responses
{
    public class OpenRevisionResponse
    {
        public int RevisionId { get; set; }
        public string FilePath { get; set; } = string.Empty;
        public string PartNumber { get; set; } = string.Empty;
        public string RevisionCode { get; set; } = string.Empty;
        public string DocumentType { get; set; } = string.Empty;
    }
}
