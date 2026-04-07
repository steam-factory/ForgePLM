using ForgePLM.Administrator.Services;
using ForgePLM.Contracts.Customers;
using ForgePLM.Contracts.Eco;
using ForgePLM.Contracts.PartCategories;
using ForgePLM.Contracts.Parts;
using ForgePLM.Contracts.Projects;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;


namespace ForgePLM.Administrator.Views
{
    public partial class EcoBuilderView : UserControl, INavigationView
    {
        private readonly ForgePlmAdminApiClient _apiClient = new();

        private readonly ObservableCollection<CustomerDto> _customers = new();
        private readonly ObservableCollection<ProjectDto> _projects = new();
        private readonly ObservableCollection<EcoDto> _ecos = new();
        private readonly List<PartCategoryDto> _partCategories = new();
        private readonly ObservableCollection<PartRevisionItemDto> _ecoContents = new();
        private readonly ObservableCollection<ProjectPartCurrentDto> _projectParts = new();

        private EcoDto? _selectedEco;

        private CustomerDto? _selectedCustomer;
        private ProjectDto? _selectedProject;

        public string ViewTitle => "ECO Builder";

        public EcoBuilderView()
        {
            InitializeComponent();

            EcoCustomerComboBox.ItemsSource = _customers;
            EcoProjectComboBox.ItemsSource = _projects;
            EcoComboBox.ItemsSource = _ecos;
            NewEcoButton.IsEnabled = false;
            AddNewPartButton.IsEnabled = false;
            EcoContentsListBox.ItemsSource = _ecoContents;
            ProjectPartsListBox.ItemsSource = _projectParts;
            ProjectPartsListBox.IsEnabled = false;

            Loaded += EcoBuilderView_Loaded;
        }

        private async void EcoBuilderView_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadCustomersAsync();
            await LoadPartCategoriesAsync();
        }

        private async Task LoadCustomersAsync()
        {
            try
            {
                var customers = await _apiClient.GetCustomersAsync();

                _customers.Clear();
                foreach (var customer in customers)
                {
                    _customers.Add(customer);
                }

                EcoCustomerComboBox.SelectedItem = null;
                EcoProjectComboBox.SelectedItem = null;
                _projects.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Load Customers Error");
            }
        }

        private void SetEcoStateBadge(string state)
        {
            EcoStateTextBlock.Text = state;

            switch (state)
            {
                case "Development":
                    EcoStateBorder.Background = Brushes.DodgerBlue;
                    EcoStateTextBlock.Foreground = Brushes.White;
                    break;

                case "Staged":
                    EcoStateBorder.Background = Brushes.Orange;
                    EcoStateTextBlock.Foreground = Brushes.Black;
                    break;

                case "Released":
                    EcoStateBorder.Background = Brushes.ForestGreen;
                    EcoStateTextBlock.Foreground = Brushes.White;
                    break;

                case "Cancelled":
                    EcoStateBorder.Background = Brushes.Gray;
                    EcoStateTextBlock.Foreground = Brushes.Black;
                    break;

                default:
                    EcoStateBorder.Background = Brushes.Transparent;
                    EcoStateTextBlock.Foreground = Brushes.Black;
                    break;
            }
        }
        private async Task LoadEcosForSelectedProjectAsync()
        {
            _ecos.Clear();
            _selectedEco = null;
            EcoComboBox.SelectedItem = null;

            RevisionFamilyTextBlock.Text = "";
            EcoStateTextBlock.Text = "";
            EcoStateBorder.Background = Brushes.Transparent;
            EcoStateTextBlock.Foreground = Brushes.Black;

            if (_selectedProject is null)
                return;

            try
            {
                var ecos = await _apiClient.GetEcosByProjectAsync(_selectedProject.ProjectId);

                foreach (var eco in ecos)
                {
                    _ecos.Add(eco);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Load ECOs Error");
            }
        }

        private async void EcoCustomerComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                _selectedCustomer = EcoCustomerComboBox.SelectedItem as CustomerDto;
                _selectedProject = null;

                _projects.Clear();
                EcoProjectComboBox.SelectedItem = null;

                if (_selectedCustomer is null)
                    return;

                var projects = await _apiClient.GetProjectsByCustomerAsync(_selectedCustomer.CustomerId);

                foreach (var project in projects)
                {
                    _projects.Add(project);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Load Projects Error");
            }
        }


        private async void EcoProjectComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedProject = EcoProjectComboBox.SelectedItem as ProjectDto;
            NewEcoButton.IsEnabled = _selectedProject is not null;

