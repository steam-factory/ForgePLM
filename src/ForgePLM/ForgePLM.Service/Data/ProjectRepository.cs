using ForgePLM.Contracts.Projects;
using Microsoft.Data.SqlClient;

namespace ForgePLM.Service.Data;

public class ProjectRepository
{
    private readonly IConfiguration _configuration;

    public ProjectRepository(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    private SqlConnection CreateConnection()
    {
        var connectionString = _configuration.GetConnectionString("ForgePLMDb")
            ?? throw new InvalidOperationException("Connection string 'ForgePLMDb' was not found.");

        return new SqlConnection(connectionString);
    }

    public async Task<List<ProjectDto>> GetByCustomerAsync(int customerId, CancellationToken ct)
    {
        const string sql = """
            SELECT
                project_id,
                customer_id,
                project_seq,
                project_code,
                project_name,
                is_active
            FROM dbo.projects
            WHERE customer_id = @customer_id
            ORDER BY project_seq;
            """;

        var results = new List<ProjectDto>();

        await using var connection = CreateConnection();
        await connection.OpenAsync(ct);

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@customer_id", customerId);

        await using var reader = await command.ExecuteReaderAsync(ct);

        while (await reader.ReadAsync(ct))
        {
            results.Add(new ProjectDto(
                ProjectId: reader.GetInt32(0),
                CustomerId: reader.GetInt32(1),
                ProjectSeq: reader.GetInt32(2),
                ProjectCode: reader.GetString(3),
                ProjectName: reader.GetString(4),
                IsActive: reader.GetBoolean(5)
            ));
        }

        return results;
    }

    public async Task<ProjectDto> CreateAsync(CreateProjectRequest request, CancellationToken ct)
    {
        if (request.CustomerId <= 0)
            throw new InvalidOperationException("CustomerId is required.");

        if (string.IsNullOrWhiteSpace(request.ProjectName))
            throw new InvalidOperationException("Project Name is required.");

        await using var connection = CreateConnection();
        await connection.OpenAsync(ct);

        await using var transaction = await connection.BeginTransactionAsync(ct);

        try
        {
            string customerCode;
            int nextProjectSeq;

            const string customerSql = """
                SELECT customer_code
                FROM dbo.customers
                WHERE customer_id = @customer_id;
                """;

            await using (var customerCommand = new SqlCommand(customerSql, connection, (SqlTransaction)transaction))
            {
                customerCommand.Parameters.AddWithValue("@customer_id", request.CustomerId);

                var customerCodeObj = await customerCommand.ExecuteScalarAsync(ct);

                if (customerCodeObj is null || customerCodeObj == DBNull.Value)
                    throw new InvalidOperationException($"Customer {request.CustomerId} was not found.");

                customerCode = Convert.ToString(customerCodeObj)?.Trim()
                    ?? throw new InvalidOperationException($"Customer {request.CustomerId} has no customer code.");
            }

            if (customerCode.Length != 5)
                throw new InvalidOperationException(
                    $"Customer code '{customerCode}' must be exactly 5 characters to generate project code.");

            const string seqSql = """
                SELECT ISNULL(MAX(project_seq), 0) + 1
                FROM dbo.projects WITH (UPDLOCK, HOLDLOCK)
                WHERE customer_id = @customer_id;
                """;

            await using (var seqCommand = new SqlCommand(seqSql, connection, (SqlTransaction)transaction))
            {
                seqCommand.Parameters.AddWithValue("@customer_id", request.CustomerId);

                var nextSeqObj = await seqCommand.ExecuteScalarAsync(ct);
                nextProjectSeq = Convert.ToInt32(nextSeqObj);
            }

            if (nextProjectSeq > 9999)
                throw new InvalidOperationException(
                    $"Customer '{customerCode}' exceeded the 4-digit project sequence limit.");

            var generatedProjectCode = $"{customerCode}-{nextProjectSeq:0000}";

            const string insertSql = """
                INSERT INTO dbo.projects
                (
                    project_seq,
                    project_code,
                    project_name,
                    is_active,
                    created_at,
                    customer_id
                )
                OUTPUT
                    INSERTED.project_id,
                    INSERTED.customer_id,
                    INSERTED.project_seq,
                    INSERTED.project_code,
                    INSERTED.project_name,
                    INSERTED.is_active
                VALUES
                (
                    @project_seq,
                    @project_code,
                    @project_name,
                    @is_active,
                    SYSDATETIME(),
                    @customer_id
                );
                """;

            ProjectDto createdProject;

            await using (var insertCommand = new SqlCommand(insertSql, connection, (SqlTransaction)transaction))
            {
                insertCommand.Parameters.AddWithValue("@project_seq", nextProjectSeq);
                insertCommand.Parameters.AddWithValue("@project_code", generatedProjectCode);
                insertCommand.Parameters.AddWithValue("@project_name", request.ProjectName.Trim());
                insertCommand.Parameters.AddWithValue("@is_active", request.IsActive);
                insertCommand.Parameters.AddWithValue("@customer_id", request.CustomerId);

                await using var reader = await insertCommand.ExecuteReaderAsync(ct);

                if (!await reader.ReadAsync(ct))
                    throw new InvalidOperationException("Project insert did not return a row.");

                createdProject = new ProjectDto(
                    ProjectId: reader.GetInt32(0),
                    CustomerId: reader.GetInt32(1),
                    ProjectSeq: reader.GetInt32(2),
                    ProjectCode: reader.GetString(3),
                    ProjectName: reader.GetString(4),
                    IsActive: reader.GetBoolean(5)
                );
            }

            await transaction.CommitAsync(ct);
            return createdProject;
        }
        catch
        {
            await transaction.RollbackAsync(ct);
            throw;
        }
    }

    public async Task<ProjectDto> UpdateAsync(int projectId, UpdateProjectRequest request, CancellationToken ct)
    {
        if (projectId <= 0)
            throw new InvalidOperationException("ProjectId is required.");

        if (string.IsNullOrWhiteSpace(request.ProjectName))
            throw new InvalidOperationException("Project Name is required.");

        const string sql = """
            UPDATE dbo.projects
            SET
                project_name = @project_name,
                is_active = @is_active
            OUTPUT
                INSERTED.project_id,
                INSERTED.customer_id,
                INSERTED.project_seq,
                INSERTED.project_code,
                INSERTED.project_name,
                INSERTED.is_active
            WHERE project_id = @project_id;
            """;

        await using var connection = CreateConnection();
        await connection.OpenAsync(ct);

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@project_id", projectId);
        command.Parameters.AddWithValue("@project_name", request.ProjectName.Trim());
        command.Parameters.AddWithValue("@is_active", request.IsActive);

        await using var reader = await command.ExecuteReaderAsync(ct);

        if (await reader.ReadAsync(ct))
        {
            return new ProjectDto(
                ProjectId: reader.GetInt32(0),
                CustomerId: reader.GetInt32(1),
                ProjectSeq: reader.GetInt32(2),
                ProjectCode: reader.GetString(3),
                ProjectName: reader.GetString(4),
                IsActive: reader.GetBoolean(5)
            );
        }

        throw new InvalidOperationException($"Project {projectId} was not found.");
    }
}