using ForgePLM.Contracts.Eco;
using Microsoft.Data.SqlClient;

namespace ForgePLM.Service.Data;

public class EcoRepository
{
    private readonly IConfiguration _configuration;

    public EcoRepository(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    private SqlConnection CreateConnection()
    {
        var connectionString = _configuration.GetConnectionString("ForgePLMDb")
            ?? throw new InvalidOperationException("Connection string 'ForgePLMDb' was not found.");

        return new SqlConnection(connectionString);
    }

    public async Task<List<EcoDto>> GetByProjectAsync(int projectId, CancellationToken ct)
    {
        const string sql = """
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
            FROM dbo.eco
            WHERE project_id = @project_id
            ORDER BY eco_number_int DESC;
            """;

        var results = new List<EcoDto>();

        await using var connection = CreateConnection();
        await connection.OpenAsync(ct);

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@project_id", projectId);

        await using var reader = await command.ExecuteReaderAsync(ct);

        while (await reader.ReadAsync(ct))
        {
            results.Add(new EcoDto(
                EcoId: reader.GetInt32(0),
                ProjectId: reader.GetInt32(1),
                EcoNumberInt: reader.GetInt32(2),
                EcoNumber: reader.GetString(3),
                EcoTitle: reader.GetString(4),
                EcoDescription: reader.IsDBNull(5) ? null : reader.GetString(5),
                ReleaseLevel: reader.GetInt32(6),
                EcoState: reader.GetString(7),
                CreatedAt: reader.GetDateTime(8)
            ));
        }

        return results;
    }

    public async Task<EcoDto> CreateAsync(CreateEcoRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.ProjectCode))
            throw new InvalidOperationException("ProjectCode is required.");

        if (string.IsNullOrWhiteSpace(request.EcoTitle))
            throw new InvalidOperationException("ECO Title is required.");

        await using var connection = CreateConnection();
        await connection.OpenAsync(ct);

        await using var command = new SqlCommand("dbo.usp_CreateEco", connection)
        {
            CommandType = System.Data.CommandType.StoredProcedure
        };

        command.Parameters.AddWithValue("@project_code", request.ProjectCode.Trim());
        command.Parameters.AddWithValue("@eco_title", request.EcoTitle.Trim());
        command.Parameters.AddWithValue("@eco_description", (object?)request.EcoDescription ?? DBNull.Value);
        command.Parameters.AddWithValue("@release_level", request.ReleaseLevel);

        await using var reader = await command.ExecuteReaderAsync(ct);

        if (!await reader.ReadAsync(ct))
            throw new InvalidOperationException("ECO create did not return a row.");

        return new EcoDto(
            EcoId: reader.GetInt32(0),
            EcoNumberInt: reader.GetInt32(1),
            EcoNumber: reader.GetString(2),
            ProjectId: reader.GetInt32(3),
            EcoTitle: reader.GetString(4),
            EcoDescription: reader.IsDBNull(5) ? null : reader.GetString(5),
            ReleaseLevel: reader.GetInt32(6),
            EcoState: reader.GetString(7),
            CreatedAt: reader.GetDateTime(8)
        );
    }

    public async Task<EcoDto> UpdateAsync(int ecoId, UpdateEcoRequest request, CancellationToken ct)
    {
        if (ecoId <= 0)
            throw new InvalidOperationException("EcoId is required.");

        if (string.IsNullOrWhiteSpace(request.EcoTitle))
            throw new InvalidOperationException("ECO Title is required.");

        const string sql = """
            UPDATE dbo.eco
            SET
                eco_title = @eco_title,
                eco_description = @eco_description,
                release_level = @release_level,
                eco_state = @eco_state
            OUTPUT
                INSERTED.eco_id,
                INSERTED.project_id,
                INSERTED.eco_number_int,
                INSERTED.eco_number,
                INSERTED.eco_title,
                INSERTED.eco_description,
                INSERTED.release_level,
                INSERTED.eco_state,
                INSERTED.created_at
            WHERE eco_id = @eco_id;
            """;

        await using var connection = CreateConnection();
        await connection.OpenAsync(ct);

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@eco_id", ecoId);
        command.Parameters.AddWithValue("@eco_title", request.EcoTitle.Trim());
        command.Parameters.AddWithValue("@eco_description", (object?)request.EcoDescription ?? DBNull.Value);
        command.Parameters.AddWithValue("@release_level", request.ReleaseLevel);
        command.Parameters.AddWithValue("@eco_state", request.EcoState);

        await using var reader = await command.ExecuteReaderAsync(ct);

        if (!await reader.ReadAsync(ct))
            throw new InvalidOperationException($"ECO {ecoId} was not found.");

        return new EcoDto(
            EcoId: reader.GetInt32(0),
            ProjectId: reader.GetInt32(1),
            EcoNumberInt: reader.GetInt32(2),
            EcoNumber: reader.GetString(3),
            EcoTitle: reader.GetString(4),
            EcoDescription: reader.IsDBNull(5) ? null : reader.GetString(5),
            ReleaseLevel: reader.GetInt32(6),
            EcoState: reader.GetString(7),
            CreatedAt: reader.GetDateTime(8)
        );
    }
}