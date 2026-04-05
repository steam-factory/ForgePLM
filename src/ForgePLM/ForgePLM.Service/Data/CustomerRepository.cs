using System.Data;
using Microsoft.Data.SqlClient;
using ForgePLM.Contracts.Customers;

namespace ForgePLM.Service.Data;

public sealed class CustomerRepository
{
    private readonly string _connectionString;

    public CustomerRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("ForgePLMDb")
            ?? throw new InvalidOperationException("Missing connection string: ForgePLMDb");
    }

    public async Task<IReadOnlyList<CustomerDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var results = new List<CustomerDto>();

        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken);

        const string sql = """
            SELECT
                customer_id,
                customer_code,
                customer_name,
                contact_name,
                contact_email,
                contact_phone,
                is_active,
                created_at
            FROM dbo.customers
            WHERE is_active = 1
            ORDER BY customer_name;
            """;

        await using var cmd = new SqlCommand(sql, conn)
        {
            CommandType = CommandType.Text
        };

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            results.Add(new CustomerDto(
                CustomerId: reader.GetInt32(reader.GetOrdinal("customer_id")),
                CustomerCode: reader.GetString(reader.GetOrdinal("customer_code")),
                CustomerName: reader.GetString(reader.GetOrdinal("customer_name")),
                ContactName: reader.IsDBNull(reader.GetOrdinal("contact_name"))
                    ? null
                    : reader.GetString(reader.GetOrdinal("contact_name")),
                ContactEmail: reader.IsDBNull(reader.GetOrdinal("contact_email"))
                    ? null
                    : reader.GetString(reader.GetOrdinal("contact_email")),
                ContactPhone: reader.IsDBNull(reader.GetOrdinal("contact_phone"))
                    ? null
                    : reader.GetString(reader.GetOrdinal("contact_phone")),
                IsActive: reader.GetBoolean(reader.GetOrdinal("is_active")),
                CreatedAt: reader.GetDateTime(reader.GetOrdinal("created_at"))
            ));
        }

        return results;
    }

    public async Task<CustomerDto> UpdateAsync(int customerId, CustomerDto request, CancellationToken cancellationToken = default)
    {
        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken);

        const string sql = """
        UPDATE dbo.customers
        SET
            customer_code = @customer_code,
            customer_name = @customer_name,
            contact_name = @contact_name,
            contact_email = @contact_email,
            contact_phone = @contact_phone,
            is_active = @is_active
        OUTPUT
            inserted.customer_id,
            inserted.customer_code,
            inserted.customer_name,
            inserted.contact_name,
            inserted.contact_email,
            inserted.contact_phone,
            inserted.is_active,
            inserted.created_at
        WHERE customer_id = @customer_id;
        """;

        await using var cmd = new SqlCommand(sql, conn);

        cmd.Parameters.Add(new SqlParameter("@customer_id", SqlDbType.Int)
        {
            Value = customerId
        });

        cmd.Parameters.Add(new SqlParameter("@customer_code", SqlDbType.NVarChar, 50)
        {
            Value = request.CustomerCode
        });

        cmd.Parameters.Add(new SqlParameter("@customer_name", SqlDbType.NVarChar, 255)
        {
            Value = request.CustomerName
        });

        cmd.Parameters.Add(new SqlParameter("@contact_name", SqlDbType.NVarChar, 255)
        {
            Value = (object?)request.ContactName ?? DBNull.Value
        });

        cmd.Parameters.Add(new SqlParameter("@contact_email", SqlDbType.NVarChar, 255)
        {
            Value = (object?)request.ContactEmail ?? DBNull.Value
        });

        cmd.Parameters.Add(new SqlParameter("@contact_phone", SqlDbType.NVarChar, 50)
        {
            Value = (object?)request.ContactPhone ?? DBNull.Value
        });

        cmd.Parameters.Add(new SqlParameter("@is_active", SqlDbType.Bit)
        {
            Value = request.IsActive
        });

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

        if (!await reader.ReadAsync(cancellationToken))
            throw new InvalidOperationException("Customer update returned no row.");

        return new CustomerDto(
            CustomerId: reader.GetInt32(reader.GetOrdinal("customer_id")),
            CustomerCode: reader.GetString(reader.GetOrdinal("customer_code")),
            CustomerName: reader.GetString(reader.GetOrdinal("customer_name")),
            ContactName: reader.IsDBNull(reader.GetOrdinal("contact_name")) ? null : reader.GetString(reader.GetOrdinal("contact_name")),
            ContactEmail: reader.IsDBNull(reader.GetOrdinal("contact_email")) ? null : reader.GetString(reader.GetOrdinal("contact_email")),
            ContactPhone: reader.IsDBNull(reader.GetOrdinal("contact_phone")) ? null : reader.GetString(reader.GetOrdinal("contact_phone")),
            IsActive: reader.GetBoolean(reader.GetOrdinal("is_active")),
            CreatedAt: reader.GetDateTime(reader.GetOrdinal("created_at"))
        );
    }

    public async Task<CustomerDto> CreateAsync(CustomerDto request, CancellationToken cancellationToken = default)
    {
        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken);

        const string sql = """
            INSERT INTO dbo.customers
            (
                customer_code,
                customer_name,
                contact_name,
                contact_email,
                contact_phone,
                is_active,
                created_at
            )
            OUTPUT
                inserted.customer_id,
                inserted.customer_code,
                inserted.customer_name,
                inserted.contact_name,
                inserted.contact_email,
                inserted.contact_phone,
                inserted.is_active,
                inserted.created_at
            VALUES
            (
                @customer_code,
                @customer_name,
                @contact_name,
                @contact_email,
                @contact_phone,
                @is_active,
                SYSUTCDATETIME()
            );
            """;

        await using var cmd = new SqlCommand(sql, conn)
        {
            CommandType = CommandType.Text
        };

        cmd.Parameters.Add(new SqlParameter("@customer_code", SqlDbType.NVarChar, 50)
        {
            Value = request.CustomerCode
        });

        cmd.Parameters.Add(new SqlParameter("@customer_name", SqlDbType.NVarChar, 255)
        {
            Value = request.CustomerName
        });

        cmd.Parameters.Add(new SqlParameter("@contact_name", SqlDbType.NVarChar, 255)
        {
            Value = (object?)request.ContactName ?? DBNull.Value
        });

        cmd.Parameters.Add(new SqlParameter("@contact_email", SqlDbType.NVarChar, 255)
        {
            Value = (object?)request.ContactEmail ?? DBNull.Value
        });

        cmd.Parameters.Add(new SqlParameter("@contact_phone", SqlDbType.NVarChar, 50)
        {
            Value = (object?)request.ContactPhone ?? DBNull.Value
        });

        cmd.Parameters.Add(new SqlParameter("@is_active", SqlDbType.Bit)
        {
            Value = request.IsActive
        });



        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

        if (!await reader.ReadAsync(cancellationToken))
            throw new InvalidOperationException("Customer insert returned no row.");

        return new CustomerDto(
            CustomerId: reader.GetInt32(reader.GetOrdinal("customer_id")),
            CustomerCode: reader.GetString(reader.GetOrdinal("customer_code")),
            CustomerName: reader.GetString(reader.GetOrdinal("customer_name")),
            ContactName: reader.IsDBNull(reader.GetOrdinal("contact_name"))
                ? null
                : reader.GetString(reader.GetOrdinal("contact_name")),
            ContactEmail: reader.IsDBNull(reader.GetOrdinal("contact_email"))
                ? null
                : reader.GetString(reader.GetOrdinal("contact_email")),
            ContactPhone: reader.IsDBNull(reader.GetOrdinal("contact_phone"))
                ? null
                : reader.GetString(reader.GetOrdinal("contact_phone")),
            IsActive: reader.GetBoolean(reader.GetOrdinal("is_active")),
            CreatedAt: reader.GetDateTime(reader.GetOrdinal("created_at"))
        );
    }
}