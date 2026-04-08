using System;
using System.Collections.Generic;
using System.Text;

namespace ForgePLM.Contracts.Responses
{
    public class OpenRevisionResponse
    {
        public int RevisionId { get; set; }
        public string FilePath { get; set; }
        public string PartNumber { get; set; }
        public string RevisionCode { get; set; }
    }
}
