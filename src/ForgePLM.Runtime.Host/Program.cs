using System;
using System.Windows.Forms;

namespace ForgePLM.Runtime.Host
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();

            // 🔥 Start your Runtime
            RuntimeBootstrap.BuildApp(Array.Empty<string>(), enableSwagger: true)
                .StartAsync()
                .GetAwaiter()
                .GetResult();

            // 🔥 Start tray app
            Application.Run(new TrayApplicationContext());
        }
    }
}