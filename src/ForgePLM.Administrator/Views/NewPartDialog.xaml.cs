using ForgePLM.Contracts.Customers;
using ForgePLM.Contracts.Eco;
using ForgePLM.Contracts.PartCategories;
using ForgePLM.Contracts.Projects;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ForgePLM.Administrator.Views
{
    public partial class NewPartDialog : Window
    {
        public CustomerDto SelectedCustomer { get; }
        public ProjectDto SelectedProject { get; }
        public EcoDto SelectedEco { get; }
        public IReadOnlyList<PartCategoryDto> Categories { get; }

        public string? Description =>
            string.IsNullOrWhiteSpace(DescriptionTextBox.Text)
                ? null
                : DescriptionTextBox.Text.Trim();

        public string? SelectedCategoryCode =>
            CategoryCodeComboBox.SelectedValue?.ToString();
        public string? DocumentType =>
            (DocumentTypeComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString();

        public NewPartDialog(
            CustomerDto selectedCustomer,
            ProjectDto selectedProject,
            EcoDto selectedEco,
            IReadOnlyList<PartCategoryDto> categories)
        {
            InitializeComponent();

            SelectedCustomer = selectedCustomer;
            SelectedProject = selectedProject;
            SelectedEco = selectedEco;
            Categories = categories;

            Loaded += NewPartDialog_Loaded;
        }

        private void NewPartDialog_Loaded(object sender, RoutedEventArgs e)
        {
            PartNumberTextBox.Text = "#######";
            CustomerDisplayTextBox.Text = $"{SelectedCustomer.CustomerCode} | {SelectedCustomer.CustomerName}";
            ProjectDisplayTextBox.Text = $"{SelectedProject.ProjectCode} | {SelectedProject.ProjectName}";
            EcoDisplayTextBox.Text = $"{SelectedEco.EcoNumber} | {SelectedEco.EcoTitle}";
            RevisionFamilyTextBlock.Text = SelectedEco.ReleaseLevel.ToString();

            CategoryCodeComboBox.ItemsSource = Categories;
            CategoryCodeComboBox.SelectedValuePath = "CategoryCode";
            CategoryCodeComboBox.DisplayMemberPath = "CategoryCode";

            if (Categories.Count == 1)
                CategoryCodeComboBox.SelectedIndex = 0;

            DescriptionTextBox.Focus();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void CreatePartButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(DescriptionTextBox.Text))
            {
                MessageBox.Show("Description is required.", "Validation");
                DescriptionTextBox.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(SelectedCategoryCode))
            {
                MessageBox.Show("Category is required.", "Validation");
                CategoryCodeComboBox.Focus();
                return;
            }

            DialogResult = true;
            Close();
        }
    }
}