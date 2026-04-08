using ForgePLM.Contracts.Dtos;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace ForgePLM.Runtime.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly string _connectionString;

        public CustomerService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("ForgePlmDb")
                ?? throw new InvalidOperationException("Missing connection string: ForgePlmDb");
        }

        public async Task<List<CustomerDto>> GetCustomersAsync()
        {
            var results = new List<CustomerDto>();

            const string sql = @"
SELECT
    customer_id,
    customer_code,
    customer_name
FROM customers
WHERE is_active = 1
ORDER BY customer_name;";

            await using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            await using var cmd = new SqlCommand(sql, conn);
            await using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                results.Add(new CustomerDto
                {
                    CustomerId = reader.GetInt32(reader.GetOrdinal("customer_id")),
                    CustomerCode = reader["customer_code"] as string ?? string.Empty,
                    CustomerName = reader["customer_name"] as string ?? string.Empty
                });
            }

            return results;
        }
    }
}