namespace ForgePLM.SolidWorks.Addin.Models
{
    public class CustomerDto
    {
        public int CustomerId { get; set; }
        public string CustomerCode { get; set; }
        public string CustomerName { get; set; }

        public string DisplayName
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(CustomerCode))
                    return $"{CustomerCode} - {CustomerName}";
                return CustomerName ?? string.Empty;
            }
        }
    }
}