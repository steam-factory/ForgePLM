using ForgePLM.Preview.Edrawings;
using System.IO;
using System.Windows;
using System.Windows.Forms.Integration;
using WpfControls = System.Windows.Controls;

namespace ForgePLM.Administrator.Controls;

public partial class EDrawingsHostControl : WpfControls.UserControl
{
    private readonly EDrawingsHost _host;
    private readonly WindowsFormsHost _formsHost;

    public EDrawingsHostControl()
    {
        InitializeComponent();

        _host = new EDrawingsHost();

        _formsHost = new WindowsFormsHost
        {
            Child = _host
        };

        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        // Inject after layout is ready (prevents ghosting)
        if (Content is WpfControls.Grid grid)
        {
            grid.Children.Clear();
            grid.Children.Add(_formsHost);
        }
    }

    public void OpenFile(string? filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
        {
            _host.CloseFile();
            return;
        }

        // Delay slightly to ensure control is fully realized
        Dispatcher.BeginInvoke(new Action(() =>
        {
            _host.OpenFile(filePath);
        }), System.Windows.Threading.DispatcherPriority.Background);
    }
}