using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using ForgePLM.Administrator.Services;
using ForgePLM.Contracts.Customers;

namespace ForgePLM.Administrator.Views
{
    public partial class CrmLiteView : UserControl, INavigationView
    {
        private readonly ForgePlmAdminApiClient _apiClient = new();

        public string ViewTitle => "CRM Lite";
        private CustomerDto? _selectedCustomer;



        public CrmLiteView()
        {
            InitializeComponent();
            Loaded += CrmLiteView_Loaded;
        }

        private async void CrmLiteView_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadCustomersAsync();
            UpdateCustomerModeUI();
        }

        private async Task LoadCustomersAsync()
        {
            try
            {
                var customers = await _apiClient.GetCustomersAsync();
                CustomersListBox.ItemsSource = customers;
                CustomerIdText.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Load Customers Error");
            }
        }

        private void CustomersListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
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

            UpdateCustomerModeUI(); // 🔥
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
                    MessageBox.Show("Customer created.", "Success");
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
    }
}