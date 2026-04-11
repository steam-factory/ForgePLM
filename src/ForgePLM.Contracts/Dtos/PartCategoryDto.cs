using System;
using System.Collections.Generic;
using System.Text;

namespace ForgePLM.Contracts.Dtos
{
    public class PartCategoryDto
    {
        public int CategoryId { get; set; }
        public string CategoryCode { get; set; }
        public string CategoryName { get; set; }
        public bool IsActive { get; set; }
    }
}