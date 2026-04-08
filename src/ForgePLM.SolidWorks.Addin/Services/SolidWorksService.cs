using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;


public class SolidWorksService
{
    private readonly SldWorks _swApp;

    public SolidWorksService(SldWorks swApp)
    {
        _swApp = swApp;
    }

    public ModelDoc2 GetActiveDoc()
    {
        return _swApp.ActiveDoc;
    }

    public void SetCustomProperty(ModelDoc2 doc, string name, string value)
    {
        var custPropMgr = doc.Extension.CustomPropertyManager[""];
        custPropMgr.Add3(name,
            (int)swCustomInfoType_e.swCustomInfoText,
            value,
            (int)swCustomPropertyAddOption_e.swCustomPropertyReplaceValue);
    }
}