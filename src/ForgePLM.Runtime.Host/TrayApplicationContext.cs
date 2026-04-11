using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace ForgePLM.Runtime.Host
{
    public class TrayApplicationContext : ApplicationContext
    {
        private NotifyIcon _notifyIcon;

        public TrayApplicationContext()
        {
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
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "http://localhost:5269/swagger",
                    UseShellExecute = true
                });
            });

            menu.Items.Add("Exit", null, (s, e) =>
            {
                _notifyIcon.Visible = false;
                Application.Exit();
            });

            _notifyIcon.ContextMenuStrip = menu;
        }
    }
}