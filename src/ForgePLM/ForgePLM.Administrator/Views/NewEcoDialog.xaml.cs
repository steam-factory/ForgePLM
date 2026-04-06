using System.Windows;
using System.Windows.Media;
using ForgePLM.Contracts.Projects;

namespace ForgePLM.Administrator.Views
{
    public partial class NewEcoDialog : Window
    {
        public ProjectDto SelectedProject { get; }

        public string EcoTitle => EcoTitleTextBox.Text.Trim();
        public string? EcoDescription =>
            string.IsNullOrWhiteSpace(EcoDescriptionTextBox.Text)
                ? null
                : EcoDescriptionTextBox.Text.Trim();

        public int SelectedRevisionFamily =>
            RevFamily300Radio.IsChecked == true ? 300 :
            RevFamily200Radio.IsChecked == true ? 200 :
            100;

        public string EcoState => "Development";

        public NewEcoDialog(ProjectDto selectedProject)
        {
            InitializeComponent();

            SelectedProject = selectedProject;

            Loaded += NewEcoDialog_Loaded;
        }

        private void NewEcoDialog_Loaded(object sender, RoutedEventArgs e)
        {
            EcoNumberTextBox.Text = "ECO-####";
            ProjectDisplayTextBox.Text = $"{SelectedProject.ProjectCode} | {SelectedProject.ProjectName}";

            SetEcoState("Development");

            RevFamily100Radio.IsChecked = true;
            EcoTitleTextBox.Focus();
        }

        private void SetEcoState(string state)
        {
            EcoStateValueTextBlock.Text = state;

            switch (state)
            {
                case "Development":
                    EcoStateBorder.Background = Brushes.DodgerBlue;
                    EcoStateValueTextBlock.Foreground = Brushes.White;
                    break;

                case "Staged":
                    EcoStateBorder.Background = Brushes.Orange;
                    EcoStateValueTextBlock.Foreground = Brushes.Black;
                    break;

                case "Released":
                    EcoStateBorder.Background = Brushes.ForestGreen;
                    EcoStateValueTextBlock.Foreground = Brushes.White;
                    break;

                case "Cancelled":
                    EcoStateBorder.Background = Brushes.Gray;
                    EcoStateValueTextBlock.Foreground = Brushes.Black;
                    break;

                default:
                    EcoStateBorder.Background = Brushes.LightGray;
                    EcoStateValueTextBlock.Foreground = Brushes.Black;
                    break;
            }
        }




        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void CreateEcoButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(EcoTitleTextBox.Text))
            {
                MessageBox.Show("ECO Title is required.", "Validation");
                EcoTitleTextBox.Focus();
                return;
            }

            DialogResult = true;
            Close();
        }
    }
}