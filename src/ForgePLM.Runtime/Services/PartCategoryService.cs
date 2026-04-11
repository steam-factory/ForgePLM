using ForgePLM.Contracts.PartCategories;
using Microsoft.Data.SqlClient;

namespace ForgePLM.Runtime.Services
{
    public class PartCategoryService : IPartCategoryService
    {
        private readonly string _connectionString;

        public PartCategoryService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("ForgePlmDb")
                ?? throw new InvalidOperationException("Missing connection string: ForgePlmDb");
        }

        public async Task<List<PartCategoryDto>> GetPartCategoriesAsync()
        {
            var results = new List<PartCategoryDto>();

            const string sql = @"
            SELECT
                category_code,
                category_name,
                is_active
            FROM part_categories
            WHERE is_active = 1
            ORDER BY category_code;";

            await using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            await using var cmd = new SqlCommand(sql, conn);
            await using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                results.Add(new PartCategoryDto(
                    CategoryCode: reader["category_code"]?.ToString() ?? string.Empty,
                    CategoryName: reader["category_name"]?.ToString() ?? string.Empty,
                    Guideline: null, // not in DB yet
                    IsActive: reader.GetBoolean(reader.GetOrdinal("is_active"))
                ));
            }

            return results;
        }
    }
}