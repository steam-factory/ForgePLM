using ForgePLM.Contracts.Dtos;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace ForgePLM.Runtime.Services
{
    public class EcoService : IEcoService
    {
        private readonly string _connectionString;

        public EcoService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("ForgePlmDb")
                ?? throw new InvalidOperationException("Missing connection string: ForgePlmDb");
        }

        public async Task<List<EcoDto>> GetEcosByProjectAsync(int projectId)
        {
            var results = new List<EcoDto>();

            const string sql = @"
SELECT
    eco_id,
    project_id,
    eco_number,
    eco_state,
    release_level
FROM eco
WHERE project_id = @projectId
ORDER BY eco_id DESC;";

            await using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@projectId", projectId);

            await using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                results.Add(new EcoDto
                {
                    EcoId = reader.GetInt32(reader.GetOrdinal("eco_id")),
                    ProjectId = reader.GetInt32(reader.GetOrdinal("project_id")),
                    EcoNumber = reader["eco_number"] as string ?? string.Empty,
                    EcoState = reader["eco_state"] as string ?? string.Empty,
                    ReleaseLevel = Convert.ToInt32(reader["release_level"])
                });
            }

            return results;
        }

        public async Task<List<RevisionDto>> GetEcoContentsAsync(int ecoId)
        {
            var results = new List<RevisionDto>();

            const string sql = @"
SELECT
    r.revision_id,
    r.part_id,
    r.eco_id,
    p.category_code,
    p.part_number_int,
    r.revision_code,
    r.part_description,
    e.eco_number
FROM revisions r
INNER JOIN part_numbers p
    ON p.part_id = r.part_id
INNER JOIN eco e
    ON e.eco_id = r.eco_id
WHERE r.eco_id = @ecoId
ORDER BY p.category_code, p.part_number_int;";

            await using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@ecoId", ecoId);

            await using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                int partNumberInt = Convert.ToInt32(reader["part_number_int"]);
                string categoryCode = reader["category_code"] as string ?? string.Empty;
                string revisionCode = reader["revision_code"]?.ToString() ?? string.Empty;

                results.Add(new RevisionDto
                {
                    RevisionId = reader.GetInt32(reader.GetOrdinal("revision_id")),
                    PartId = reader.GetInt32(reader.GetOrdinal("part_id")),
                    EcoId = reader.GetInt32(reader.GetOrdinal("eco_id")),
                    CategoryCode = categoryCode,
                    PartNumberInt = partNumberInt,
                    PartNumber = $"{categoryCode}-{partNumberInt:D7}",
                    RevisionCode = revisionCode,
                    Description = reader["part_description"] as string ?? string.Empty,
                    EcoNumber = reader["eco_number"] as string ?? string.Empty,
                    FilePath = null // TODO: wire real path resolution once file storage rules are finalized
                });
            }

            return results;
        }
    }
}