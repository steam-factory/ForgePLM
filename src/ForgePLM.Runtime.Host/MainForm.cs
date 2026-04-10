using ForgePLM.Runtime;
using Microsoft.AspNetCore.Builder;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.AspNetCore.Hosting.Server.Features;

namespace ForgePLM.Runtime.Host
{
    public partial class MainForm : Form
    {
        private WebApplication? _app;
        private NotifyIcon? _tray;

        public MainForm()
        {
            InitializeComponent();
        }

        protected override async void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            Hide();
            ShowInTaskbar = false;
            WindowState = FormWindowState.Minimized;

            try
            {
                _app = RuntimeBootstrap.BuildApp(Array.Empty<string>(), enableSwagger: true);
                await _app.StartAsync();

                var addressesFeature = _app.Services
                .GetService(typeof(Microsoft.AspNetCore.Hosting.Server.IServer)) as Microsoft.AspNetCore.Hosting.Server.IServer;

                var boundAddresses = addressesFeature?
                    .Features
                    .Get<IServerAddressesFeature>()?
                    .Addresses;

                string addressText = boundAddresses == null
                    ? "[no addresses found]"
                    : string.Join(Environment.NewLine, boundAddresses);

                //MessageBox.Show($"ForgePLM Runtime started from Host.\n\nBound addresses:\n{addressText}", "ForgePLM.Runtime.Host");
                _tray = new NotifyIcon
                {
                    Icon = SystemIcons.Application,
                    Visible = true,
                    Text = "ForgePLM Runtime"
                };

                var menu = new ContextMenuStrip();

                menu.Items.Add("Open Swagger", null, (_, __) =>
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "http://localhost:5269/swagger",
                        UseShellExecute = true
                    });
                });

                menu.Items.Add("Open Health", null, (_, __) =>
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "http://localhost:5269/health",
                        UseShellExecute = true
                    });
                });

                menu.Items.Add("Exit", null, async (_, __) =>
                {
                    await ShutdownAsync();
                    Application.Exit();
                });

                _tray.ContextMenuStrip = menu;

                //MessageBox.Show("ForgePLM Runtime started from Host.", "ForgePLM.Runtime.Host");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to start ForgePLM Runtime from Host:\n\n{ex}", "ForgePLM.Runtime.Host");
            }
        }

        private async Task ShutdownAsync()
        {
            if (_app != null)
            {
                await _app.StopAsync();
                _app = null;
            }

            if (_tray != null)
            {
                _tray.Visible = false;
                _tray.Dispose();
                _tray = null;
            }
        }

        protected override async void OnFormClosing(FormClosingEventArgs e)
        {
            await ShutdownAsync();
            base.OnFormClosing(e);
        }
    }
}