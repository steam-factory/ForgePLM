using Forms = System.Windows.Forms;

namespace ForgePLM.Preview.Edrawings;

public sealed class EDrawingsHost : Forms.AxHost
{
    private dynamic? _control;
    private bool _isReady;

    public EDrawingsHost()
        : base("22945A69-1191-4DCF-9E6F-409BDE94D101")
    {
    }

    protected override void OnCreateControl()
    {
        base.OnCreateControl();

        _control = GetOcx();
        _isReady = _control != null;

        System.Diagnostics.Debug.WriteLine(
            $"eDrawings control created. Ready = {_isReady}");
    }

    public void OpenFile(string filePath)
    {
        System.Diagnostics.Debug.WriteLine($"eDrawings OpenFile: {filePath}");
        System.Diagnostics.Debug.WriteLine($"Exists: {File.Exists(filePath)}");
        System.Diagnostics.Debug.WriteLine($"Ready: {_isReady}");

        var control = _control;

        if (!_isReady || control is null)
            return;

        control.OpenDoc(filePath, false, false, false, "");
    }

    public void CloseFile()
    {
        var control = _control;

        if (!_isReady || control is null)
            return;

        control.CloseActiveDoc("");
    }
}