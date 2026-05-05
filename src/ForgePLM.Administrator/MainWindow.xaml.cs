using ForgePLM.Administrator.Services;
using ForgePLM.Administrator.Views;
using Microsoft.Extensions.Configuration;
using System;
using System.Net.Http;
using System.Windows;
using System.Windows.Input;
using System.IO;

namespace ForgePLM.Administrator
{
    public partial class MainWindow : Window
    {
        private readonly ForgePlmAdminApiClient _apiClient;
        private readonly IConfiguration _config;

        public MainWindow()
        {
            InitializeComponent();

            _config = new ConfigurationBuilder()
                .AddJsonFile("E:\\ForgePLM\\config\\appsettings.json",
                    optional: false,
                    reloadOnChange: true)
                .Build();

            var baseUrl = _config["Api:BaseUrl"] ?? "http://localhost:5269";

            _apiClient = new ForgePlmAdminApiClient(new HttpClient
            {
                BaseAddress = new Uri(baseUrl)
            });

            ShowView(new DashboardView()); // or new DashboardView()

            System.Diagnostics.Debug.WriteLine("MainWindow constructor finished");
        }
        private void ShowView(object view)
        {
            MainContentControl.Content = view;

            if (view is INavigationView navigationView)
            {
                PageTitleTextBlock.Text = navigationView.ViewTitle;
            }
            else
            {
                PageTitleTextBlock.Text = "ForgePLM";
            }


            
        }
        private void VersionTextBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            MessageBox.Show(
                $"Version: {BuildInfo.DisplayVersion}\n" +
                $"Built: {BuildInfo.BuildDate}\n" +
                $"Commit: {BuildInfo.Commit}",
                "ForgePLM Build Info");
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                await _apiClient.GetHealthAsync();
                ServiceStatusTextBlock.Text = "Service: Online";
            }
            catch
            { }
                

        }

        private void DashboardButton_Click(object sender, RoutedEventArgs e)
        {
            
            ShowView(new DashboardView());
        }

        private void CrmLiteButton_Click(object sender, RoutedEventArgs e)
        {
            var view = new CrmLiteView(_apiClient);
            ShowView(view);
        }

        private void EcoBuilderButton_Click(object sender, RoutedEventArgs e)
        {
            var view = new EcoBuilderView(_apiClient);
            ShowView(view);
        }

        private void PartNumberManagerButton_Click(object sender, RoutedEventArgs e)
        {
            ShowView(new PartNumberManagerView());
        }

        private void ArtifactGeneratorButton_Click(object sender, RoutedEventArgs e)
        {
            var view = new ArtifactGeneratorView(_apiClient, _config);
            ShowView(view);
        }
    }
}