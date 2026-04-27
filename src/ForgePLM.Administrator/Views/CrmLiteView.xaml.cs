using ForgePLM.Administrator.Services;
using ForgePLM.Contracts.Customers;
using ForgePLM.Contracts.Projects;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using static ForgePLM.Administrator.Views.CrmLiteView;

namespace ForgePLM.Administrator.Views
{

    
    public partial class CrmLiteView : UserControl, INavigationView
    {
        private readonly ForgePlmAdminApiClient _apiClient = new();

        public string ViewTitle => "CRM Lite";
        private readonly ObservableCollection<CustomerDto> _customers = new();
        private CustomerDto? _selectedCustomer;
        private readonly ObservableCollection<ProjectDto> _projects = new();
        private ProjectDto? _selectedProject;

        

        public CrmLiteView()
        {
            InitializeComponent();
            Loaded += CrmLiteView_Loaded;

            ProjectsListBox.ItemsSource = _projects;
            CustomersListBox.ItemsSource = _customers;
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

                CustomerIdText.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Load Customers Error");
            }
        }

        private async void CrmLiteView_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadCustomersAsync();

            ClearProjectForm();
            SetProjectEditorEnabled(false);

            UpdateCustomerModeUI();
            UpdateProjectModeUI();
        }

       

        private async void CustomersListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CustomersListBox.SelectedItem is not CustomerDto customer)
                return;

            _selectedCustomer = customer;

            CustomerIdText.Text = $"ID: {customer.CustomerId}";
            CustomerIdText.Visibility = Visibility.Visible;
            CustomerIdValue.Text = customer.CustomerId.ToString();
            CustomerCodeTextBox.Text = customer.CustomerCode;
            CustomerNameTextBox.Text = customer.CustomerName;
            ContactNameTextBox.Text = customer.ContactName ?? "";
            ContactEmailTextBox.Text = customer.ContactEmail ?? "";
            ContactPhoneTextBox.Text = customer.ContactPhone ?? "";
            CustomerIsActiveCheckBox.IsChecked = customer.IsActive;
            CustomerIsActiveCheckBox.IsEnabled = true;

            UpdateCustomerModeUI();

            await LoadProjectsForSelectedCustomerAsync();
        }

        private async Task LoadProjectsForSelectedCustomerAsync()
        {
            _projects.Clear();
            _selectedProject = null;
            ClearProjectForm();

            if (_selectedCustomer is null)
            {
                SetProjectEditorEnabled(false);
                return;
            }

            SetProjectEditorEnabled(true);
            ProjectCustomerTextBox.Text = _selectedCustomer.CustomerName;

            try
            {
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

            UpdateProjectModeUI();
        }

        private void ProjectsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedProject = ProjectsListBox.SelectedItem as ProjectDto;
            PopulateProjectForm(_selectedProject);
            UpdateProjectModeUI();
        }

        private async void SaveCustomerButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(CustomerCodeTextBox.Text))
                {
                    MessageBox.Show("Customer Code is required.");
                    return;
                }

                if (string.IsNullOrWhiteSpace(CustomerNameTextBox.Text))
                {
                    MessageBox.Show("Customer Name is required.");
                    return;
                }

                var newIsActive = CustomerIsActiveCheckBox.IsChecked ?? true;

                if (_selectedCustomer is not null &&
                    _selectedCustomer.IsActive &&
                    !newIsActive)
                {
                    var confirm = MessageBox.Show(
                        "Deactivating this customer will hide it from the default list and prevent it from being used in normal workflows.\n\nYou can reactivate it later from an inactive/customers view.\n\nContinue?",
                        "Deactivate Customer",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Warning);

                    if (confirm != MessageBoxResult.Yes)
                        return;
                }

                var customer = new CustomerDto(
                    CustomerId: _selectedCustomer?.CustomerId ?? 0,
                    CustomerCode: CustomerCodeTextBox.Text.Trim(),
                    CustomerName: CustomerNameTextBox.Text.Trim(),
                    ContactName: string.IsNullOrWhiteSpace(ContactNameTextBox.Text) ? null : ContactNameTextBox.Text.Trim(),
                    ContactEmail: string.IsNullOrWhiteSpace(ContactEmailTextBox.Text) ? null : ContactEmailTextBox.Text.Trim(),
                    ContactPhone: string.IsNullOrWhiteSpace(ContactPhoneTextBox.Text) ? null : ContactPhoneTextBox.Text.Trim(),
                    IsActive: newIsActive,
                    CreatedAt: _selectedCustomer?.CreatedAt ?? DateTime.MinValue
                );

                if (_selectedCustomer is null)
                {
                    await _apiClient.CreateCustomerAsync(customer);
                }
                else
                {
                    await _apiClient.UpdateCustomerAsync(customer);
                    MessageBox.Show(newIsActive ? "Customer updated." : "Customer deactivated.", "Success");
                }

                await LoadCustomersAsync();

                CustomersListBox.SelectedItem = null;
                _selectedCustomer = null;

                ClearCustomerForm();

                CustomerIdText.Text = "";
                CustomerIdText.Visibility = Visibility.Collapsed;

                UpdateCustomerModeUI();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Save Customer Error");
            }
        }

        private void NewCustomerButton_Click(object sender, RoutedEventArgs e)
        {
            CustomersListBox.SelectedItem = null;
            _selectedCustomer = null;

            ClearCustomerForm();

            CustomerIdText.Text = "";
            CustomerIdText.Visibility = Visibility.Collapsed;
            CustomerIsActiveCheckBox.IsChecked = true;
            CustomerIsActiveCheckBox.IsEnabled = false;

            UpdateCustomerModeUI(); // 🔥
        }
        private void ClearCustomerForm()
        {
            CustomerCodeTextBox.Text = "";
            CustomerNameTextBox.Text = "";
            ContactNameTextBox.Text = "";
            ContactEmailTextBox.Text = "";
            ContactPhoneTextBox.Text = "";
            CustomerIdValue.Text = "";
            CustomerIsActiveCheckBox.IsChecked = true;
        }

        private void UpdateCustomerModeUI()
        {
            SaveCustomerButton.Style = (Style)FindResource(
                _selectedCustomer == null ? "PrimaryButtonStyle" : "AccentButtonStyle"
            );
            if (_selectedCustomer == null)
            {
                SaveCustomerButton.Content = "Save Customer";
            }
            else
            {
                SaveCustomerButton.Content = "Update Customer";
            }
        }

        private void NewProjectButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedCustomer is null)
            {
                MessageBox.Show("Select a customer first.");
                return;
            }

            ProjectsListBox.SelectedItem = null;
            _selectedProject = null;

            ClearProjectForm();
            ProjectCustomerTextBox.Text = _selectedCustomer.CustomerName;

            UpdateProjectModeUI();
        }

        private void PopulateProjectForm(ProjectDto? project)
        {
            if (project is null)
            {
                ClearProjectForm();
                return;
            }

            ProjectCodeTextBox.Text = project.ProjectCode;
            ProjectNameTextBox.Text = project.ProjectName;
            ProjectCustomerTextBox.Text = _selectedCustomer?.CustomerName ?? "";
            ProjectIsActiveCheckBox.IsChecked = project.IsActive;
        }

        private void ClearProjectForm()
        {
            ProjectCodeTextBox.Text = "";
            ProjectNameTextBox.Text = "";
            ProjectCustomerTextBox.Text = _selectedCustomer?.CustomerName ?? "";
            ProjectIsActiveCheckBox.IsChecked = false;
        }

        private async void SaveProjectButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_selectedCustomer is null)
                {
                    MessageBox.Show("Select a customer first.");
                    return;
                }

                if (string.IsNullOrWhiteSpace(ProjectCodeTextBox.Text))
                {
                    MessageBox.Show("Project Code is required.");
                    return;
                }

                if (string.IsNullOrWhiteSpace(ProjectNameTextBox.Text))
                {
                    MessageBox.Show("Project Name is required.");
                    return;
                }

                ProjectDto savedProject;

                if (_selectedProject is null)
                {
                    var createRequest = new CreateProjectRequest(
                        CustomerId: _selectedCustomer.CustomerId,
                        ProjectCode: ProjectCodeTextBox.Text.Trim(),
                        ProjectName: ProjectNameTextBox.Text.Trim(),
                        ProjectDescription: null
                    );

                    savedProject = await _apiClient.CreateProjectAsync(createRequest);
                    //MessageBox.Show("Project created.", "Success");
                }
                else
                {
                    var updateRequest = new UpdateProjectRequest(
                        ProjectName: ProjectNameTextBox.Text.Trim(),
                        IsActive: ProjectIsActiveCheckBox.IsChecked ?? true
                    );

                    savedProject = await _apiClient.UpdateProjectAsync(_selectedProject.ProjectId, updateRequest);
                    MessageBox.Show("Project updated.", "Success");
                }

                await LoadProjectsForSelectedCustomerAsync();
                SelectProject(savedProject.ProjectId);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Save Project Error");
            }
        }

        private static string GenerateCustomerCode(string? customerName)
        {
            if (string.IsNullOrWhiteSpace(customerName))
                return string.Empty;

            var sb = new StringBuilder();

            foreach (var ch in customerName.ToUpperInvariant())
            {
                if (char.IsLetterOrDigit(ch))
                {
                    sb.Append(ch);

                    if (sb.Length == 5)
                        break;
                }
            }

            return sb.ToString().PadRight(5, '0');
        }
        private void UpdateProjectCodePreview()
        {
            if (_selectedCustomer is null)
            {
                ProjectCodeTextBox.Text = string.Empty;
                return;
            }

            if (_selectedProject is not null)
            {
                ProjectCodeTextBox.Text = _selectedProject.ProjectCode;
                return;
            }

            // Placeholder preview until backend returns the real sequence
            ProjectCodeTextBox.Text = $"{_selectedCustomer.CustomerCode}-#####";
        }



        private void CustomerNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_selectedCustomer is not null)
                return;

            CustomerCodeTextBox.Text = GenerateCustomerCode(CustomerNameTextBox.Text);
        }

        private void ProjectNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_selectedProject is not null)
                return;

            UpdateProjectCodePreview();
        }




        private void SelectProject(int projectId)
        {
            var match = _projects.FirstOrDefault(p => p.ProjectId == projectId);
            if (match is not null)
            {
                ProjectsListBox.SelectedItem = match;
            }
        }

        private void UpdateProjectModeUI()
        {
            SaveProjectButton.Style = (Style)FindResource(
                _selectedProject == null ? "PrimaryButtonStyle" : "AccentButtonStyle"
            );

            SaveProjectButton.Content = _selectedProject == null
                ? "Save Project"
                : "Update Project";
        }

        private void SetProjectEditorEnabled(bool isEnabled)
        {
            NewProjectButton.IsEnabled = isEnabled;
            ProjectsListBox.IsEnabled = isEnabled;
            ProjectCodeTextBox.IsEnabled = isEnabled;
            ProjectNameTextBox.IsEnabled = isEnabled;
            ProjectIsActiveCheckBox.IsEnabled = isEnabled;
            SaveProjectButton.IsEnabled = isEnabled;

            ProjectCustomerTextBox.IsEnabled = false;
        }

        
    }
}