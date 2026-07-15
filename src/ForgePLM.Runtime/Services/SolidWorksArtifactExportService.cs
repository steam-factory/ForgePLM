using ForgePLM.Contracts.Artifacts;
using ForgePLM.Runtime.Models;
using Microsoft.Data.SqlClient;
using SwConst;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using SwApp = SldWorks.SldWorks;
using SwModelDoc = SldWorks.ModelDoc2;

namespace ForgePLM.Runtime.Services
{
    public class SolidWorksArtifactExportService
    {
        private Task ExportWithSolidWorksAsync(
    string sourceFilePath,
    string outputPath,
    Action<SwApp>? configureExport,
    string exportLabel,
    int saveOptions = (int)SwConst.swSaveAsOptions_e.swSaveAsOptions_Silent)
        {
            return Task.Run(() =>
            {
                if (string.IsNullOrWhiteSpace(sourceFilePath))
                    throw new InvalidOperationException("Source file path is missing.");

                if (!File.Exists(sourceFilePath))
                    throw new FileNotFoundException("Source SolidWorks file was not found.", sourceFilePath);

                string? outputDirectory = Path.GetDirectoryName(outputPath);

                if (string.IsNullOrWhiteSpace(outputDirectory))
                    throw new InvalidOperationException("Output directory could not be determined.");

                Directory.CreateDirectory(outputDirectory);

                SwApp swApp = GetOrStartSolidWorks();

                swApp.Visible = false;
                swApp.UserControl = false;
                swApp.CommandInProgress = true;

                int errors = 0;
                int warnings = 0;

                int docType = GetDocumentType(sourceFilePath);

                bool wasAlreadyOpen = swApp.GetOpenDocumentByName(sourceFilePath) != null;

                SwModelDoc model = swApp.OpenDoc6(
                    sourceFilePath,
                    docType,
                    (int)SwConst.swOpenDocOptions_e.swOpenDocOptions_Silent,
                    "",
                    ref errors,
                    ref warnings);

                    if (model == null)
                    {
                        throw new InvalidOperationException(
                            $"SolidWorks failed to open source file.\n\n" +
                            $"File: {sourceFilePath}\n" +
                            $"Errors: {errors}\n" +
                            $"Warnings: {warnings}");
                    }

                    int activationErrors = 0;

                    SwModelDoc? activeModel = swApp.ActivateDoc3(
                        model.GetTitle(),
                        false,
                        (int)SwConst.swRebuildOnActivation_e.swUserDecision,
                        ref activationErrors);

                    if (activeModel == null)
                    {
                        throw new InvalidOperationException(
                            $"SolidWorks failed to activate the source document.\n\n" +
                            $"File: {sourceFilePath}\n" +
                            $"Activation errors: {activationErrors}");
                    }

                    model = activeModel;

                try
                {
                    configureExport?.Invoke(swApp);

                    int saveErrors = 0;
                    int saveWarnings = 0;

                    SwModelDoc? verifiedActiveModel = swApp.ActiveDoc as SwModelDoc;

                    if (verifiedActiveModel == null ||
                        !string.Equals(
                            verifiedActiveModel.GetPathName(),
                            sourceFilePath,
                            StringComparison.OrdinalIgnoreCase))
                    {
                        throw new InvalidOperationException(
                            $"SolidWorks activated the wrong document.\n\n" +
                            $"Expected: {sourceFilePath}\n" +
                            $"Active: {verifiedActiveModel?.GetPathName() ?? "(none)"}");
                    }

                    model = verifiedActiveModel;

                    bool success = model.Extension.SaveAs(
                        outputPath,
                        (int)SwConst.swSaveAsVersion_e.swSaveAsCurrentVersion,
                        saveOptions,
                        null,
                        ref saveErrors,
                        ref saveWarnings);

                    if (!success)
                    {
                        throw new InvalidOperationException(
                            $"{exportLabel} export failed.\n\nOutput: {outputPath}\nErrors: {saveErrors}\nWarnings: {saveWarnings}");
                    }
                }
                finally
                {
                    if (!wasAlreadyOpen)
                        swApp.CloseDoc(model.GetTitle());

                    swApp.CommandInProgress = false;
                }
            });
        }
        public Task ExportStepAp214Async(
        string sourceFilePath,
        string outputStepPath)
        {
            return ExportWithSolidWorksAsync(
                sourceFilePath,
                outputStepPath,
                swApp =>
                {
                    swApp.SetUserPreferenceIntegerValue(
                        (int)SwConst.swUserPreferenceIntegerValue_e.swStepAP,
                        214);
                },
                "STEP");
        }