            await LoadEcosForSelectedProjectAsync();
            await LoadProjectPartsAsync();
        }

        private void SelectEco(int ecoId)
        {
            var match = _ecos.FirstOrDefault(e => e.EcoId == ecoId);
            if (match is not null)
            {
                EcoComboBox.SelectedItem = match;
            }
        }

        private async void NewEcoButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedCustomer is null)
            {
                MessageBox.Show("Select a customer first.", "New ECO");
                return;
            }

            if (_selectedProject is null)
            {
                MessageBox.Show("Select a project first.", "New ECO");
                return;
            }

            var dialog = new NewEcoDialog(_selectedProject)
            {
                Owner = Window.GetWindow(this)
            };

            var result = dialog.ShowDialog();

            if (result != true)
                return;

            try
            {
                var request = new CreateEcoRequest(
                    ProjectCode: _selectedProject.ProjectCode,
                    EcoTitle: dialog.EcoTitle,
                    EcoDescription: dialog.EcoDescription,
                    ReleaseLevel: dialog.SelectedRevisionFamily
                );

                var createdEco = await _apiClient.CreateEcoAsync(request);

                await LoadEcosForSelectedProjectAsync();
                SelectEco(createdEco.EcoId);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Create ECO Error");
            }
        }

        private async Task LoadPartCategoriesAsync()
        {
            try
            {
                var categories = await _apiClient.GetPartCategoriesAsync();

                _partCategories.Clear();
                _partCategories.AddRange(categories);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Load Part Categories Error");
            }
        }

        private async Task LoadAndSelectEcoAsync(int ecoId)
        {
            await LoadEcosForSelectedProjectAsync();
            SelectEco(ecoId);
            await LoadEcoContentsAsync();
        }
        private async void EcoComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedEco = EcoComboBox.SelectedItem as EcoDto;

            ProjectPartsListBox.IsEnabled = _selectedEco is not null;
            AddNewPartButton.IsEnabled = _selectedEco is not null;

            if (_selectedEco is null)
            {
                RevisionFamilyTextBlock.Text = "";
                SetEcoStateBadge("");
                _ecoContents.Clear();
                return;
            }

            RevisionFamilyTextBlock.Text = _selectedEco.ReleaseLevel.ToString();
            SetEcoStateBadge(_selectedEco.EcoState);

            await LoadEcoContentsAsync();
        }

        private async Task LoadEcoContentsAsync()
        {
            _ecoContents.Clear();

            if (_selectedEco is null)
                return;

            try
            {
                var items = await _apiClient.GetEcoContentsAsync(_selectedEco.EcoId);

                foreach (var item in items)
                    _ecoContents.Add(item);

                RefreshProjectPartAvailability();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Load ECO Contents Error");
            }
        }

        private async Task LoadProjectPartsAsync()
        {
            _projectParts.Clear();

            if (_selectedProject is null)
                return;

            try
            {
                var parts = await _apiClient.GetProjectPartsCurrentAsync(_selectedProject.ProjectId);

                foreach (var part in parts)
                    _projectParts.Add(part);

                RefreshProjectPartAvailability();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Load Project Parts Error");
            }
        }

        private void RefreshProjectPartAvailability()
        {
            if (_projectParts.Count == 0)
                return;

            var ecoContentsPartIds = _ecoContents
                .Select(x => x.PartId)
                .ToHashSet();

            var updated = _projectParts
                .Select(part =>
                {
                    bool canSelect = false;
                    string? reason = null;

                    if (_selectedEco is null)
                    {
                        canSelect = false;
                        reason = "No active ECO selected.";
                    }
                    else if (ecoContentsPartIds.Contains(part.PartId))
                    {
                        canSelect = false;
                        reason = "Already in selected ECO.";
                    }
                    else if (!string.Equals(part.RevisionState, "Released", StringComparison.OrdinalIgnoreCase))
                    {
                        canSelect = false;
                        reason = "Current revision is not Released.";
                    }
                    else if (part.RevisionFamily > _selectedEco.ReleaseLevel)
                    {
                        canSelect = false;
                        reason = "Revision family exceeds selected ECO family.";
                    }
                    else
                    {
                        canSelect = true;
                        reason = null;
                    }

                    return part with
                    {
                        CanSelect = canSelect,
                        AvailabilityReason = reason
                    };
                })
                .ToList();

            _projectParts.Clear();

            foreach (var item in updated)
                _projectParts.Add(item);
        }

        private void ProjectPartsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach (var added in e.AddedItems.OfType<ProjectPartCurrentDto>().ToList())
            {
                if (!added.CanSelect)
                {
                    ProjectPartsListBox.SelectedItems.Remove(added);
                }
            }
        }
        private async void AddNewPartButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedCustomer is null)
            {
                MessageBox.Show("Select a customer first.", "New Part");
                return;
            }

            if (_selectedProject is null)
            {
                MessageBox.Show("Select a project first.", "New Part");
                return;
            }

            if (_selectedEco is null)
            {
                MessageBox.Show("Select an ECO first.", "New Part");
                return;
            }

            if (_partCategories.Count == 0)
            {
                MessageBox.Show("No part categories were loaded.", "New Part");
                return;
            }

            var dialog = new NewPartDialog(
                _selectedCustomer,
                _selectedProject,
                _selectedEco,
                _partCategories)
            {
                Owner = Window.GetWindow(this)
            };

            var result = dialog.ShowDialog();

            if (result != true)
                return;

            try
            {
                var request = new ForgePLM.Contracts.Parts.CreatePartRequest(
                    ProjectCode: _selectedProject.ProjectCode,
                    EcoNumber: _selectedEco.EcoNumber,
                    CategoryCode: dialog.SelectedCategoryCode!,
                    Description: dialog.Description!
                );

                var created = await _apiClient.CreatePartUnderEcoAsync(request);

                await LoadEcoContentsAsync();
                await LoadProjectPartsAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Create Part Error");
            }
        }


    }
}