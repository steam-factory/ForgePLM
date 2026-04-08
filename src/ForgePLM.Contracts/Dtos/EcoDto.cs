using System;
using System.Collections.Generic;
using System.Text;

namespace ForgePLM.Contracts.Dtos
{
    public class EcoDto
    {
        public int EcoId { get; set; }
        public int ProjectId { get; set; }
        public string EcoNumber { get; set; }
        public string EcoState { get; set; }
        public int ReleaseLevel { get; set; }
    }
}