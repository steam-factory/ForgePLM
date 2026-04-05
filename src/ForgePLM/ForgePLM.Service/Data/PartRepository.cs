using System.Data;
using Microsoft.Data.SqlClient;
using ForgePLM.Contracts.Parts;

namespace ForgePLM.Service.Data;

public sealed class PartRepository
{
    private readonly string _connectionString;

    public PartRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("ForgePlmDb")
            ?? throw new InvalidOperationException("Missing connection string: ForgePlmDb");
    }

    public async Task<CreatePartResponse> CreatePartAsync(
        CreatePartRequest request,
        CancellationToken cancellationToken = default)
    {
        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken);

        await using var cmd = new SqlCommand("dbo.usp_CreatePartNumber", conn)
        {
            CommandType = CommandType.StoredProcedure
        };

        cmd.Parameters.Add(new SqlParameter("@project_code", SqlDbType.NVarChar, 50)
        {
            Value = request.ProjectCode
        });

        cmd.Parameters.Add(new SqlParameter("@category_code", SqlDbType.Char, 2)
        {
            Value = request.CategoryCode
        });

        cmd.Parameters.Add(new SqlParameter("@description", SqlDbType.NVarChar, 255)
        {
            Value = (object?)request.Description ?? DBNull.Value
        });

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

        if (!await reader.ReadAsync(cancellationToken))
            throw new InvalidOperationException("usp_CreatePartNumber returned no rows.");

        return new CreatePartResponse(
            PartId: reader.GetInt32(reader.GetOrdinal("part_id")),
            PartNumberInt: reader.GetInt32(reader.GetOrdinal("part_number_int")),
            PartNumber: reader.GetString(reader.GetOrdinal("part_number")),
            ProjectId: reader.GetInt32(reader.GetOrdinal("project_id")),
            CategoryCode: reader.GetString(reader.GetOrdinal("category_code")),
            CategoryName: reader.GetString(reader.GetOrdinal("category_name")),
            DescriptionCurrent: reader.IsDBNull(reader.GetOrdinal("description_current"))
                ? null
                : reader.GetString(reader.GetOrdinal("description_current")),
            CurrentRevisionId: reader.IsDBNull(reader.GetOrdinal("current_revision_id"))
                ? null
                : reader.GetInt32(reader.GetOrdinal("current_revision_id")),
            CreatedAt: reader.GetDateTime(reader.GetOrdinal("created_at")),
            RetiredAt: reader.IsDBNull(reader.GetOrdinal("retired_at"))
                ? null
                : reader.GetDateTime(reader.GetOrdinal("retired_at"))
        );
    }
}