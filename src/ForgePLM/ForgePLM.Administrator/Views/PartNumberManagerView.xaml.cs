using ForgePLM.Administrator.Services;
using ForgePLM.Contracts.Parts;
using ForgePLM.Contracts.Requests;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ForgePLM.Administrator.Views
{
    public partial class PartNumberManagerView : UserControl, INavigationView
    {
        private readonly ForgePlmAdminApiClient _apiClient = new();
        private readonly ObservableCollection<PartNumberManagerItemDto> _rows = new();

        private List<PartNumberManagerItemDto> _allRows = new();
        private readonly HashSet<int> _dirtyRevisionIds = new();

        public string ViewTitle => "Part Number Manager";

        public PartNumberManagerView()
        {
            InitializeComponent();

            PartNumbersDataGrid.ItemsSource = _rows;

            Loaded += PartNumberManagerView_Loaded;
            RefreshButton.Click += RefreshButton_Click;
            SaveDescriptionsButton.Click += SaveDescriptionsButton_Click;

            SearchTextBox.TextChanged += FilterChanged;
            CustomerFilterComboBox.SelectionChanged += FilterChanged;
            ProjectFilterComboBox.SelectionChanged += FilterChanged;
            StateFilterComboBox.SelectionChanged += FilterChanged;
            DocumentTypeFilterComboBox.SelectionChanged += FilterChanged;
            CurrentOnlyCheckBox.Checked += FilterChanged;
            CurrentOnlyCheckBox.Unchecked += FilterChanged;
            EditableOnlyCheckBox.Checked += FilterChanged;
            EditableOnlyCheckBox.Unchecked += FilterChanged;

            PartNumbersDataGrid.CellEditEnding += PartNumbersDataGrid_CellEditEnding;
            PartNumbersDataGrid.BeginningEdit += PartNumbersDataGrid_BeginningEdit;
        }

        private async void PartNumberManagerView_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadDataAsync();
        }

        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            await LoadDataAsync();
        }

        private async void SaveDescriptionsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dirtyRows = _allRows
                    .Where(x => _dirtyRevisionIds.Contains(x.RevisionId))
                    .ToList();

                foreach (var row in dirtyRows)
                {
                    await _apiClient.UpdateRevisionDescriptionAsync(
                        row.RevisionId,
                        new UpdateRevisionDescriptionRequest(row.Description));
                }

                _dirtyRevisionIds.Clear();
                await LoadDataAsync();

                MessageBox.Show("Description changes saved.", "Part Number Manager");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Save Description Error");
            }
        }

        private void PartNumbersDataGrid_BeginningEdit(object? sender, DataGridBeginningEditEventArgs e)
        {
            if (e.Row.Item is not PartNumberManagerItemDto row)
                return;

            if (e.Column.Header?.ToString() != "Description")
            {
                e.Cancel = true;
                return;
            }

            if (!row.CanEditDescription)
            {
                e.Cancel = true;
            }
        }
        private async System.Threading.Tasks.Task LoadDataAsync()
        {
            try
            {
                var items = await _apiClient.GetPartNumberManagerItemsAsync();

                _allRows = items.ToList();
                _dirtyRevisionIds.Clear();

                LoadFilters();
                ApplyFilters();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Load Part Number Manager Error");
            }
        }

        private void LoadFilters()
        {
            var customerOptions = new[] { "All" }
                .Concat(_allRows.Select(x => x.CustomerCode).Distinct().OrderBy(x => x))
                .ToList();

            var projectOptions = new[] { "All" }
                .Concat(_allRows.Select(x => x.ProjectCode).Distinct().OrderBy(x => x))
                .ToList();

            var stateOptions = new[] { "All" }
                .Concat(_allRows.Select(x => x.RevisionState).Distinct().OrderBy(x => x))
                .ToList();

            var docTypeOptions = new[] { "All" }
                .Concat(_allRows.Select(x => x.DocumentType).Distinct().OrderBy(x => x))
                .ToList();

            CustomerFilterComboBox.ItemsSource = customerOptions;
            ProjectFilterComboBox.ItemsSource = projectOptions;
            StateFilterComboBox.ItemsSource = stateOptions;
            DocumentTypeFilterComboBox.ItemsSource = docTypeOptions;

            if (CustomerFilterComboBox.SelectedItem is null) CustomerFilterComboBox.SelectedIndex = 0;
            if (ProjectFilterComboBox.SelectedItem is null) ProjectFilterComboBox.SelectedIndex = 0;
            if (StateFilterComboBox.SelectedItem is null) StateFilterComboBox.SelectedIndex = 0;
            if (DocumentTypeFilterComboBox.SelectedItem is null) DocumentTypeFilterComboBox.SelectedIndex = 0;
        }

        private void ApplyFilters()
        {
            IEnumerable<PartNumberManagerItemDto> filtered = _allRows;

            string search = SearchTextBox.Text?.Trim() ?? string.Empty;
            string customer = CustomerFilterComboBox.SelectedItem as string ?? "All";
            string project = ProjectFilterComboBox.SelectedItem as string ?? "All";
            string state = StateFilterComboBox.SelectedItem as string ?? "All";
            string docType = DocumentTypeFilterComboBox.SelectedItem as string ?? "All";

            if (!string.IsNullOrWhiteSpace(search))
            {
                filtered = filtered.Where(x =>
                    x.PartNumber.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    x.CompositeCode.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    x.Description.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    x.EcoNumber.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    x.RevisionCode.Contains(search, StringComparison.OrdinalIgnoreCase));
            }

            if (customer != "All")
                filtered = filtered.Where(x => x.CustomerCode == customer);

            if (project != "All")
                filtered = filtered.Where(x => x.ProjectCode == project);

            if (state != "All")
                filtered = filtered.Where(x => x.RevisionState == state);

            if (docType != "All")
                filtered = filtered.Where(x => x.DocumentType == docType);

            if (EditableOnlyCheckBox.IsChecked == true)
                filtered = filtered.Where(x => x.CanEditDescription);

            _rows.Clear();
            foreach (var item in filtered)
                _rows.Add(item);

            RecordCountTextBlock.Text = $"{_rows.Count} records";
        }

        private void FilterChanged(object? sender, EventArgs e)
        {
            ApplyFilters();
        }

        private void PartNumbersDataGrid_CellEditEnding(object? sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.Row.Item is not PartNumberManagerItemDto row)
                return;

            if (!row.CanEditDescription)
            {
                MessageBox.Show("Description can only be edited for Development or Staged revisions.", "Part Number Manager");
                return;
            }

            if (e.Column.Header?.ToString() != "Description")
                return;

            if (e.EditingElement is TextBox textBox)
            {
                var updated = row with { Description = textBox.Text ?? string.Empty };

                int indexAll = _allRows.FindIndex(x => x.RevisionId == row.RevisionId);
                if (indexAll >= 0)
                    _allRows[indexAll] = updated;

                int indexRows = _rows.IndexOf(row);
                if (indexRows >= 0)
                    _rows[indexRows] = updated;

                _dirtyRevisionIds.Add(updated.RevisionId);
            }
        }
    }
}