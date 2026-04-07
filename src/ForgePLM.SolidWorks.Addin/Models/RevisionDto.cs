using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForgePLM.SolidWorks.Addin.Models
{
    public class RevisionDto
    {
        public int PartId { get; set; }
        public int RevisionId { get; set; }

        public string PartNumber { get; set; }
        public string RevisionCode { get; set; }
        public string Description { get; set; }
        public string EcoNumber { get; set; }
    }
}