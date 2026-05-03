using ForgePLM.Contracts.Artifacts;
using ForgePLM.Runtime.Models;
using Microsoft.Data.SqlClient;
using System.IO.Compression;

namespace ForgePLM.Runtime.Services;

public class ArtifactBatchService : IArtifactBatchService
{
    private const string ProductionRootPath = @"E:\ForgePLM\production";
    private const string DevelopmentProjectsRootPath = @"E:\SteamFactory_DEV\Projects";

    private readonly string _connectionString;
    private readonly IArtifactGenerationJobStore _jobStore;

    public ArtifactBatchService(
        IConfiguration configuration,
        IArtifactGenerationJobStore jobStore)
    {
        _connectionString = configuration.GetConnectionString("ForgePlmDb")
            ?? throw new InvalidOperationException("Missing connection string: ForgePlmDb");

        _jobStore = jobStore;
    }
    
    public Task<ArtifactBatchDto> GenerateArtifactBatchAsync(
        GenerateArtifactBatchRequest request,
        CancellationToken cancellationToken = default)
    {
        return GenerateArtifactBatchAsync(
            request,
            Guid.Empty,
            cancellationToken);
    }

    public async Task<ArtifactBatchDto> GenerateArtifactBatchAsync(
        GenerateArtifactBatchRequest request,
        Guid jobId,
        CancellationToken cancellationToken = default)
    {
        bool trackProgress = jobId != Guid.Empty;

        ValidateRequest(request);

        if (trackProgress)
            _jobStore.Update(jobId, "processing", 0, 0, "Creating artifact batch...");

        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken);

        ArtifactBatchHeader batchHeader = await CreateArtifactBatchAsync(
            conn,
            request,
            cancellationToken);

        var workItems = await ResolveArtifactWorkItemsAsync(
            conn,
            request.EcoId,
            request.RevisionIds,
            cancellationToken);

        if (workItems.Count == 0)
            throw new InvalidOperationException("No matching ECO revisions were found for artifact generation.");

        var requestedOutputs = request.Outputs.ToList();

        int totalSteps = workItems.Count * requestedOutputs.Count;

        int completedSteps = 0;

        string realOutputRoot = Path.Combine(
        ProductionRootPath,
        batchHeader.BatchCode);

        string zipPath = Path.Combine(
        Path.GetDirectoryName(realOutputRoot)!,
        $"{batchHeader.BatchCode}.zip");

        Directory.CreateDirectory(realOutputRoot);

            await UpdateBatchOutputPathAsync(
                conn,
                batchHeader.ArtifactBatchId,
                realOutputRoot,
                cancellationToken);

        if (trackProgress)
        {
            _jobStore.Update(
                jobId,
                "processing",
                completedSteps,
                totalSteps,
                $"Starting {totalSteps} export(s)...");
        }

        var artifacts = new List<ArtifactDto>();
        var exporter = new SolidWorksArtifactExportService();

        foreach (var item in workItems)
        {
            foreach (var output in requestedOutputs)
            {
                string outputType = output.OutputType.Trim().ToUpperInvariant();
                string variant = string.IsNullOrWhiteSpace(output.Variant)
                    ? string.Empty
                    : output.Variant.Trim();

                if (trackProgress)
                {
                    _jobStore.Update(
                        jobId,
                        "processing",
                        completedSteps,
                        totalSteps,
                        $"Generating {item.DisplayCompositeCode} {outputType} {variant}...");
                }

                ArtifactDto artifact = outputType switch
                {
                    "STEP" => await ExportStepArtifactAsync(
                        conn, exporter, batchHeader, item, variant, realOutputRoot, cancellationToken),

                    "STL" => await ExportStlArtifactAsync(
                        conn, exporter, batchHeader, item, variant, realOutputRoot, cancellationToken),

                    "SW_NATIVE" => await CopyNativeArtifactAsync(
                        conn, batchHeader, item, variant, realOutputRoot, cancellationToken),

                    "PDF" => await ExportPdfArtifactAsync(
                        conn, exporter, batchHeader, item, variant, realOutputRoot, cancellationToken),

                    _ => throw new InvalidOperationException($"Unsupported output type: {outputType}")
                };

                artifacts.Add(artifact);
                completedSteps++;
            }
        }

