using System;
using System.Collections.Generic;
using System.Text;

namespace ForgePLM.Contracts.Dtos
{
    public class CustomerDto
    {
        public int CustomerId { get; set; }
        public string CustomerCode { get; set; }
        public string CustomerName { get; set; }
    }
}