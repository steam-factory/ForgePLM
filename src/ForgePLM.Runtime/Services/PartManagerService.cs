using ForgePLM.Contracts.Parts;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ForgePLM.Runtime.Services
{
    public class PartManagerService : IPartManagerService
    {
        private readonly string _connectionString;

        public PartManagerService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("ForgePlmDb")
                ?? throw new InvalidOperationException("Missing connection string: ForgePlmDb");
        }

        public async Task<List<PartNumberManagerItemDto>> GetPartNumberManagerItemsAsync(CancellationToken cancellationToken = default)
        {
            var results = new List<PartNumberManagerItemDto>();

            const string sql = @"
SELECT
    p.part_id,
    r.revision_id,
    CONCAT(p.category_code, '-', RIGHT('0000000' + CAST(p.part_number_int AS varchar(7)), 7)) AS part_number,
    r.revision_code,
    CONCAT(p.category_code, '-', RIGHT('0000000' + CAST(p.part_number_int AS varchar(7)), 7), '-', r.revision_code) AS composite_code,
    COALESCE(r.part_description, '') AS description,
    COALESCE(p.document_type, 'PART') AS document_type,
    COALESCE(r.revision_state, '') AS revision_state,
    e.release_level AS revision_family,
    e.eco_id,
    COALESCE(e.eco_number, '') AS eco_number,
    COALESCE(e.eco_state, '') AS eco_state,
    pr.project_id,
    COALESCE(pr.project_code, '') AS project_code,
    COALESCE(pr.project_name, '') AS project_name,
    c.customer_id,
    COALESCE(c.customer_code, '') AS customer_code,
    COALESCE(c.customer_name, '') AS customer_name,
    CASE
        WHEN UPPER(COALESCE(r.revision_state, '')) IN ('DEVELOPMENT', 'STAGED') THEN CAST(1 AS bit)
        ELSE CAST(0 AS bit)
    END AS can_edit_description
FROM revisions r
INNER JOIN part_numbers p
    ON p.part_id = r.part_id
INNER JOIN eco e
    ON e.eco_id = r.eco_id
INNER JOIN projects pr
    ON pr.project_id = e.project_id
INNER JOIN customers c
    ON c.customer_id = pr.customer_id
ORDER BY c.customer_name, pr.project_name, p.category_code, p.part_number_int, r.revision_id DESC;";

            await using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(cancellationToken);

            await using var cmd = new SqlCommand(sql, conn);
            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

            while (await reader.ReadAsync(cancellationToken))
            {
                results.Add(new PartNumberManagerItemDto(
                    PartId: reader.GetInt32(reader.GetOrdinal("part_id")),
                    RevisionId: reader.GetInt32(reader.GetOrdinal("revision_id")),
                    PartNumber: reader["part_number"]?.ToString() ?? string.Empty,
                    RevisionCode: reader["revision_code"]?.ToString() ?? string.Empty,
                    CompositeCode: reader["composite_code"]?.ToString() ?? string.Empty,
                    Description: reader["description"]?.ToString() ?? string.Empty,
                    DocumentType: reader["document_type"]?.ToString() ?? "PART",
                    RevisionState: reader["revision_state"]?.ToString() ?? string.Empty,
                    RevisionFamily: Convert.ToInt32(reader["revision_family"]),
                    EcoId: reader.GetInt32(reader.GetOrdinal("eco_id")),
                    EcoNumber: reader["eco_number"]?.ToString() ?? string.Empty,
                    EcoState: reader["eco_state"]?.ToString() ?? string.Empty,
                    ProjectId: reader.GetInt32(reader.GetOrdinal("project_id")),
                    ProjectCode: reader["project_code"]?.ToString() ?? string.Empty,
                    ProjectName: reader["project_name"]?.ToString() ?? string.Empty,
                    CustomerId: reader.GetInt32(reader.GetOrdinal("customer_id")),
                    CustomerCode: reader["customer_code"]?.ToString() ?? string.Empty,
                    CustomerName: reader["customer_name"]?.ToString() ?? string.Empty,
                    CanEditDescription: reader.GetBoolean(reader.GetOrdinal("can_edit_description"))
                ));
            }

            return results;
        }

        public async Task<PartNumberManagerItemDto> UpdateRevisionDescriptionAsync(
            int revisionId,
            string description,
            CancellationToken cancellationToken = default)
        {
            const string sql = @"
UPDATE r
SET r.part_description = @description
FROM revisions r
WHERE r.revision_id = @revisionId
  AND UPPER(COALESCE(r.revision_state, '')) IN ('DEVELOPMENT', 'STAGED');

SELECT
    p.part_id,
    r.revision_id,
    CONCAT(p.category_code, '-', RIGHT('0000000' + CAST(p.part_number_int AS varchar(7)), 7)) AS part_number,
    r.revision_code,
    CONCAT(p.category_code, '-', RIGHT('0000000' + CAST(p.part_number_int AS varchar(7)), 7), '-', r.revision_code) AS composite_code,
    COALESCE(r.part_description, '') AS description,
    COALESCE(p.document_type, 'PART') AS document_type,
    COALESCE(r.revision_state, '') AS revision_state,
    e.release_level AS revision_family,
    e.eco_id,
    COALESCE(e.eco_number, '') AS eco_number,
    COALESCE(e.eco_state, '') AS eco_state,
    pr.project_id,
    COALESCE(pr.project_code, '') AS project_code,
    COALESCE(pr.project_name, '') AS project_name,
    c.customer_id,
    COALESCE(c.customer_code, '') AS customer_code,
    COALESCE(c.customer_name, '') AS customer_name,
    CASE
        WHEN UPPER(COALESCE(r.revision_state, '')) IN ('DEVELOPMENT', 'STAGED') THEN CAST(1 AS bit)
        ELSE CAST(0 AS bit)
    END AS can_edit_description
FROM revisions r
INNER JOIN part_numbers p
    ON p.part_id = r.part_id
INNER JOIN eco e
    ON e.eco_id = r.eco_id
INNER JOIN projects pr
    ON pr.project_id = e.project_id
INNER JOIN customers c
    ON c.customer_id = pr.customer_id
WHERE r.revision_id = @revisionId;";

            await using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(cancellationToken);

            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@revisionId", revisionId);
            cmd.Parameters.AddWithValue("@description", description ?? string.Empty);

            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

            if (!await reader.ReadAsync(cancellationToken))
                throw new InvalidOperationException($"Revision {revisionId} was not found.");

            return new PartNumberManagerItemDto(
                PartId: reader.GetInt32(reader.GetOrdinal("part_id")),
                RevisionId: reader.GetInt32(reader.GetOrdinal("revision_id")),
                PartNumber: reader["part_number"]?.ToString() ?? string.Empty,
                RevisionCode: reader["revision_code"]?.ToString() ?? string.Empty,
                CompositeCode: reader["composite_code"]?.ToString() ?? string.Empty,
                Description: reader["description"]?.ToString() ?? string.Empty,
                DocumentType: reader["document_type"]?.ToString() ?? "PART",
                RevisionState: reader["revision_state"]?.ToString() ?? string.Empty,
                RevisionFamily: Convert.ToInt32(reader["revision_family"]),
                EcoId: reader.GetInt32(reader.GetOrdinal("eco_id")),
                EcoNumber: reader["eco_number"]?.ToString() ?? string.Empty,
                EcoState: reader["eco_state"]?.ToString() ?? string.Empty,
                ProjectId: reader.GetInt32(reader.GetOrdinal("project_id")),
                ProjectCode: reader["project_code"]?.ToString() ?? string.Empty,
                ProjectName: reader["project_name"]?.ToString() ?? string.Empty,
                CustomerId: reader.GetInt32(reader.GetOrdinal("customer_id")),
                CustomerCode: reader["customer_code"]?.ToString() ?? string.Empty,
                CustomerName: reader["customer_name"]?.ToString() ?? string.Empty,
                CanEditDescription: reader.GetBoolean(reader.GetOrdinal("can_edit_description"))
            );
        }
    }
}