        string? zipFilePath = batchHeader.ZipFilePath;

        // If ZIP creation was requested, create the ZIP package after all artifacts have been generated. This ensures that the ZIP file contains all the newly created artifacts. We perform this step at the end to avoid issues with files being added to the ZIP while it's being created, which can lead to file locks and incomplete ZIP contents.
        if (request.CreateZip)
        {
            if (trackProgress)
            {
                _jobStore.Update(
                    jobId,
                    "processing",
                    totalSteps,
                    totalSteps,
                    $"Creating ZIP package for {batchHeader.BatchCode}...");
            }

            zipFilePath = await CreateBatchZipAsync(
                conn,
                batchHeader.ArtifactBatchId,
                realOutputRoot,
                batchHeader.BatchCode,
                cancellationToken);
        }

        // after loop finishes, before Complete()
        var result = new ArtifactBatchDto(
        ArtifactBatchId: batchHeader.ArtifactBatchId,
        BatchNumber: batchHeader.BatchNumber,
        BatchCode: batchHeader.BatchCode,
        EcoId: batchHeader.EcoId,
        BatchDescription: batchHeader.BatchDescription,
        BatchState: batchHeader.BatchState,
        OutputRootPath: realOutputRoot, // 👈 FIXED
        ZipFilePath: zipFilePath,
        Artifacts: artifacts);

        if (trackProgress)
        {
            _jobStore.Update(
                jobId,
                "processing",
                totalSteps,
                totalSteps,
                $"Finalizing {batchHeader.BatchCode}...");
        }

        await Task.Delay(2400, cancellationToken);

        if (trackProgress)
        {
            _jobStore.Complete(jobId, result);
        }


