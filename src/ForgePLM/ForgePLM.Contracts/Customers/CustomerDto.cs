namespace ForgePLM.Contracts.Customers;

public record CustomerDto(
    int CustomerId,
    string CustomerCode,
    string CustomerName,
    string? ContactName,
    string? ContactEmail,
    string? ContactPhone,
    bool IsActive,
    DateTime CreatedAt
);