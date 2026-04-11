using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Windows.Forms;

namespace ForgePLM.Runtime.Host
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();

            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            string administratorExePath =
                configuration["Paths:AdministratorExe"]
                ?? throw new InvalidOperationException("Missing configuration: Paths:AdministratorExe");

            RuntimeBootstrap.BuildApp(Array.Empty<string>(), enableSwagger: true)
                .StartAsync()
                .GetAwaiter()
                .GetResult();

            Application.Run(new TrayApplicationContext(administratorExePath));
        }
    }
}