        public Task ExportStlAsync(
            string sourceFilePath,
            string outputStlPath,
            string quality)
        {
            return ExportWithSolidWorksAsync(
                sourceFilePath,
                outputStlPath,
                swApp =>
                {
                    swApp.SetUserPreferenceIntegerValue(
                        (int)SwConst.swUserPreferenceIntegerValue_e.swSTLQuality,
                        (int)SwConst.swSTLQuality_e.swSTLQuality_Custom);

                    // TODO: tune fine/coarse STL preferences later
                },
                "STL");
        }

        public Task ExportPdfAsync(
            string sourceFilePath,
            string outputPdfPath)
        {
            return ExportWithSolidWorksAsync(
                sourceFilePath,
                outputPdfPath,
                null,
                "PDF");
        }


        public Task ExportNativeCopyAsync(
            string sourceFilePath,
            string outputNativePath)
        {
            return ExportWithSolidWorksAsync(
                sourceFilePath,
                outputNativePath,
                null,
                "SolidWorks Native",
                (int)SwConst.swSaveAsOptions_e.swSaveAsOptions_Silent |
                (int)SwConst.swSaveAsOptions_e.swSaveAsOptions_Copy);
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
            cmd.Parameters.AddWithValue("@variant", string.IsNullOrWhiteSpace(artifact.Variant) ? DBNull.Value : artifact.Variant);
            cmd.Parameters.AddWithValue("@file_name", artifact.FileName);
            cmd.Parameters.AddWithValue("@file_path", artifact.FilePath);
            cmd.Parameters.AddWithValue("@artifact_state", artifact.ArtifactState);
            cmd.Parameters.AddWithValue("@file_size_bytes", fileSizeBytes);
            cmd.Parameters.AddWithValue("@file_hash", string.IsNullOrWhiteSpace(fileHash) ? DBNull.Value : fileHash);

            object? result = await cmd.ExecuteScalarAsync(cancellationToken);

            return Convert.ToInt32(result);
        }


        private static bool TryGetActiveObject(string progId, out object? activeObject)
        {
            activeObject = null;

            Guid clsid;
            int hr = CLSIDFromProgID(progId, out clsid);

            if (hr < 0)
                return false;

            try
            {
                GetActiveObject(ref clsid, IntPtr.Zero, out activeObject);
                return activeObject != null;
            }
            catch
            {
                activeObject = null;
                return false;
            }
        }



        [System.Runtime.InteropServices.DllImport("ole32.dll")]
        private static extern int CLSIDFromProgID(
        [System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.LPWStr)] string progId,
        out Guid clsid);

        [System.Runtime.InteropServices.DllImport("oleaut32.dll", PreserveSig = false)]
        private static extern void GetActiveObject(
            ref Guid rclsid,
            IntPtr pvReserved,
            [System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.IUnknown)] out object? ppunk);



        private static SwApp GetOrStartSolidWorks()
        {
            if (!OperatingSystem.IsWindows())
                throw new PlatformNotSupportedException("SolidWorks automation is only supported on Windows.");

            object? activeObject;

            if (TryGetActiveObject("SldWorks.Application", out activeObject) && activeObject != null)
                return (SwApp)activeObject;

            Type? swType = Type.GetTypeFromProgID("SldWorks.Application");

            if (swType == null)
                throw new InvalidOperationException("SolidWorks is not installed or COM registration is missing.");

            object? instance = Activator.CreateInstance(swType);

            if (instance == null)
                throw new InvalidOperationException("Failed to create SolidWorks instance.");

            return (SwApp)instance;
        }


        private static int GetDocumentType(string filePath)
        {
            string ext = Path.GetExtension(filePath).ToLowerInvariant();

            if (ext == ".sldprt")
                return (int)SwConst.swDocumentTypes_e.swDocPART;

            if (ext == ".sldasm")
                return (int)SwConst.swDocumentTypes_e.swDocASSEMBLY;

            if (ext == ".slddrw")
                return (int)SwConst.swDocumentTypes_e.swDocDRAWING;

            throw new InvalidOperationException("Unsupported SolidWorks document type: " + ext);
        }

        
    }
}