using SolidWorks.Interop.sldworks;
using System;
using System.IO;

public class FileService
{
    public void EnsureCorrectFileName(ModelDoc2 doc, string partNumber)
    {
        var path = doc.GetPathName();

        if (string.IsNullOrEmpty(path))
            throw new Exception("File must be saved first.");

        var dir = Path.GetDirectoryName(path);
        var newPath = Path.Combine(dir, $"{partNumber}.sldprt");

        if (!path.Equals(newPath, StringComparison.OrdinalIgnoreCase))
        {
            File.Move(path, newPath);
        }
    }
}