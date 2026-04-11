using System;
using System.Collections.Generic;
using System.Text;

namespace ForgePLM.Contracts.Dtos
{
    public class PartRevisionItemDto
    {
        public int PartId { get; set; }
        public int RevisionId { get; set; }

        public string CategoryCode { get; set; }
        public int PartNumberInt { get; set; }
        public string RevisionCode { get; set; }

        public string Description { get; set; }
        public string EcoNumber { get; set; }
        public string DocumentType { get; set; }

        public string DisplayPartNumber
        {
            get { return $"{CategoryCode}-{PartNumberInt:D7}"; }
        }

        public string DisplayPartRevision
        {
            get { return $"{CategoryCode}-{PartNumberInt:D7}-{RevisionCode}"; }
        }
    }
}