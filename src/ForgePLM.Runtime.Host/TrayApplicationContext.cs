using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace ForgePLM.Runtime.Host
{
    public class TrayApplicationContext : ApplicationContext
    {
        private readonly NotifyIcon _notifyIcon;
        private readonly string _administratorExePath;

        public TrayApplicationContext(string administratorExePath)
        {
            _administratorExePath = administratorExePath;

            string iconPath = Path.Combine(AppContext.BaseDirectory, "Assets", "forgeplm.ico");

            _notifyIcon = new NotifyIcon
            {
                Icon = File.Exists(iconPath) ? new Icon(iconPath) : SystemIcons.Application,
                Visible = true,
                Text = "ForgePLM Runtime"
            };

            var menu = new ContextMenuStrip();

            menu.Items.Add("Open Swagger", null, (s, e) =>
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "http://localhost:5269/swagger",
                    UseShellExecute = true
                });
            });

            menu.Items.Add("Open Admin", null, (s, e) =>
            {
                OpenAdministrator();
            });

            menu.Items.Add(new ToolStripSeparator());

            menu.Items.Add("Exit", null, (s, e) =>
            {
                _notifyIcon.Visible = false;
                Application.Exit();
            });

            _notifyIcon.ContextMenuStrip = menu;
        }

        private void OpenAdministrator()
        {
            var existing = Process.GetProcessesByName("ForgePLM.Administrator");
            if (existing.Any())
                return;

            if (!File.Exists(_administratorExePath))
            {
                MessageBox.Show(
                    $"ForgePLM Administrator not found:\n{_administratorExePath}",
                    "ForgePLM Runtime",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            Process.Start(new ProcessStartInfo
            {
                FileName = _administratorExePath,
                UseShellExecute = true
            });
        }
    }
}