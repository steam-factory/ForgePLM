using ForgePLM.Contracts.Requests;
using ForgePLM.Contracts.Responses;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace ForgePLM.Runtime.Services
{
    public class RevisionService : IRevisionService
    {
        private readonly string _connectionString;

        public RevisionService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("ForgePlmDb")
                ?? throw new InvalidOperationException("Missing connection string: ForgePlmDb");
        }

        public async Task<AssignRevisionResponse> AssignRevisionAsync(AssignRevisionRequest request)
        {
            const string sql = @"
SELECT TOP 1
    r.revision_id,
    r.part_id,
    r.revision_code,
    r.part_description,
    p.category_code,
    p.part_number_int,
    e.eco_number
FROM revisions r
INNER JOIN part_numbers p
    ON p.part_id = r.part_id
INNER JOIN eco e
    ON e.eco_id = r.eco_id
WHERE r.revision_id = @revisionId;";

            await using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@revisionId", request.RevisionId);

            await using var reader = await cmd.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
                throw new InvalidOperationException($"Revision {request.RevisionId} was not found.");

            int partNumberInt = Convert.ToInt32(reader["part_number_int"]);
            string categoryCode = reader["category_code"] as string ?? string.Empty;

            return new AssignRevisionResponse
            {
                Guid = Guid.NewGuid().ToString().ToUpperInvariant(),
                PartId = reader.GetInt32(reader.GetOrdinal("part_id")),
                RevisionId = reader.GetInt32(reader.GetOrdinal("revision_id")),
                PartNumber = $"{categoryCode}-{partNumberInt:D7}",
                RevisionCode = reader["revision_code"]?.ToString() ?? string.Empty,
                Description = reader["part_description"] as string ?? string.Empty,
                EcoNumber = reader["eco_number"] as string ?? string.Empty
            };
        }

        public async Task<OpenRevisionResponse> GetOpenInfoAsync(int revisionId)
        {
            const string sql = @"
SELECT TOP 1
    r.revision_id,
    r.revision_code,
    p.category_code,
    p.part_number_int
FROM revisions r
INNER JOIN part_numbers p
    ON p.part_id = r.part_id
WHERE r.revision_id = @revisionId;";

            await using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@revisionId", revisionId);

            await using var reader = await cmd.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
                throw new InvalidOperationException($"Revision {revisionId} was not found.");

            int partNumberInt = Convert.ToInt32(reader["part_number_int"]);
            string categoryCode = reader["category_code"] as string ?? string.Empty;
            string partNumber = $"{categoryCode}-{partNumberInt:D7}";

            return new OpenRevisionResponse
            {
                RevisionId = reader.GetInt32(reader.GetOrdinal("revision_id")),
                PartNumber = partNumber,
                RevisionCode = reader["revision_code"]?.ToString() ?? string.Empty,
                FilePath = null // TODO: return resolved development path once file-location rules are finalized
            };
        }
    }
}