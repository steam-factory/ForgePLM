using System;

namespace ForgePLM.Contracts.Dtos
{
    public class ProjectPartCurrentDto
    {
        public int PartId { get; set; }
        public int RevisionId { get; set; }

        public string CategoryCode { get; set; }
        public int PartNumberInt { get; set; }
        public string PartNumber { get; set; }

        public string RevisionCode { get; set; }
        public string Description { get; set; }

        public string RevisionState { get; set; }
        public int RevisionFamily { get; set; }

        public string DocumentType { get; set; }

        public bool CanSelect { get; set; }
        public string AvailabilityReason { get; set; }

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