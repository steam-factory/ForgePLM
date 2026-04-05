namespace ForgePLM.Contracts.Customers;

public record CreateCustomerRequest(
    string CustomerCode,
    string CustomerName,
    string? ContactName,
    string? ContactEmail,
    string? ContactPhone,
    bool IsActive
);