using ForgePLM.Contracts.Eco;
using ForgePLM.Contracts.Revisions;
using ForgePLM.Runtime.Common;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.IO;

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
                eco_number_int,
                eco_number,
                eco_title,
                eco_description,
                release_level,
                eco_state,
                created_at
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
                results.Add(new EcoDto(
                    EcoId: reader.GetInt32(reader.GetOrdinal("eco_id")),
                    ProjectId: reader.GetInt32(reader.GetOrdinal("project_id")),
                    EcoNumberInt: reader.GetInt32(reader.GetOrdinal("eco_number_int")),
                    EcoNumber: reader["eco_number"] as string ?? string.Empty,
                    EcoTitle: reader["eco_title"] as string ?? string.Empty,
                    EcoDescription: reader["eco_description"] as string,
                    ReleaseLevel: Convert.ToInt32(reader["release_level"]),
                    EcoState: reader["eco_state"] as string ?? string.Empty,
                    CreatedAt: reader.GetDateTime(reader.GetOrdinal("created_at"))
                ));
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
                p.document_type,
                r.revision_code,
                r.part_description,
                e.eco_number,
                pr.project_code,
                pr.project_name
            FROM revisions r
            INNER JOIN part_numbers p
                ON p.part_id = r.part_id
            INNER JOIN eco e
                ON e.eco_id = r.eco_id
            INNER JOIN projects pr
                ON pr.project_id = e.project_id
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
                string documentType = reader["document_type"]?.ToString() ?? "PART";
                string projectCode = reader["project_code"] as string ?? string.Empty;
                string projectName = reader["project_name"] as string ?? string.Empty;

                string partNumber = $"{categoryCode}-{partNumberInt:D7}";
                string projectDisplay = $"{projectCode} - {projectName}"; 
                string extension = DocumentTypeHelper.GetExtension(documentType);

                results.Add(new RevisionDto
                {
                    RevisionId = reader.GetInt32(reader.GetOrdinal("revision_id")),
                    PartId = reader.GetInt32(reader.GetOrdinal("part_id")),
                    EcoId = reader.GetInt32(reader.GetOrdinal("eco_id")),
                    CategoryCode = categoryCode,
                    PartNumberInt = partNumberInt,
                    PartNumber = partNumber,
                    RevisionCode = revisionCode,
                    Description = reader["part_description"] as string ?? string.Empty,
                    EcoNumber = reader["eco_number"] as string ?? string.Empty,
                    DocumentType = documentType,
                    FilePath = Path.Combine(
                        @"e:\SteamFactory_DEV\projects",
                        projectDisplay,
                        "development",
                        partNumber + extension)
                });
            }

            return results;
        }
    }
}