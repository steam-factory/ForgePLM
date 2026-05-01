using ForgePLM.Administrator.Services;
using ForgePLM.Contracts.Customers;
using ForgePLM.Contracts.Eco;
using ForgePLM.Contracts.Projects;
using ForgePLM.Contracts.Revisions;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;


namespace ForgePLM.Administrator.Views
{
    public partial class ArtifactGeneratorView : UserControl, INavigationView
    {
        public string ViewTitle => "Artifact Generator";
        private readonly ForgePlmAdminApiClient _client = new ForgePlmAdminApiClient();

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

    
        private async void ArtifactEcoComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            EcoParts.Clear();

            var eco = ArtifactEcoComboBox.SelectedItem as EcoDto;
            if (eco == null)
                return;

            var rows = await _client.GetArtifactEcoContentsAsync(eco.EcoId);


            foreach (var row in rows)
            {
                EcoParts.Add(new ArtifactPartRowViewModel
                {
                    IsSelected = false,

                    EcoId = eco.EcoId,   // ← comes from selected ECO
                    PartId = row.PartId,
                    RevisionId = row.RevisionId,

                    PartNumber = row.DisplayPartNumber,
                    Revision = row.RevisionCode.ToString(),
                    Description = row.Description ?? string.Empty,
                    RevisionState = row.RevisionState ?? string.Empty,
                    DocumentType = row.DocumentType ?? string.Empty,
                    DisplayCompositeCode = row.DisplayCompositeCode,
                    //SourceFilePath = row.FilePath ?? string.Empty
                    SourceFilePath = BuildDevFilePath(row)
                });
            }
        }

        private string BuildDevFilePath(PartRevisionItemDto row)
        {
            return Path.Combine(
                @"E:\Vault\development",
                $"{row.DisplayPartNumber}.sldprt"
            );
        }

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
            var selectedParts = ArtifactPartsGrid.SelectedItems
                .Cast<ArtifactPartRowViewModel>()
                .ToList();

            if (!selectedParts.Any())
            {
                MessageBox.Show("Select at least one ECO part.", "Artifact Generator");
                return;
            }

            bool generateStep = StepAp214CheckBox.IsChecked == true;

            if (!generateStep)
            {
                MessageBox.Show("Select at least one output type.", "Artifact Generator");
                return;
            }

            await GenerateStepOutputsAsync(selectedParts);
        }

        private async Task GenerateStepOutputsAsync(List<ArtifactPartRowViewModel> selectedParts)
        {
            try
            {
                var exporter = new SolidWorksArtifactExportService();

                foreach (var part in selectedParts)
                {
                    // temporary path until artifact_batch table exists
                    string outputFolder = Path.Combine(
                        @"E:\ForgePLM\production",
                        "artifact-generator-test");

                    

                    Directory.CreateDirectory(outputFolder);

                    string outputPath = Path.Combine(
                        outputFolder,
                        $"{part.DisplayCompositeCode} {SanitizeFileName(part.Description)}-DRAFT.step");

                    await exporter.ExportStepAp214Async(part.SourceFilePath, outputPath);
                }

                MessageBox.Show("STEP outputs generated.", "Artifact Generator");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Artifact Generator Error");
            }
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