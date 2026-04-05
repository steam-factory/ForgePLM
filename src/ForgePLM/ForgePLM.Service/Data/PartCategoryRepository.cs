using System.Data;
using Microsoft.Data.SqlClient;
using ForgePLM.Contracts.PartCategories;

namespace ForgePLM.Service.Data;

public sealed class PartCategoryRepository
{
    private readonly string _connectionString;

    public PartCategoryRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("ForgePlmDb")
            ?? throw new InvalidOperationException("Missing connection string: ForgePlmDb");
    }

    public async Task<IReadOnlyList<PartCategoryDto>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        var results = new List<PartCategoryDto>();

        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken);

        const string sql = """
            SELECT
                category_code,
                category_name,
                guideline,
                is_active
            FROM dbo.part_categories
            WHERE is_active = 1
            ORDER BY category_code;
            """;

        await using var cmd = new SqlCommand(sql, conn)
        {
            CommandType = CommandType.Text
        };

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            results.Add(new PartCategoryDto(
                CategoryCode: reader.GetString(reader.GetOrdinal("category_code")),
                CategoryName: reader.GetString(reader.GetOrdinal("category_name")),
                Guideline: reader.IsDBNull(reader.GetOrdinal("guideline"))
                    ? null
                    : reader.GetString(reader.GetOrdinal("guideline")),
                IsActive: reader.GetBoolean(reader.GetOrdinal("is_active"))
            ));
        }

        return results;
    }
}