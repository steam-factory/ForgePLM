using ForgePLM.Administrator.Services;
using ForgePLM.Contracts.Artifacts;
using ForgePLM.Contracts.Customers;
using ForgePLM.Contracts.Eco;
using ForgePLM.Contracts.Projects;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;



namespace ForgePLM.Administrator.Views
{
    public partial class ArtifactGeneratorView : UserControl, INavigationView
    {
        public string ViewTitle => "Artifact Generator";
        private readonly ForgePlmAdminApiClient _client = new ForgePlmAdminApiClient();
        private ArtifactBatchDto? _lastBatch;

        public ObservableCollection<ArtifactPartRowViewModel> EcoParts { get; }
            = new ObservableCollection<ArtifactPartRowViewModel>();

        public ArtifactGeneratorView()
        {
            InitializeComponent();

            ArtifactPartsGrid.ItemsSource = EcoParts;

            Loaded += ArtifactGeneratorView_Loaded;
        }



        private void ValidateButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                "Validation placeholder.\n\nNext step: selected rows × selected output options.",
                "Artifact Generator");
        }


        private EcoDto? _selectedEco;

        private async void ArtifactEcoComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            EcoParts.Clear();

            _selectedEco = ArtifactEcoComboBox.SelectedItem as EcoDto;
            if (_selectedEco == null)
                return;

            ArtifactEcoStateTextBox.Text = _selectedEco.EcoState ?? string.Empty;

            var rows = await _client.GetArtifactEcoContentsAsync(_selectedEco.EcoId);

            foreach (var row in rows)
            {
                EcoParts.Add(new ArtifactPartRowViewModel
                {
                    IsSelected = false,

                    EcoId = _selectedEco.EcoId,
                    PartId = row.PartId,
                    RevisionId = row.RevisionId,

                    PartNumber = row.DisplayPartNumber,
                    Revision = row.RevisionCode.ToString(),
                    Description = row.Description ?? string.Empty,
                    RevisionState = row.RevisionState ?? string.Empty,
                    DocumentType = row.DocumentType ?? string.Empty,
                    DisplayCompositeCode = row.DisplayCompositeCode
                });
            }
        }

        //private string BuildDevFilePath(PartRevisionItemDto row)
        //{
        //    return Path.Combine(
        //        @"E:\Vault\development",
        //        $"{row.DisplayPartNumber}.sldprt"
        //    );
        //}

        private async void ArtifactGeneratorView_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadCustomersAsync();
        }

        private async Task LoadCustomersAsync()
        {
            EcoParts.Clear();

            ArtifactProjectComboBox.ItemsSource = null;
            ArtifactEcoComboBox.ItemsSource = null;

            var customers = await _client.GetCustomersAsync();

            ArtifactCustomerComboBox.DisplayMemberPath = "CustomerName";
            ArtifactCustomerComboBox.SelectedValuePath = "CustomerId";
            ArtifactCustomerComboBox.ItemsSource = customers;
        }

        private async void ArtifactCustomerComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            EcoParts.Clear();

            ArtifactProjectComboBox.ItemsSource = null;
            ArtifactEcoComboBox.ItemsSource = null;

            var customer = ArtifactCustomerComboBox.SelectedItem as CustomerDto;
            if (customer == null)
                return;

            var projects = await _client.GetProjectsByCustomerAsync(customer.CustomerId);

            ArtifactProjectComboBox.DisplayMemberPath = "ProjectName";
            ArtifactProjectComboBox.SelectedValuePath = "ProjectId";
            ArtifactProjectComboBox.ItemsSource = projects;
        }

        private async void ArtifactProjectComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            EcoParts.Clear();

            ArtifactEcoComboBox.ItemsSource = null;

            var project = ArtifactProjectComboBox.SelectedItem as ProjectDto;
            if (project == null)
                return;

            var ecos = await _client.GetEcosByProjectAsync(project.ProjectId);

            ArtifactEcoComboBox.DisplayMemberPath = "EcoNumber";
            ArtifactEcoComboBox.SelectedValuePath = "EcoId";
            ArtifactEcoComboBox.ItemsSource = ecos;
        }

        private async void GenerateOutputsButton_Click(object sender, RoutedEventArgs e)
        {


            if (_selectedEco == null)

            {
                MessageBox.Show("Select an ECO first.", "Artifact Generator");
                return;
            }

            var selectedParts = ArtifactPartsGrid.SelectedItems
                .Cast<ArtifactPartRowViewModel>()
                .ToList();

            if (!selectedParts.Any())
            {
                MessageBox.Show("Select at least one ECO part.", "Artifact Generator");
                return;
            }

            var description = ArtifactBatchDescriptionTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(description))
            {
                MessageBox.Show("Enter a batch description.", "Artifact Generator");
                return;
            }

            var outputs = new List<ArtifactOutputOptionDto>();

            if (StepAp214CheckBox.IsChecked == true)
                outputs.Add(new ArtifactOutputOptionDto("STEP", "AP214"));

            if (StlCheckBox.IsChecked == true)
                outputs.Add(new ArtifactOutputOptionDto("STL", StlQualityComboBox.Text));

            if (NativeCheckBox.IsChecked == true)
                outputs.Add(new ArtifactOutputOptionDto("SW_NATIVE", "Original"));

            if (PdfCheckBox.IsChecked == true)
                outputs.Add(new ArtifactOutputOptionDto("PDF", "DrawingOnly"));

            if (!outputs.Any())
            {
                MessageBox.Show("Select at least one output type.", "Artifact Generator");
                return;
            }

            try
            {
                SetArtifactBusy(true, "Generating artifact batch...");


                var request = new GenerateArtifactBatchRequest(
                EcoId: _selectedEco.EcoId,
                BatchDescription: description,
                CreateZip: CreateZipCheckBox.IsChecked == true,
                ArchivePrevious: ArchivePreviousCheckBox.IsChecked == true,
                RevisionIds: selectedParts.Select(x => x.RevisionId).ToList(),
                Outputs: outputs
            );

                var job = await _client.StartArtifactGenerationJobAsync(request);

                SetArtifactBusy(true, job.Message);


                while (job.Status is "queued" or "processing")
                {
                    await Task.Delay(750);

                    job = await _client.GetArtifactGenerationJobAsync(job.JobId);

                    ArtifactProgressBar.IsIndeterminate = job.TotalSteps <= 0;
                    ArtifactProgressBar.Maximum = Math.Max(job.TotalSteps, 1);
                    ArtifactProgressBar.Value = job.CompletedSteps;

                    ArtifactProgressTextBlock.Text = job.Message;
                }

                if (job.Status == "completed")
                {
                    _lastBatch = job.Result;

                    SetArtifactBusy(false,
                        $"Created {_lastBatch?.BatchCode} with {_lastBatch?.Artifacts.Count ?? 0} artifact(s).");

                    ArtifactResultsGrid.ItemsSource = _lastBatch?.Artifacts;
                    ArtifactResultsGrid.Visibility = Visibility.Visible;

                    OpenOutputFolderButton.IsEnabled = true;

                    // 👇 NEW: conditional auto-open
                    if (AutoOpenFolderCheckBox.IsChecked == true &&
                        _lastBatch != null &&
                        Directory.Exists(_lastBatch.OutputRootPath))
                    {
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = _lastBatch.OutputRootPath,
                            UseShellExecute = true
                        });
                    }
                }
                else
                {
                    SetArtifactBusy(false, "Generation failed.");
                    MessageBox.Show(job.Message, "Artifact Generator");
                }

            }
            catch (Exception ex)
            {
                SetArtifactBusy(false, "Generation failed.");

                MessageBox.Show(
                    ex.Message,
                    "Artifact Generator",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
        private void OpenOutputFolder_Click(object sender, RoutedEventArgs e)
        {
            if (_lastBatch == null || string.IsNullOrWhiteSpace(_lastBatch.OutputRootPath))
            {
                MessageBox.Show("No output folder is available yet.", "Artifact Generator");
                return;
            }

            if (!Directory.Exists(_lastBatch.OutputRootPath))
            {
                MessageBox.Show(
                    "Output folder was not found:\n\n" + _lastBatch.OutputRootPath,
                    "Artifact Generator");
                return;
            }

            Process.Start(new ProcessStartInfo
            {
                FileName = _lastBatch.OutputRootPath,
                UseShellExecute = true
            });
        }
        private void SetArtifactBusy(bool isBusy, string message = "")
        {
            ArtifactProgressBar.Visibility = isBusy
                ? Visibility.Visible
                : Visibility.Collapsed;

            ArtifactProgressTextBlock.Text = message;

            GenerateOutputsButton.IsEnabled = !isBusy;
            ValidateButton.IsEnabled = !isBusy;
        }

        

        private static string SanitizeFileName(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            foreach (var c in Path.GetInvalidFileNameChars())
                value = value.Replace(c, '-');

            // optional trims
            return value.Trim().TrimEnd('.');
        }



    }
}