using ForgePLM.Administrator.Services;
using ForgePLM.Administrator.Views;
using System;
using System.Windows;
using System.Windows.Input;

namespace ForgePLM.Administrator
{
    public partial class MainWindow : Window
    {
        private readonly ForgePlmAdminApiClient _apiClient = new();

        public MainWindow()
        {
            InitializeComponent();
            ShowView(new DashboardView());
            Loaded += MainWindow_Loaded;
            VersionTextBlock.Text = BuildInfo.DisplayVersion;

            VersionTextBlock.ToolTip =
                $"Built: {BuildInfo.BuildDate}\nCommit: {BuildInfo.Commit}";
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
            {
                ServiceStatusTextBlock.Text = "Service: Offline";
            }
        }

        private void DashboardButton_Click(object sender, RoutedEventArgs e)
        {
            ShowView(new DashboardView());
        }

        private void CrmLiteButton_Click(object sender, RoutedEventArgs e)
        {
            ShowView(new CrmLiteView());
        }

        private void EcoBuilderButton_Click(object sender, RoutedEventArgs e)
        {
            ShowView(new EcoBuilderView());
        }

        private void PartNumberManagerButton_Click(object sender, RoutedEventArgs e)
        {
            ShowView(new PartNumberManagerView());
        }
    }
}