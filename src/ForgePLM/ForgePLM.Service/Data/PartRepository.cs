using System.Data;
using ForgePLM.Contracts.Parts;
using ForgePLM.Contracts.Revisions;
using Microsoft.Data.SqlClient;

namespace ForgePLM.Service.Data;

public partial class PartRepository
{
    private readonly IConfiguration _configuration;


    public PartRepository(IConfiguration configuration)
    {
        _configuration = configuration;
    }


    private SqlConnection CreateConnection()
    {
        var connectionString = _configuration.GetConnectionString("ForgePLMDb")
            ?? throw new InvalidOperationException("Connection string 'ForgePLMDb' was not found.");

        return new SqlConnection(connectionString);
    }

    public async Task<List<PartRevisionItemDto>> GetEcoContentsAsync(int ecoId, CancellationToken ct)
    {
        const string sql = """
        SELECT
            p.part_id,
            p.category_code,
            p.part_number_int,
            p.part_number,
            r.revision_id,
            r.revision_code,
            r.revision_family,
            r.revision_seq,
            r.revision_state,
            r.part_description
        FROM dbo.revisions r
        INNER JOIN dbo.part_numbers p
            ON p.part_id = r.part_id
        WHERE r.eco_id = @eco_id
        ORDER BY p.category_code, p.part_number_int, r.revision_code;
        """;

        var results = new List<PartRevisionItemDto>();

        await using var connection = CreateConnection();
        await connection.OpenAsync(ct);

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@eco_id", ecoId);

        await using var reader = await command.ExecuteReaderAsync(ct);

        while (await reader.ReadAsync(ct))
        {
            var categoryCode = reader.GetString(1);
            var partNumberInt = reader.GetInt32(2);
            var revisionCode = reader.GetInt32(5);

            results.Add(new PartRevisionItemDto(
                PartId: reader.GetInt32(0),
                CategoryCode: categoryCode,
                PartNumberInt: partNumberInt,
                PartNumber: reader.GetString(3),
                RevisionId: reader.GetInt32(4),
                RevisionCode: revisionCode,
                RevisionFamily: reader.GetInt32(6),
                RevisionSeq: reader.GetInt32(7),
                RevisionState: reader.GetString(8),
                CompositeCode: $"{categoryCode}-{partNumberInt:0000000}-{revisionCode}",
                Description: reader.IsDBNull(9) ? string.Empty : reader.GetString(9)
            ));
        }

        return results;
    }
    public async Task<List<ProjectPartCurrentDto>> GetProjectPartsCurrentAsync(int projectId, CancellationToken ct)
    {
        const string sql = """
        SELECT
            p.part_id,
            p.category_code,
            p.part_number_int,
            p.part_number,
            p.document_type,
            r.revision_id,
            r.revision_code,
            r.revision_family,
            r.revision_state,
            e.eco_id,
            e.eco_number,
            e.eco_state,
            COALESCE(r.part_description, p.description_current, '') AS description
        FROM dbo.part_numbers p
        INNER JOIN dbo.revisions r
            ON r.revision_id = p.current_revision_id
        INNER JOIN dbo.eco e
            ON e.eco_id = r.eco_id
        WHERE p.project_id = @project_id
        ORDER BY p.category_code, p.part_number_int;
        """;

        var results = new List<ProjectPartCurrentDto>();

        await using var connection = CreateConnection();
        await connection.OpenAsync(ct);

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@project_id", projectId);

        await using var reader = await command.ExecuteReaderAsync(ct);

        while (await reader.ReadAsync(ct))
        {
            var categoryCode = reader.GetString(1);
            var partNumberInt = reader.GetInt32(2);
            var revisionCode = reader.GetInt32(5);

            results.Add(new ProjectPartCurrentDto(
                PartId: reader.GetInt32(0),
                CategoryCode: categoryCode,
                PartNumberInt: partNumberInt,
                PartNumber: reader.GetString(3),
                CurrentRevisionId: reader.GetInt32(5),
                RevisionId: reader.GetInt32(5),
                RevisionCode: reader.GetString(6),
                RevisionFamily: reader.GetInt32(7),
                RevisionState: reader.GetString(8),
                EcoId: reader.GetInt32(9),
                EcoNumber: reader.GetString(10),
                EcoState: reader.GetString(11),
                Description: reader.GetString(12),
                CompositeCode: $"{categoryCode}-{partNumberInt:0000000}-{revisionCode}",
                DocumentType: reader["document_type"]?.ToString() ?? "PART",
                CanSelect: false,
                AvailabilityReason: "No active ECO selected."
            ));
        }

        return results;
    }

