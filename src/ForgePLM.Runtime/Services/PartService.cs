using ForgePLM.Contracts.Parts;
using ForgePLM.Contracts.Revisions;
using Microsoft.Data.SqlClient;
using System.Data;

namespace ForgePLM.Runtime.Services
{
    public class PartService : IPartService
    {
        private readonly string _connectionString;

        public PartService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("ForgePlmDb")
                ?? throw new InvalidOperationException("Missing connection string: ForgePlmDb");
        }

        public async Task<PartRevisionItemDto> CreatePartAsync(
            CreatePartRequest request,
            CancellationToken cancellationToken = default)
        {
            await using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(cancellationToken);

            await using var cmd = new SqlCommand("usp_CreatePartUnderEco", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.AddWithValue("@project_code", request.ProjectCode);
            cmd.Parameters.AddWithValue("@eco_id", request.EcoId);
            cmd.Parameters.AddWithValue("@category_code", request.CategoryCode);
            cmd.Parameters.AddWithValue("@description", (object?)request.Description ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@document_type", request.DocumentType ?? "PART");

            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

            if (!await reader.ReadAsync(cancellationToken))
                throw new InvalidOperationException("CreatePart returned no result.");

            return new PartRevisionItemDto(
                PartId: reader.GetInt32(reader.GetOrdinal("part_id")),
                RevisionId: reader.GetInt32(reader.GetOrdinal("revision_id")),
                CategoryCode: reader["category_code"]?.ToString() ?? string.Empty,
                PartNumberInt: Convert.ToInt32(reader["part_number_int"]),
                RevisionCode: Convert.ToInt32(reader["revision_code"]),
                RevisionFamily: Convert.ToInt32(reader["revision_family"]),
                RevisionSeq: Convert.ToInt32(reader["revision_seq"]),
                RevisionState: reader["revision_state"]?.ToString() ?? string.Empty,
                CompositeCode: reader["composite_code"]?.ToString() ?? string.Empty,
                Description: reader["part_description"]?.ToString() ?? string.Empty,
                EcoNumber: reader["eco_number"]?.ToString() ?? string.Empty,
                DocumentType: reader["document_type"]?.ToString() ?? "PART"
            );
        }

        public async Task<List<ProjectPartCurrentDto>> GetProjectPartsCurrentAsync(
            int projectId,
            CancellationToken cancellationToken = default)
        {
            var results = new List<ProjectPartCurrentDto>();

            const string sql = @"
            WITH ranked_revisions AS
            (
                SELECT
                    p.part_id,
                    r.revision_id,
                    r.revision_id AS current_revision_id,
                    p.category_code,
                    p.part_number_int,
                    p.document_type,
                    r.revision_code,
                    r.part_description,
                    r.revision_state,
                    e.eco_id,
                    e.eco_number,
                    e.eco_state,
                    e.release_level,
                    ROW_NUMBER() OVER
                    (
                        PARTITION BY p.part_id
                        ORDER BY e.release_level DESC, r.revision_id DESC
                    ) AS rn
                FROM revisions r
                INNER JOIN part_numbers p
                    ON p.part_id = r.part_id
                INNER JOIN eco e
                    ON e.eco_id = r.eco_id
                WHERE e.project_id = @projectId
            )
            SELECT
                part_id,
                revision_id,
                current_revision_id,
                category_code,
                part_number_int,
                document_type,
                revision_code,
                part_description,
                revision_state,
                eco_id,
                eco_number,
                eco_state,
                release_level
            FROM ranked_revisions
            WHERE rn = 1
            ORDER BY category_code, part_number_int;";

            await using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(cancellationToken);

            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@projectId", projectId);

            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

            while (await reader.ReadAsync(cancellationToken))
            {
                int partNumberInt = Convert.ToInt32(reader["part_number_int"]);
                string categoryCode = reader["category_code"]?.ToString() ?? string.Empty;

                results.Add(new ProjectPartCurrentDto(
                    PartId: reader.GetInt32(reader.GetOrdinal("part_id")),
                    CategoryCode: categoryCode,
                    PartNumberInt: partNumberInt,
                    PartNumber: $"{categoryCode}-{partNumberInt:D7}",
                    CurrentRevisionId: reader.GetInt32(reader.GetOrdinal("current_revision_id")),
                    RevisionId: reader.GetInt32(reader.GetOrdinal("revision_id")),
                    RevisionCode: reader["revision_code"]?.ToString() ?? string.Empty,
                    RevisionFamily: Convert.ToInt32(reader["release_level"]),
                    RevisionState: reader["revision_state"]?.ToString() ?? string.Empty,
                    EcoId: reader.GetInt32(reader.GetOrdinal("eco_id")),
                    EcoNumber: reader["eco_number"]?.ToString() ?? string.Empty,
                    EcoState: reader["eco_state"]?.ToString() ?? string.Empty,
                    Description: reader["part_description"]?.ToString() ?? string.Empty,
                    CompositeCode: $"{categoryCode}-{partNumberInt:D7}-{reader["revision_code"]?.ToString() ?? string.Empty}",
                    DocumentType: reader["document_type"]?.ToString() ?? "PART",
                    CanSelect: false,
                    AvailabilityReason: null
                ));
            }

            return results;
        }
    }
}