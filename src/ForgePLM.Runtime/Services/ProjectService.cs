using ForgePLM.Contracts.Projects;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace ForgePLM.Runtime.Services
{
    public class ProjectService : IProjectService
    {
        private readonly string _connectionString;

        public ProjectService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("ForgePlmDb")
                ?? throw new InvalidOperationException("Missing connection string: ForgePlmDb");
        }

        public async Task<List<ProjectDto>> GetProjectsByCustomerAsync(int customerId)
        {
            var results = new List<ProjectDto>();

            const string sql = @"
            SELECT
                project_id,
                customer_id,
                project_seq,
                project_code,
                project_name,
                is_active
            FROM projects
            WHERE customer_id = @customerId
              AND is_active = 1
            ORDER BY project_seq;";

            await using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@customerId", customerId);

            await using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                results.Add(new ProjectDto(
                    ProjectId: reader.GetInt32(reader.GetOrdinal("project_id")),
                    CustomerId: reader.GetInt32(reader.GetOrdinal("customer_id")),
                    ProjectSeq: reader.GetInt32(reader.GetOrdinal("project_seq")),
                    ProjectCode: reader["project_code"] as string ?? string.Empty,
                    ProjectName: reader["project_name"] as string ?? string.Empty,
                    IsActive: reader.GetBoolean(reader.GetOrdinal("is_active"))
                ));
            }

            return results;
        }
    }
}