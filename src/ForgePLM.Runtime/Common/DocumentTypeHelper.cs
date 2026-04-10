namespace ForgePLM.Runtime.Common;

public static class DocumentTypeHelper
{
    public static string Normalize(string documentType)
    {
        return (documentType ?? string.Empty)
            .Trim()
            .ToUpperInvariant();
    }

    public static string GetExtension(string documentType)
    {
        string dt = Normalize(documentType);

        return dt switch
        {
            "PART" => ".sldprt",
            "ASSEMBLY" => ".sldasm",
            "DRAWING" => ".slddrw",
            _ => throw new InvalidOperationException(
                $"Unsupported document type: '{documentType}' (normalized: '{dt}')")
        };
    }
}