        return result;

    }

    private static string BuildSourceFilePath(ArtifactWorkItem item)
    {
        string extension = item.DocumentType.ToUpperInvariant() switch
        {
            "ASSEMBLY" => ".SLDASM",
            "DRAWING" => ".SLDDRW",
            _ => ".SLDPRT"
        };

        return Path.Combine(
            @"E:\SteamFactory_DEV\Projects",
            $"{item.ProjectCode} - {item.ProjectName}",
            "development",
            $"{item.DisplayPartNumber}{extension}");
    }

    //exports
    
    // For SW_NATIVE, we simply copy the original SolidWorks file to the output location and record it as an artifact, without any conversion.
    private async Task<ArtifactDto> CopyNativeArtifactAsync(
        SqlConnection conn,
        ArtifactBatchHeader batchHeader,
        ArtifactWorkItem item,
        string variant,
        string realOutputRoot,
        CancellationToken cancellationToken)
    {
        string sourceFilePath = BuildSourceFilePath(item);
        string outputFolder = Path.Combine(realOutputRoot, "SW_NATIVE");

        Directory.CreateDirectory(outputFolder);

        string extension = Path.GetExtension(sourceFilePath);
        string safeDescription = SanitizeFileName(item.Description);

        string outputFileName =
            $"{item.DisplayCompositeCode}.{batchHeader.BatchCode} {safeDescription}-DRAFT{extension}";

        string outputPath = Path.Combine(outputFolder, outputFileName);

        File.Copy(sourceFilePath, outputPath, overwrite: false);

        var fileInfo = new FileInfo(outputPath);

        var tempArtifact = new ArtifactDto(
            ArtifactId: 0,
            RevisionId: item.RevisionId,
            ArtifactType: "SW_NATIVE",
            Variant: variant,
            FileName: outputFileName,
            FilePath: outputPath,
            ArtifactState: "completed");

        int artifactId = await InsertArtifactRecordAsync(
            conn,
            batchHeader.ArtifactBatchId,
            tempArtifact,
            fileInfo.Length,
            fileHash: null,
            cancellationToken);

        return tempArtifact with { ArtifactId = artifactId };
    }

    // For STEP, we export the part or assembly file to STEP AP214 format using SolidWorks. We support exporting both parts and assemblies to STEP, as this is a common use case for sharing 3D models in a neutral format.
    private async Task<ArtifactDto> ExportStepArtifactAsync(
        SqlConnection conn,
        SolidWorksArtifactExportService exporter,
        ArtifactBatchHeader batchHeader,
        ArtifactWorkItem item,
        string variant,
        string realOutputRoot,
        CancellationToken cancellationToken)
    {
        string sourceFilePath = Path.Combine(
            DevelopmentProjectsRootPath,
            $"{item.ProjectCode} - {item.ProjectName}",
            "development",
            $"{item.DisplayPartNumber}.SLDPRT");

        if (!File.Exists(sourceFilePath))
            throw new FileNotFoundException("Source SolidWorks file was not found.", sourceFilePath);

        string outputFolder = Path.Combine(realOutputRoot, "STEP");

        Directory.CreateDirectory(outputFolder);

        string safeDescription = SanitizeFileName(item.Description);
        string outputFileName =
            $"{item.DisplayCompositeCode}.{batchHeader.BatchCode} {safeDescription}-DRAFT.step";

        string outputPath = Path.Combine(outputFolder, outputFileName);
        string outputRootPath = Path.Combine(@"E:\ForgePLM\production", batchHeader.BatchCode);

        Directory.CreateDirectory(realOutputRoot);

        await UpdateBatchOutputPathAsync(
            conn,
            batchHeader.ArtifactBatchId,
            realOutputRoot,
            cancellationToken);
        await exporter.ExportStepAp214Async(sourceFilePath, outputPath);

        var fileInfo = new FileInfo(outputPath);

        var tempArtifact = new ArtifactDto(
            ArtifactId: 0,
            RevisionId: item.RevisionId,
            ArtifactType: "STEP",
            Variant: variant,
            FileName: outputFileName,
            FilePath: outputPath,
            ArtifactState: "completed");

        int artifactId = await InsertArtifactRecordAsync(
            conn,
            batchHeader.ArtifactBatchId,
            tempArtifact,
            fileInfo.Length,
            fileHash: null,
            cancellationToken);

        return tempArtifact with { ArtifactId = artifactId };
    }

    // For STL, we export the part file to STL format using SolidWorks. We only support exporting parts to STL, as assemblies and drawings typically require different handling and may not be suitable for STL output.
    private async Task<ArtifactDto> ExportStlArtifactAsync(
        SqlConnection conn,
        SolidWorksArtifactExportService exporter,
        ArtifactBatchHeader batchHeader,
        ArtifactWorkItem item,
        string variant,
        string realOutputRoot,
        CancellationToken cancellationToken)
    {
        if (!string.Equals(item.DocumentType, "PART", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("STL output is only supported for parts.");

        string sourceFilePath = BuildSourceFilePath(item);
        string outputFolder = Path.Combine(realOutputRoot, "STL");
        Directory.CreateDirectory(outputFolder);

        string safeDescription = SanitizeFileName(item.Description);
        string outputFileName =
            $"{item.DisplayCompositeCode}.{batchHeader.BatchCode} {safeDescription}-DRAFT.stl";

        string outputPath = Path.Combine(outputFolder, outputFileName);

        await exporter.ExportStlAsync(sourceFilePath, outputPath, variant);

        var fileInfo = new FileInfo(outputPath);

        var tempArtifact = new ArtifactDto(
            0,
            item.RevisionId,
            "STL",
            variant,
            outputFileName,
            outputPath,
            "completed");

        int artifactId = await InsertArtifactRecordAsync(
            conn,
            batchHeader.ArtifactBatchId,
            tempArtifact,
            fileInfo.Length,
            null,
            cancellationToken);

        return tempArtifact with { ArtifactId = artifactId };
    }



    // For PDF, we export the drawing file to PDF format using SolidWorks. We only support exporting drawings to PDF, as parts and assemblies typically require different handling and may not be suitable for PDF output.
    private async Task<ArtifactDto> ExportPdfArtifactAsync(
            SqlConnection conn,
            SolidWorksArtifactExportService exporter,
            ArtifactBatchHeader batchHeader,
            ArtifactWorkItem item,
            string variant,
            string realOutputRoot,
            CancellationToken cancellationToken)
    {
        if (!string.Equals(item.DocumentType, "DRAWING", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("PDF output is only supported for drawings.");

        string sourceFilePath = BuildSourceFilePath(item);
        string outputFolder = Path.Combine(realOutputRoot, "PDF");
        Directory.CreateDirectory(outputFolder);

        string safeDescription = SanitizeFileName(item.Description);
        string outputFileName =
            $"{item.DisplayCompositeCode}.{batchHeader.BatchCode} {safeDescription}-DRAFT.pdf";

        string outputPath = Path.Combine(outputFolder, outputFileName);

        await exporter.ExportPdfAsync(sourceFilePath, outputPath);

        var fileInfo = new FileInfo(outputPath);

        var tempArtifact = new ArtifactDto(
            0,
            item.RevisionId,
            "PDF",
            variant,
            outputFileName,
            outputPath,
            "completed");

        int artifactId = await InsertArtifactRecordAsync(
            conn,
            batchHeader.ArtifactBatchId,
            tempArtifact,
            fileInfo.Length,
            null,
            cancellationToken);

        return tempArtifact with { ArtifactId = artifactId };
    }

    private static void ValidateRequest(GenerateArtifactBatchRequest request)
    {
        if (request.EcoId <= 0)
            throw new InvalidOperationException("ECO is required.");

        if (string.IsNullOrWhiteSpace(request.BatchDescription))
            throw new InvalidOperationException("Batch description is required.");

        if (request.RevisionIds == null || request.RevisionIds.Count == 0)
            throw new InvalidOperationException("At least one revision is required.");

        if (request.Outputs == null || request.Outputs.Count == 0)
            throw new InvalidOperationException("At least one output type is required.");
    }

    private async Task<ArtifactBatchHeader> CreateArtifactBatchAsync(
        SqlConnection conn,
        GenerateArtifactBatchRequest request,
        CancellationToken cancellationToken)
    {
        const string sql = @"
        INSERT INTO dbo.artifact_batches
        (
            eco_id,
            batch_description,
            batch_state,
            create_zip,
            archive_previous,
            output_root_path,
            zip_file_path,
            created_by,
            created_utc
        )
        OUTPUT
            INSERTED.artifact_batch_id,
            INSERTED.batch_number,
            INSERTED.batch_code,
            INSERTED.eco_id,
            INSERTED.batch_description,
            INSERTED.batch_state,
            INSERTED.output_root_path,
            INSERTED.zip_file_path
        VALUES
        (
            @eco_id,
            @batch_description,
            'created',
            @create_zip,
            @archive_previous,
            @output_root_path,
            NULL,
            @created_by,
            SYSUTCDATETIME()
        );";

        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@eco_id", request.EcoId);
        cmd.Parameters.AddWithValue("@batch_description", request.BatchDescription.Trim());
        cmd.Parameters.AddWithValue("@create_zip", request.CreateZip);
        cmd.Parameters.AddWithValue("@archive_previous", request.ArchivePrevious);
        cmd.Parameters.AddWithValue("@output_root_path", Path.Combine(ProductionRootPath, "pending"));
        cmd.Parameters.AddWithValue("@created_by", Environment.UserName);

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

        if (!await reader.ReadAsync(cancellationToken))
            throw new InvalidOperationException("Artifact batch insert returned no row.");

        return new ArtifactBatchHeader
        {
            ArtifactBatchId = reader.GetInt32(reader.GetOrdinal("artifact_batch_id")),
            BatchNumber = Convert.ToInt64(reader["batch_number"]),
            BatchCode = reader["batch_code"] as string ?? string.Empty,
            EcoId = reader.GetInt32(reader.GetOrdinal("eco_id")),
            BatchDescription = reader["batch_description"] as string ?? string.Empty,
            BatchState = reader["batch_state"] as string ?? string.Empty,
            OutputRootPath = reader["output_root_path"] as string ?? string.Empty,
            ZipFilePath = reader["zip_file_path"] as string
        };
    }

    private async Task<string> CreateBatchZipAsync(
    SqlConnection conn,
    int artifactBatchId,
    string realOutputRoot,
    string batchCode,
    CancellationToken cancellationToken)
    {
        string parentFolder = Path.GetDirectoryName(realOutputRoot)
            ?? throw new InvalidOperationException("Could not determine ZIP parent folder.");
        string packageFolder = Path.Combine(realOutputRoot, "package");
        Directory.CreateDirectory(packageFolder);
        string zipPath = Path.Combine(packageFolder, $"{batchCode}.zip");



        if (File.Exists(zipPath))
            File.Delete(zipPath);


        using (var archive = ZipFile.Open(zipPath, ZipArchiveMode.Create))
        {
            foreach (string filePath in Directory.EnumerateFiles(realOutputRoot, "*.*", SearchOption.AllDirectories))
            {
                if (filePath.StartsWith(packageFolder, StringComparison.OrdinalIgnoreCase))
                    continue;

                string entryName = Path.GetRelativePath(realOutputRoot, filePath);
                archive.CreateEntryFromFile(filePath, entryName, CompressionLevel.Optimal);
            }
        }

        await UpdateBatchZipPathAsync(
            conn,
            artifactBatchId,
            zipPath,
            cancellationToken);

        return zipPath;
    }

    private async Task UpdateBatchZipPathAsync(
    SqlConnection conn,
    int artifactBatchId,
    string zipFilePath,
    CancellationToken cancellationToken)
    {
        const string sql = @"
        UPDATE dbo.artifact_batches
        SET zip_file_path = @zip_file_path
        WHERE artifact_batch_id = @artifact_batch_id;";

        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@artifact_batch_id", artifactBatchId);
        cmd.Parameters.AddWithValue("@zip_file_path", zipFilePath);

        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }


    private async Task UpdateBatchOutputPathAsync(
    SqlConnection conn,
    int artifactBatchId,
    string outputRootPath,
    CancellationToken cancellationToken)
    {
        const string sql = @"
        UPDATE dbo.artifact_batches
        SET output_root_path = @output_root_path
        WHERE artifact_batch_id = @artifact_batch_id;";

        await using var cmd = new SqlCommand(sql, conn);

        cmd.Parameters.AddWithValue("@artifact_batch_id", artifactBatchId);
        cmd.Parameters.AddWithValue("@output_root_path", outputRootPath);

        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    private static string SanitizeFileName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "Artifact";

        foreach (char c in Path.GetInvalidFileNameChars())
            value = value.Replace(c, '-');

        return value.Trim().TrimEnd('.');
    }

    private async Task<int> InsertArtifactRecordAsync(
    SqlConnection conn,
    int artifactBatchId,
    ArtifactDto artifact,
    long fileSizeBytes,
    string? fileHash,
    CancellationToken cancellationToken)
    {
        const string sql = @"
    INSERT INTO dbo.artifacts
    (
        artifact_batch_id,
        revision_id,
        artifact_type,
        variant,
        file_name,
        file_path,
        artifact_state,
        file_size_bytes,
        file_hash,
        created_utc
    )
    OUTPUT INSERTED.artifact_id
    VALUES
    (
        @artifact_batch_id,
        @revision_id,
        @artifact_type,
        @variant,
        @file_name,
        @file_path,
        @artifact_state,
        @file_size_bytes,
        @file_hash,
        SYSUTCDATETIME()
    );";

        await using var cmd = new SqlCommand(sql, conn);

        cmd.Parameters.AddWithValue("@artifact_batch_id", artifactBatchId);
        cmd.Parameters.AddWithValue("@revision_id", artifact.RevisionId);
        cmd.Parameters.AddWithValue("@artifact_type", artifact.ArtifactType);
        cmd.Parameters.AddWithValue("@variant", (object?)artifact.Variant ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@file_name", artifact.FileName);
        cmd.Parameters.AddWithValue("@file_path", artifact.FilePath);
        cmd.Parameters.AddWithValue("@artifact_state", artifact.ArtifactState);
        cmd.Parameters.AddWithValue("@file_size_bytes", fileSizeBytes);
        cmd.Parameters.AddWithValue("@file_hash", (object?)fileHash ?? DBNull.Value);

        var result = await cmd.ExecuteScalarAsync(cancellationToken);

        return Convert.ToInt32(result);
    }

    private async Task<List<ArtifactWorkItem>> ResolveArtifactWorkItemsAsync(
        SqlConnection conn,
        int ecoId,
        IReadOnlyList<int> revisionIds,
        CancellationToken cancellationToken)
    {
        if (revisionIds == null || revisionIds.Count == 0)
            return new List<ArtifactWorkItem>();



        var results = new List<ArtifactWorkItem>();

        var parameterNames = revisionIds
            .Select((_, index) => $"@revisionId{index}")
            .ToList();

        var sql = $@"
        SELECT
            e.eco_id,
            e.eco_number,
            e.eco_state,

            pr.project_id,
            pr.project_code,
            pr.project_name,

            p.part_id,
            p.category_code,
            p.part_number_int,

            r.revision_id,
            r.revision_code,
            r.part_description,
            r.revision_state,
            p.document_type
        FROM dbo.revisions r
        INNER JOIN dbo.part_numbers p
            ON p.part_id = r.part_id
        INNER JOIN dbo.eco e
            ON e.eco_id = r.eco_id
        INNER JOIN dbo.projects pr
            ON pr.project_id = e.project_id
        WHERE r.eco_id = @ecoId
          AND r.revision_id IN ({string.Join(",", parameterNames)})
        ORDER BY p.category_code, p.part_number_int, r.revision_code;";

        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@ecoId", ecoId);

        for (int i = 0; i < revisionIds.Count; i++)
        {
            cmd.Parameters.AddWithValue(parameterNames[i], revisionIds[i]);
        }

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            results.Add(new ArtifactWorkItem
            {
                EcoId = reader.GetInt32(reader.GetOrdinal("eco_id")),
                EcoNumber = reader["eco_number"] as string ?? string.Empty,
                EcoState = reader["eco_state"] as string ?? string.Empty,

                ProjectId = reader.GetInt32(reader.GetOrdinal("project_id")),
                ProjectCode = reader["project_code"] as string ?? string.Empty,
                ProjectName = reader["project_name"] as string ?? string.Empty,

                PartId = reader.GetInt32(reader.GetOrdinal("part_id")),
                CategoryCode = reader["category_code"] as string ?? string.Empty,
                PartNumberInt = Convert.ToInt32(reader["part_number_int"]),

                RevisionId = reader.GetInt32(reader.GetOrdinal("revision_id")),
                RevisionCode = Convert.ToInt32(reader["revision_code"]),

                Description = reader["part_description"] as string ?? string.Empty,
                RevisionState = reader["revision_state"] as string ?? string.Empty,
                DocumentType = reader["document_type"] as string ?? string.Empty
            });
        }

        return results;
    }


    private sealed class ArtifactBatchHeader
    {
        public int ArtifactBatchId { get; init; }
        public long BatchNumber { get; init; }
        public string BatchCode { get; init; } = string.Empty;
        public int EcoId { get; init; }
        public string BatchDescription { get; init; } = string.Empty;
        public string BatchState { get; init; } = string.Empty;
        public string OutputRootPath { get; init; } = string.Empty;
        public string? ZipFilePath { get; init; }
    }
}
