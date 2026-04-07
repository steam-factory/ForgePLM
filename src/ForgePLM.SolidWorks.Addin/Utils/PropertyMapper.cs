using ForgePLM.SolidWorks.Addin.Models;
using SolidWorks.Interop.sldworks;

public static class PropertyMapper
{
    public static void ApplyRevision(ModelDoc2 doc, RevisionDto rev, SolidWorksService sw)
    {
        sw.SetCustomProperty(doc, "PartNumber", rev.PartNumber);
        sw.SetCustomProperty(doc, "Revision", rev.RevisionCode);
        sw.SetCustomProperty(doc, "Description", rev.Description);
        sw.SetCustomProperty(doc, "ECO", rev.EcoNumber);

        // Hidden anchors (future-proof)
        sw.SetCustomProperty(doc, "PartId", rev.PartId.ToString());
        sw.SetCustomProperty(doc, "RevisionId", rev.RevisionId.ToString());
    }
}