    public async Task<PartRevisionItemDto> CreatePartAndInitialRevisionAsync(
        CreatePartRequest request,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.ProjectCode))
            throw new InvalidOperationException("ProjectCode is required.");

        if (string.IsNullOrWhiteSpace(request.EcoNumber))
            throw new InvalidOperationException("EcoNumber is required.");

        if (string.IsNullOrWhiteSpace(request.CategoryCode) || request.CategoryCode.Trim().Length != 2)
            throw new InvalidOperationException("CategoryCode must be 2 characters.");

        if (string.IsNullOrWhiteSpace(request.Description))
            throw new InvalidOperationException("Description is required.");

        await using var connection = CreateConnection();
        await connection.OpenAsync(ct);

        await using var transaction = await connection.BeginTransactionAsync(ct);

        try
        {
            int partId;
            int partNumberInt;
            string partNumber;
            string categoryCode;
            string descriptionCurrent;

            await using (var createPartCommand = new SqlCommand("dbo.usp_CreatePartNumber", connection, (SqlTransaction)transaction))
            {
                createPartCommand.CommandType = CommandType.StoredProcedure;
                createPartCommand.Parameters.AddWithValue("@project_code", request.ProjectCode.Trim());
                createPartCommand.Parameters.AddWithValue("@category_code", request.CategoryCode.Trim().ToUpperInvariant());
                createPartCommand.Parameters.AddWithValue("@description", request.Description.Trim());

                await using var reader = await createPartCommand.ExecuteReaderAsync(ct);

                if (!await reader.ReadAsync(ct))
                    throw new InvalidOperationException("Part create did not return a row.");

                // part_id, part_number_int, part_number, project_id, category_code, category_name,
                // description_current, current_revision_id, created_at, retired_at
                partId = reader.GetInt32(0);
                partNumberInt = reader.GetInt32(1);
                partNumber = reader.GetString(2);
                categoryCode = reader.GetString(4);
                descriptionCurrent = reader.IsDBNull(6) ? string.Empty : reader.GetString(6);
            }

            int revisionId;
            int revisionCode;
            int revisionFamily;
            int revisionSeq;
            string revisionState;
            string partDescription;

            await using (var createRevisionCommand = new SqlCommand("dbo.usp_CreateRevision", connection, (SqlTransaction)transaction))
            {
                createRevisionCommand.CommandType = CommandType.StoredProcedure;
                createRevisionCommand.Parameters.AddWithValue("@part_number", partNumber);
                createRevisionCommand.Parameters.AddWithValue("@eco_number", request.EcoNumber.Trim());
                createRevisionCommand.Parameters.AddWithValue("@revision_description", DBNull.Value);
                createRevisionCommand.Parameters.AddWithValue("@part_description", request.Description.Trim());

                await using var reader = await createRevisionCommand.ExecuteReaderAsync(ct);

                if (!await reader.ReadAsync(ct))
                    throw new InvalidOperationException("Revision create did not return a row.");

                // revision_id, part_number, revision_code, revision_family, revision_seq,
                // revision_description, part_description, revision_state, is_current, eco_number, created_at
                revisionId = reader.GetInt32(0);
                revisionCode = reader.GetInt32(2);
                revisionFamily = reader.GetInt32(3);
                revisionSeq = reader.GetInt32(4);
                partDescription = reader.IsDBNull(6) ? descriptionCurrent : reader.GetString(6);
                revisionState = reader.GetString(7);
            }

            const string syncCurrentRevisionSql = """
                UPDATE dbo.part_numbers
                SET
                    current_revision_id = @current_revision_id,
                    description_current = @description_current
                WHERE part_id = @part_id;
                """;

            await using (var syncCommand = new SqlCommand(syncCurrentRevisionSql, connection, (SqlTransaction)transaction))
            {
                syncCommand.Parameters.AddWithValue("@current_revision_id", revisionId);
                syncCommand.Parameters.AddWithValue("@description_current", partDescription);
                syncCommand.Parameters.AddWithValue("@part_id", partId);

                var rows = await syncCommand.ExecuteNonQueryAsync(ct);

                if (rows != 1)
                    throw new InvalidOperationException("Failed to sync current revision onto part record.");
            }



            await transaction.CommitAsync(ct);

            var compositeCode = $"{categoryCode}-{partNumberInt:0000000}-{revisionCode}";

            return new PartRevisionItemDto(
                PartId: partId,
                CategoryCode: categoryCode,
                PartNumberInt: partNumberInt,
                PartNumber: partNumber,
                RevisionId: revisionId,
                RevisionCode: revisionCode,
                RevisionFamily: revisionFamily,
                RevisionSeq: revisionSeq,
                RevisionState: revisionState,
                CompositeCode: compositeCode,
                Description: partDescription
            );


        }
        catch
        {
            await transaction.RollbackAsync(ct);
            throw;
        }
    }

    

}