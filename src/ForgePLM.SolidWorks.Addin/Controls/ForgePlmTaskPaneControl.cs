using ForgePLM.Contracts.Dtos;
using ForgePLM.Contracts.Requests;
using ForgePLM.SolidWorks.Addin.Services;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ForgePLM.SolidWorks.Addin
{
    [ComVisible(true)]
    [Guid("A7E8C2D1-5F3B-4D91-9A6F-2C4D8B7E2201")]
    [ProgId("ForgePLM.SolidWorks.Addin.ForgePlmTaskPaneControl")]
    public class ForgePlmTaskPaneControl : UserControl
    {
        private SldWorks _swApp;

        private const string PropGuid = "PLM_GUID";
        private const string PropPartId = "PLM_PartId";
        private const string PropRevisionId = "PLM_RevisionId";
        private const string PropPartNumber = "PLM_PartNumber";
        private const string PropRevision = "PLM_Revision";
        private const string PropDescription = "PLM_Description";
        private const string PropEco = "PLM_ECO";
        private const string PropProject = "PLM_Project";

        private readonly ForgePlmApiClient _api = new ForgePlmApiClient();

        private bool _isLoadingContext;

        private List<CustomerDto> _customers = new List<CustomerDto>();
        private List<ProjectDto> _projects = new List<ProjectDto>();
        private List<EcoDto> _ecos = new List<EcoDto>();
        private List<EcoRowViewModel> _allEcoRows = new List<EcoRowViewModel>();

        private BindingList<EcoRowViewModel> _ecoRows = new BindingList<EcoRowViewModel>();

        // Layout
        private TableLayoutPanel _rootLayout;

        // Context
        private GroupBox _contextGroup;
        private TableLayoutPanel _contextLayout;
        private ComboBox _cmbCustomer;
        private ComboBox _cmbProject;
        private ComboBox _cmbEco;

        // ECO contents
        private GroupBox _ecoGroup;
        private TableLayoutPanel _ecoLayout;
        private FlowLayoutPanel _filterPanel;
        private ComboBox _cmbCategoryFilter;
        private ComboBox _cmbPartSort;
        private ComboBox _cmbRevSort;
        private Label _lblCategory;
        private Label _lblPartSort;
        private Label _lblRevSort;
        private DataGridView _gridEcoContents;

        // Active file
        private GroupBox _activeFileGroup;
        private TableLayoutPanel _activeFileLayout;
        private Label _lblActiveFileValue;
        private Label _lblActiveStateValue;
        private Label _lblActivePartValue;
        private Label _lblActiveProjectValue;
        private Label _lblActiveRevValue;
        private Label _lblActiveNoteValue;

        public ForgePlmTaskPaneControl()
        {
            BuildUi();
            WireEvents();

            Load += async (_, __) => await LoadInitialContextAsync();

            RefreshActiveFilePanel(null);
        }

        public void Initialize(SldWorks swApp)
        {
            _swApp = swApp;
        }

        public void OnActiveDocumentChanged()
        {
            EvaluateActiveDocument();
        }

        private void BuildUi()
        {
            Dock = DockStyle.Fill;
            Controls.Clear();

            _rootLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                Padding = new Padding(8)
            };

            _rootLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            _rootLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            _rootLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            _rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 210));

            BuildContextSection();
            BuildEcoSection();
            BuildActiveFileSection();

            _rootLayout.Controls.Add(_contextGroup, 0, 0);
            _rootLayout.Controls.Add(_ecoGroup, 0, 1);
            _rootLayout.Controls.Add(_activeFileGroup, 0, 2);

            Controls.Add(_rootLayout);
        }

        private void BuildContextSection()
        {
            _contextGroup = new GroupBox
            {
                Text = "Context",
                Dock = DockStyle.Top,
                AutoSize = true
            };

            _contextLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                ColumnCount = 2,
                RowCount = 3,
                Padding = new Padding(8),
                AutoSize = true
            };

            _contextLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 70));
            _contextLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            _cmbCustomer = new ComboBox
            {
                Dock = DockStyle.Top,
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            _cmbProject = new ComboBox
            {
                Dock = DockStyle.Top,
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            _cmbEco = new ComboBox
            {
                Dock = DockStyle.Top,
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            _contextLayout.Controls.Add(new Label { Text = "Customer", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 0);
            _contextLayout.Controls.Add(_cmbCustomer, 1, 0);

            _contextLayout.Controls.Add(new Label { Text = "Project", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 1);
            _contextLayout.Controls.Add(_cmbProject, 1, 1);

            _contextLayout.Controls.Add(new Label { Text = "ECO", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 2);
            _contextLayout.Controls.Add(_cmbEco, 1, 2);

            _contextGroup.Controls.Add(_contextLayout);
        }

        private void BuildEcoSection()
        {
            _ecoGroup = new GroupBox
            {
                Text = "ECO Contents",
                Dock = DockStyle.Fill
            };

            _ecoLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                Padding = new Padding(8)
            };

            _ecoLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            _ecoLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            _ecoLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            _filterPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                WrapContents = true,
                FlowDirection = FlowDirection.LeftToRight,
                Margin = new Padding(0, 0, 0, 8)
            };

            _lblCategory = new Label
            {
                Text = "Category",
                AutoSize = true,
                Margin = new Padding(0, 8, 6, 0)
            };

            _cmbCategoryFilter = new ComboBox
            {
                Width = 90,
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            _lblPartSort = new Label
            {
                Text = "Part",
                AutoSize = true,
                Margin = new Padding(12, 8, 6, 0)
            };

            _cmbPartSort = new ComboBox
            {
                Width = 90,
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            _lblRevSort = new Label
            {
                Text = "Rev",
                AutoSize = true,
                Margin = new Padding(12, 8, 6, 0)
            };

            _cmbRevSort = new ComboBox
            {
                Width = 90,
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            _filterPanel.Controls.Add(_lblCategory);
            _filterPanel.Controls.Add(_cmbCategoryFilter);
            _filterPanel.Controls.Add(_lblPartSort);
            _filterPanel.Controls.Add(_cmbPartSort);
            _filterPanel.Controls.Add(_lblRevSort);
            _filterPanel.Controls.Add(_cmbRevSort);

            _gridEcoContents = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoGenerateColumns = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                MultiSelect = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                ReadOnly = true,
                RowHeadersVisible = false,
                BackgroundColor = SystemColors.Window,
                BorderStyle = BorderStyle.FixedSingle,
                ShowCellToolTips = true
            };

            BuildEcoGridColumns();

            _ecoLayout.Controls.Add(_filterPanel, 0, 0);
            _ecoLayout.Controls.Add(_gridEcoContents, 0, 1);

            _ecoGroup.Controls.Add(_ecoLayout);
        }

        private void BuildEcoGridColumns()
        {
            _gridEcoContents.Columns.Clear();

            _gridEcoContents.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Part",
                HeaderText = "Part",
                DataPropertyName = "DisplayPartNumber",
                Width = 110
            });

            _gridEcoContents.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Rev",
                HeaderText = "Rev",
                DataPropertyName = "RevisionCode",
                Width = 50
            });

            _gridEcoContents.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Description",
                HeaderText = "Description",
                DataPropertyName = "Description",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });

            _gridEcoContents.Columns.Add(new DataGridViewButtonColumn
            {
                Name = "Open",
                HeaderText = "",
                Text = "Open",
                UseColumnTextForButtonValue = true,
                Width = 32
            });

            _gridEcoContents.Columns.Add(new DataGridViewButtonColumn
            {
                Name = "New",
                HeaderText = "",
                Text = "New",
                UseColumnTextForButtonValue = true,
                Width = 32
            });

            _gridEcoContents.Columns.Add(new DataGridViewButtonColumn
            {
                Name = "Assign",
                HeaderText = "",
                Text = "Assign",
                UseColumnTextForButtonValue = true,
                Width = 32
            });
        }

        private void BuildActiveFileSection()
        {
            _activeFileGroup = new GroupBox
            {
                Text = "Active File",
                Dock = DockStyle.Fill
            };

            _activeFileLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 6,
                Padding = new Padding(8)
            };

            _activeFileLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 72));
            _activeFileLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            _activeFileLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            _activeFileLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            _activeFileLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            _activeFileLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            _activeFileLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            _lblActiveFileValue = new Label { AutoSize = true };
            _lblActiveStateValue = new Label { AutoSize = true };
            _lblActivePartValue = new Label { AutoSize = true };
            _lblActiveRevValue = new Label { AutoSize = true };
            _lblActiveProjectValue = new Label { AutoSize = true };
            _lblActiveNoteValue = new Label
            {
                AutoSize = true,
                ForeColor = Color.DimGray,
                MaximumSize = new Size(0, 0)
            };

            _activeFileLayout.Controls.Add(new Label { Text = "File", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 0);
            _activeFileLayout.Controls.Add(_lblActiveFileValue, 1, 0);

            _activeFileLayout.Controls.Add(new Label { Text = "State", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 1);
            _activeFileLayout.Controls.Add(_lblActiveStateValue, 1, 1);

            _activeFileLayout.Controls.Add(new Label { Text = "Part", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 2);
            _activeFileLayout.Controls.Add(_lblActivePartValue, 1, 2);

            _activeFileLayout.Controls.Add(new Label { Text = "Rev", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 3);
            _activeFileLayout.Controls.Add(_lblActiveRevValue, 1, 3);

            _activeFileLayout.Controls.Add(new Label { Text = "Project", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 4);
            _activeFileLayout.Controls.Add(_lblActiveProjectValue, 1, 4);

            _activeFileLayout.Controls.Add(new Label { Text = "Note", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 4);
            _activeFileLayout.Controls.Add(_lblActiveNoteValue, 1, 4);

            _activeFileGroup.Controls.Add(_activeFileLayout);
        }

        private void WireEvents()
        {
            _cmbCustomer.SelectedIndexChanged += async (_, __) =>
            {
                if (_isLoadingContext) return;
                await LoadProjectsForSelectedCustomerAsync();
            };

            _cmbProject.SelectedIndexChanged += async (_, __) =>
            {
                if (_isLoadingContext) return;
                await LoadEcosForSelectedProjectAsync();
            };

            _cmbEco.SelectedIndexChanged += async (_, __) =>
            {
                if (_isLoadingContext) return;
                await LoadEcoContentsForSelectedEcoAsync();
            };

            _cmbCategoryFilter.SelectedIndexChanged += (_, __) => ApplyGridViewState();
            _cmbPartSort.SelectedIndexChanged += (_, __) => ApplyGridViewState();
            _cmbRevSort.SelectedIndexChanged += (_, __) => ApplyGridViewState();

            _gridEcoContents.CellContentClick += GridEcoContents_CellContentClick;
            _gridEcoContents.CellFormatting += GridEcoContents_CellFormatting;
            _gridEcoContents.CellToolTipTextNeeded += GridEcoContents_CellToolTipTextNeeded;
            _gridEcoContents.SelectionChanged += (_, __) => EvaluateActiveDocument();
        }

        private async Task LoadInitialContextAsync()
        {
            try
            {
                //MessageBox.Show("LoadInitialContextAsync fired.", "ForgePLM");
                _isLoadingContext = true;

                _cmbCustomer.Items.Clear();
                _cmbProject.Items.Clear();
                _cmbEco.Items.Clear();

                _customers = await _api.GetCustomersAsync();

                foreach (var customer in _customers)
                {
                    _cmbCustomer.Items.Add($"{customer.CustomerCode} - {customer.CustomerName}");
                }

                //MessageBox.Show($"Customers loaded: {_customers.Count}", "ForgePLM");

                if (_cmbCustomer.Items.Count > 0)
                {
                    _cmbCustomer.SelectedIndex = 0;
                }

                _cmbCategoryFilter.Items.Clear();
                _cmbCategoryFilter.Items.Add("All");
                _cmbCategoryFilter.SelectedIndex = 0;

                _cmbPartSort.Items.Clear();
                _cmbPartSort.Items.AddRange(new object[] { "Ascending", "Descending" });
                _cmbPartSort.SelectedIndex = 0;

                _cmbRevSort.Items.Clear();
                _cmbRevSort.Items.AddRange(new object[] { "Ascending", "Descending" });
                _cmbRevSort.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                string message = ex.Message;
                Exception inner = ex.InnerException;

                while (inner != null)
                {
                    message += "\n\nInner: " + inner.Message;
                    inner = inner.InnerException;
                }

                MessageBox.Show($"Failed to load customers:\n{message}", "ForgePLM");
            }
            finally
            {
                _isLoadingContext = false;
            }
        }

        private async Task LoadProjectsForSelectedCustomerAsync()
        {
            try
            {
                _isLoadingContext = true;

                _cmbProject.Items.Clear();
                _cmbEco.Items.Clear();

                _allEcoRows = new List<EcoRowViewModel>();
                _ecoRows = new BindingList<EcoRowViewModel>(_allEcoRows);
                _gridEcoContents.DataSource = _ecoRows;

                var customer = GetSelectedCustomer();
                if (customer == null)
                    return;

                _projects = await _api.GetProjectsByCustomerAsync(customer.CustomerId);

                foreach (var project in _projects)
                {
                    _cmbProject.Items.Add($"{project.ProjectCode} - {project.ProjectName}");
                }

                if (_cmbProject.Items.Count > 0)
                {
                    _cmbProject.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load projects:\n{ex.Message}", "ForgePLM");
            }
            finally
            {
                _isLoadingContext = false;
            }
        }

        private async Task LoadEcosForSelectedProjectAsync()
        {
            try
            {
                _isLoadingContext = true;

                _cmbEco.Items.Clear();

                _allEcoRows = new List<EcoRowViewModel>();
                _ecoRows = new BindingList<EcoRowViewModel>(_allEcoRows);
                _gridEcoContents.DataSource = _ecoRows;

                var project = GetSelectedProject();
                if (project == null)
                    return;

                _ecos = await _api.GetEcosByProjectAsync(project.ProjectId);

                foreach (var eco in _ecos)
                {
                    _cmbEco.Items.Add($"{eco.EcoNumber} [{eco.EcoState}]");
                }

                if (_cmbEco.Items.Count > 0)
                {
                    _cmbEco.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load ECOs:\n{ex.Message}", "ForgePLM");
            }
            finally
            {
                _isLoadingContext = false;
            }
        }

        private async Task LoadEcoContentsForSelectedEcoAsync()
        {
            try
            {
                _isLoadingContext = true;
                var project = GetSelectedProject();

                string projectDisplay = project == null
                    ? string.Empty
                    : $"{project.ProjectCode} - {project.ProjectName}";

                var eco = GetSelectedEco();
                if (eco == null)
                {
                    _allEcoRows = new List<EcoRowViewModel>();
                    _ecoRows = new BindingList<EcoRowViewModel>(_allEcoRows);
                    _gridEcoContents.DataSource = _ecoRows;
                    return;
                }

                var revisions = await _api.GetEcoContentsAsync(eco.EcoId);
                var rows = new List<EcoRowViewModel>();

                foreach (var rev in revisions)
                {
                    rows.Add(new EcoRowViewModel
                    {
                        PartId = rev.PartId,
                        RevisionId = rev.RevisionId,
                        CategoryCode = rev.CategoryCode,
                        PartNumberInt = rev.PartNumberInt,
                        RevisionCode = rev.RevisionCode,
                        Description = rev.Description,
                        EcoNumber = rev.EcoNumber,
                        FilePath = rev.FilePath,
                        ProjectDisplay = projectDisplay // 🔥 add this
                    });
                }

                _allEcoRows = rows;
                _ecoRows = new BindingList<EcoRowViewModel>(_allEcoRows);
                _gridEcoContents.DataSource = _ecoRows;

                ApplyCategoryFilterOptions();
                ApplyGridViewState();
                EvaluateActiveDocument();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load ECO contents:\n{ex.Message}", "ForgePLM");
            }
            finally
            {
                _isLoadingContext = false;
            }
        }

        private CustomerDto GetSelectedCustomer()
        {
            int index = _cmbCustomer.SelectedIndex;
            if (index < 0 || index >= _customers.Count)
                return null;

            return _customers[index];
        }

        private ProjectDto GetSelectedProject()
        {
            int index = _cmbProject.SelectedIndex;
            if (index < 0 || index >= _projects.Count)
                return null;

            return _projects[index];
        }

        private EcoDto GetSelectedEco()
        {
            int index = _cmbEco.SelectedIndex;
            if (index < 0 || index >= _ecos.Count)
                return null;

            return _ecos[index];
        }

        private void ApplyCategoryFilterOptions()
        {
            string selected = _cmbCategoryFilter.SelectedItem?.ToString();

            var categories = new List<string> { "All" };

            foreach (var row in _allEcoRows)
            {
                if (!string.IsNullOrWhiteSpace(row.CategoryCode) && !categories.Contains(row.CategoryCode))
                {
                    categories.Add(row.CategoryCode);
                }
            }

            _cmbCategoryFilter.Items.Clear();

            foreach (var category in categories)
            {
                _cmbCategoryFilter.Items.Add(category);
            }

            if (!string.IsNullOrWhiteSpace(selected) && _cmbCategoryFilter.Items.Contains(selected))
            {
                _cmbCategoryFilter.SelectedItem = selected;
            }
            else if (_cmbCategoryFilter.Items.Count > 0)
            {
                _cmbCategoryFilter.SelectedIndex = 0;
            }
        }

        private void ApplyGridViewState()
        {
            var filtered = new List<EcoRowViewModel>(_allEcoRows);

            string category = _cmbCategoryFilter.SelectedItem?.ToString() ?? "All";
            string partSort = _cmbPartSort.SelectedItem?.ToString() ?? "Ascending";
            string revSort = _cmbRevSort.SelectedItem?.ToString() ?? "Ascending";

            if (category != "All")
            {
                filtered = filtered.FindAll(x => x.CategoryCode == category);
            }

            filtered.Sort((a, b) =>
            {
                int partCompare = a.PartNumberInt.CompareTo(b.PartNumberInt);
                if (partSort == "Descending")
                    partCompare *= -1;

                if (partCompare != 0)
                    return partCompare;

                int revCompare = string.Compare(a.RevisionCode, b.RevisionCode, StringComparison.OrdinalIgnoreCase);
                if (revSort == "Descending")
                    revCompare *= -1;

                return revCompare;
            });

            _ecoRows = new BindingList<EcoRowViewModel>(filtered);
            _gridEcoContents.DataSource = _ecoRows;

            EvaluateActiveDocument();
        }
        private async Task OpenSelectedRowAsync(EcoRowViewModel row)
        {
            if (_swApp == null)
                return;

            try
            {
                var openInfo = await _api.GetOpenInfoAsync(row.RevisionId);

                if (string.IsNullOrWhiteSpace(openInfo.FilePath))
                {
                    MessageBox.Show("No file path was returned for this revision.", "ForgePLM");
                    return;
                }

                if (!System.IO.File.Exists(openInfo.FilePath))
                {
                    MessageBox.Show($"Resolved file was not found:\n{openInfo.FilePath}", "ForgePLM");
                    return;
                }

                var existingDoc = FindOpenDocumentByPath(openInfo.FilePath);

                if (existingDoc != null)
                {
                    ActivateDocument(existingDoc);
                    EvaluateActiveDocument();
                    return;
                }



                int errors = 0;
                int warnings = 0;
                _swApp.OpenDoc6(
                    openInfo.FilePath,
                    GetSwDocumentType(openInfo.DocumentType),
                    (int)swOpenDocOptions_e.swOpenDocOptions_Silent,
                    "",
                    ref errors,
                    ref warnings);

                var openedDoc = FindOpenDocumentByPath(openInfo.FilePath);
                if (openedDoc != null)
                {
                    ActivateDocument(openedDoc);
                }

                EvaluateActiveDocument();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Open failed:\n{ex.Message}", "ForgePLM");
            }
        }

        private ModelDoc2 FindOpenDocumentByPath(string fullPath)
        {
            if (_swApp == null || string.IsNullOrWhiteSpace(fullPath))
                return null;

            string target = System.IO.Path.GetFullPath(fullPath);

            object[] docs = _swApp.GetDocuments() as object[];
            if (docs == null)
                return null;

            foreach (object obj in docs)
            {
                if (obj is ModelDoc2 doc)
                {
                    string docPath = doc.GetPathName();
                    if (string.IsNullOrWhiteSpace(docPath))
                        continue;

                    string current = System.IO.Path.GetFullPath(docPath);

                    if (string.Equals(current, target, StringComparison.OrdinalIgnoreCase))
                        return doc;
                }
            }

            return null;
        }

        private void ActivateDocument(ModelDoc2 doc)
        {
            if (_swApp == null || doc == null)
                return;

            string title = doc.GetTitle();
            int errors = 0;

            _swApp.ActivateDoc3(
                title,
                true,
                (int)swRebuildOnActivation_e.swUserDecision,
                ref errors);
        }

        private int GetSwDocumentType(string documentType)
        {

            switch ((documentType ?? string.Empty).Trim().ToUpperInvariant())
            {
                case "PART":
                    return (int)swDocumentTypes_e.swDocPART;

                case "ASSEMBLY":
                    return (int)swDocumentTypes_e.swDocASSEMBLY;

                case "DRAWING":
                    return (int)swDocumentTypes_e.swDocDRAWING;

                default:
                    throw new InvalidOperationException($"Unsupported document type: {documentType}");
            }
        }
        private async void GridEcoContents_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
                return;

            var row = _gridEcoContents.Rows[e.RowIndex].DataBoundItem as EcoRowViewModel;
            if (row == null)
                return;

            string columnName = _gridEcoContents.Columns[e.ColumnIndex].Name;
            var state = GetActiveFileState(row);

            if (!IsActionEnabled(columnName, state))
                return;

            switch (columnName)
            {
                case "Open":
                    await OpenSelectedRowAsync(row);
                    break;

                case "New":
                    CreateNewForRow(row);
                    break;

                case "Assign":
                    await AssignSelectedRowAsync(row);
                    break;
            }
        }

        private void GridEcoContents_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
                return;

            string columnName = _gridEcoContents.Columns[e.ColumnIndex].Name;

            if (columnName == "Open")
            {
                e.Value = "↗";
                e.FormattingApplied = true;
            }
            else if (columnName == "New")
            {
                e.Value = "+";
                e.FormattingApplied = true;
            }
            else if (columnName == "Assign")
            {
                e.Value = "✓";
                e.FormattingApplied = true;
            }
        }

        private void GridEcoContents_CellToolTipTextNeeded(object sender, DataGridViewCellToolTipTextNeededEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
                return;

            var row = _gridEcoContents.Rows[e.RowIndex].DataBoundItem as EcoRowViewModel;
            if (row == null)
                return;

            string columnName = _gridEcoContents.Columns[e.ColumnIndex].Name;

            switch (columnName)
            {
                case "Open":
                    e.ToolTipText = $"Open {row.DisplayPartNumber} in SolidWorks";
                    break;

                case "New":
                    {
                        var state = GetActiveFileState(row);
                        e.ToolTipText = state == ActiveFileState.Derived
                            ? $"Create new file for {row.DisplayPartNumber}"
                            : "New not available (file is already managed)";
                        break;
                    }

                case "Assign":
                    {
                        var state = GetActiveFileState(row);
                        e.ToolTipText = state == ActiveFileState.Derived
                            ? $"Assign active file to {row.DisplayPartNumber}"
                            : "Assign not available unless the active file is Derived";
                        break;
                    }

                case "Part":
                    e.ToolTipText = $"Part Number: {row.DisplayPartNumber}";
                    break;

                case "Rev":
                    e.ToolTipText = $"Revision: {row.RevisionCode}";
                    break;

                case "Description":
                    e.ToolTipText = row.Description;
                    break;
            }
        }


        private void CreateNewForRow(EcoRowViewModel row)
        {
            MessageBox.Show($"Create new file for {row.DisplayPartNumber} coming next.", "ForgePLM");
        }
        private string BuildDevelopmentVaultPath(EcoRowViewModel row, string extensionWithDot)
        {
            string basePath = @"e:\SteamFactory_DEV\projects";
            string projectDisplay = row.ProjectDisplay ?? string.Empty;
            string fileName = row.DisplayPartNumber + extensionWithDot;

            return System.IO.Path.Combine(basePath, projectDisplay, "development", fileName);
        }

        private string GetActiveDocumentExtension(ModelDoc2 model)
        {
            if (model == null)
                return string.Empty;

            string path = model.GetPathName();
            if (!string.IsNullOrWhiteSpace(path))
            {
                return System.IO.Path.GetExtension(path);
            }

            switch ((swDocumentTypes_e)model.GetType())
            {
                case swDocumentTypes_e.swDocPART:
                    return ".sldprt";

                case swDocumentTypes_e.swDocASSEMBLY:
                    return ".sldasm";

                case swDocumentTypes_e.swDocDRAWING:
                    return ".slddrw";

                default:
                    return string.Empty;
            }
        }
        private void ShowSaveToVaultDialog(EcoRowViewModel row)
        {
            var model = GetActiveModel();
            if (model == null)
            {
                MessageBox.Show("No active SolidWorks document.", "ForgePLM");
                return;
            }

            string extension = GetActiveDocumentExtension(model);
            if (string.IsNullOrWhiteSpace(extension))
            {
                MessageBox.Show("Could not determine the active document extension.", "ForgePLM");
                return;
            }

            string fullTargetPath = BuildDevelopmentVaultPath(row, extension);

            using (var dialog = new SaveToVaultDialog(
                row.ProjectDisplay ?? string.Empty,
                row.DisplayPartNumber,
                extension,
                fullTargetPath))
            {
                if (dialog.ShowDialog(this) != DialogResult.OK)
                    return;
            }

            try
            {
                string targetDirectory = Path.GetDirectoryName(fullTargetPath);

                if (!string.IsNullOrWhiteSpace(targetDirectory))
                {
                    Directory.CreateDirectory(targetDirectory);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed while creating directory:\n{ex}", "ForgePLM");
                return;
            }

            try
            {
                int errors = 0;
                int warnings = 0;

                bool success = model.Extension.SaveAs(
                    fullTargetPath,
                    (int)swSaveAsVersion_e.swSaveAsCurrentVersion,
                    (int)swSaveAsOptions_e.swSaveAsOptions_Silent,
                    null,
                    ref errors,
                    ref warnings);

                if (!success)
                {
                    MessageBox.Show(
                        $"Save failed.\nErrors: {errors}\nWarnings: {warnings}",
                        "ForgePLM");
                    return;
                }

                if (warnings != 0)
                {
                    MessageBox.Show(
                        $"Saved with warnings.\nWarnings: {warnings}",
                        "ForgePLM");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed during SaveAs:\n{ex}", "ForgePLM");
                return;
            }
        }
        private async Task AssignSelectedRowAsync(EcoRowViewModel row)
        {
            var model = GetActiveModel();
            if (model == null)
            {
                MessageBox.Show("No active SolidWorks document.", "ForgePLM");
                return;
            }

            var eval = EvaluateActiveFile(row);

            if (eval.State != ActiveFileState.Derived)
            {
                MessageBox.Show("Assign is only available for a Derived file.", "ForgePLM");
                return;
            }

            try
            {
                var result = await _api.AssignRevisionAsync(new AssignRevisionRequest
                {
                    RevisionId = row.RevisionId
                });

                if (result == null)
                {
                    MessageBox.Show("Runtime returned no assignment data.", "ForgePLM");
                    return;
                }

                var propMgr = model.Extension.CustomPropertyManager[""];

                EnsureProperty(propMgr, PropGuid, result.Guid ?? string.Empty);
                EnsureProperty(propMgr, PropPartId, result.PartId.ToString());
                EnsureProperty(propMgr, PropRevisionId, result.RevisionId.ToString());
                EnsureProperty(propMgr, PropPartNumber, result.PartNumber ?? string.Empty);
                EnsureProperty(propMgr, PropRevision, result.RevisionCode ?? string.Empty);
                EnsureProperty(propMgr, PropDescription, result.Description ?? string.Empty);
                EnsureProperty(propMgr, PropEco, result.EcoNumber ?? string.Empty);
                EnsureProperty(propMgr, PropProject, row.ProjectDisplay ?? string.Empty);

                model.ForceRebuild3(false);
                model.Save3((int)swSaveAsOptions_e.swSaveAsOptions_Silent, 0, 0);

                EvaluateActiveDocument();

                ShowSaveToVaultDialog(row);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Assign failed:\n{ex.Message}", "ForgePLM");
            }
        }

        private void EvaluateActiveDocument()
        {
            EcoRowViewModel selectedRow = null;

            if (_gridEcoContents.CurrentRow?.DataBoundItem is EcoRowViewModel current)
            {
                selectedRow = current;
            }

            var eval = EvaluateActiveFile(selectedRow);

            RefreshActiveFilePanel(new ActiveFileStatus
            {
                FileName = eval.FileName,
                StateText = eval.State.ToString(),
                PartNumber = eval.PartNumber,
                Revision = eval.Revision,
                Project = eval.Project, // 🔥 NEW
                Note = eval.Note
            });
        }

        private void RefreshActiveFilePanel(ActiveFileStatus status)
        {
            if (status == null)
            {
                _lblActiveFileValue.Text = "No active document";
                _lblActiveStateValue.Text = "None";
                _lblActivePartValue.Text = "—";
                _lblActiveRevValue.Text = "—";
                _lblActiveNoteValue.Text = string.Empty;
                return;
            }

            _lblActiveFileValue.Text = status.FileName ?? "—";
            _lblActiveStateValue.Text = status.StateText ?? "—";
            _lblActivePartValue.Text = status.PartNumber ?? "—";
            _lblActiveRevValue.Text = status.Revision ?? "—";
            _lblActiveProjectValue.Text = status.Project ?? "—";
            _lblActiveNoteValue.Text = status.Note ?? string.Empty;
        }

        private bool IsActionEnabled(string actionName, ActiveFileState state)
        {
            switch (actionName)
            {
                case "Open":
                    return true;

                case "New":
                    return state == ActiveFileState.None || state == ActiveFileState.Derived;

                case "Assign":
                    return state == ActiveFileState.Derived;

                default:
                    return false;
            }
        }

        private ActiveFileState GetActiveFileState(EcoRowViewModel selectedRow)
        {
            return EvaluateActiveFile(selectedRow).State;
        }

        private class ActiveFileEvaluation
        {
            public ActiveFileState State { get; set; }
            public string FileName { get; set; }
            public string PartNumber { get; set; }
            public string Revision { get; set; }
            public string GuidValue { get; set; }
            public int? PartId { get; set; }
            public int? RevisionId { get; set; }
            public string Note { get; set; }
            public string Project { get; set; }
        }

        private ActiveFileEvaluation EvaluateActiveFile(EcoRowViewModel selectedRow)
        {
            var model = GetActiveModel();

            if (model == null)
            {
                return new ActiveFileEvaluation
                {
                    State = ActiveFileState.None,
                    FileName = "No active document",
                    PartNumber = "—",
                    Revision = "—",
                    Note = "Open or create a file to begin."
                };
            }

            string fullPath = model.GetPathName();
            string fileName = string.IsNullOrWhiteSpace(fullPath)
                ? (model.GetTitle() ?? "—")
                : Path.GetFileName(fullPath);

            string guid = GetCustomProperty(model, PropGuid);
            string partIdText = GetCustomProperty(model, PropPartId);
            string revisionIdText = GetCustomProperty(model, PropRevisionId);
            string partNumber = GetCustomProperty(model, PropPartNumber);
            string revision = GetCustomProperty(model, PropRevision);
            string project = GetCustomProperty(model, PropProject);



            int parsedPartId;
            int? partId = int.TryParse(partIdText, out parsedPartId) ? parsedPartId : (int?)null;

            int parsedRevisionId;
            int? revisionId = int.TryParse(revisionIdText, out parsedRevisionId) ? parsedRevisionId : (int?)null;

            if (string.IsNullOrWhiteSpace(guid))
            {
                return new ActiveFileEvaluation
                {
                    State = ActiveFileState.Derived,
                    FileName = fileName,
                    PartNumber = string.IsNullOrWhiteSpace(partNumber) ? "—" : partNumber,
                    Revision = string.IsNullOrWhiteSpace(revision) ? "—" : revision,
                    GuidValue = guid,
                    PartId = partId,
                    RevisionId = revisionId,
                    Project = string.IsNullOrWhiteSpace(project) ? "—" : project,
                    Note = "No ForgePLM GUID detected."
                };
            }

            if (selectedRow != null && partId.HasValue && partId.Value != selectedRow.PartId)
            {
                return new ActiveFileEvaluation
                {
                    State = ActiveFileState.Conflict,
                    FileName = fileName,
                    PartNumber = string.IsNullOrWhiteSpace(partNumber) ? "—" : partNumber,
                    Revision = string.IsNullOrWhiteSpace(revision) ? "—" : revision,
                    GuidValue = guid,
                    PartId = partId,
                    RevisionId = revisionId,
                    Project = string.IsNullOrWhiteSpace(project) ? "—" : project,
                    Note = "Active file does not match selected ECO item."
                };
            }

            return new ActiveFileEvaluation
            {
                State = ActiveFileState.Managed,
                FileName = fileName,
                PartNumber = string.IsNullOrWhiteSpace(partNumber) ? "—" : partNumber,
                Revision = string.IsNullOrWhiteSpace(revision) ? "—" : revision,
                GuidValue = guid,
                PartId = partId,
                RevisionId = revisionId,
                Project = string.IsNullOrWhiteSpace(project) ? "—" : project,
                Note = "ForgePLM-managed file."
            };
        }

        private ModelDoc2 GetActiveModel()
        {
            return _swApp?.ActiveDoc as ModelDoc2;
        }

        private string GetCustomProperty(ModelDoc2 model, string name)
        {
            if (model == null)
                return string.Empty;

            var propMgr = model.Extension.CustomPropertyManager[""];

            string rawValue;
            string resolvedValue;
            bool wasResolved;
            bool linkToProp;

            propMgr.Get6(name, false, out rawValue, out resolvedValue, out wasResolved, out linkToProp);

            return string.IsNullOrWhiteSpace(resolvedValue)
                ? (rawValue ?? string.Empty)
                : resolvedValue;
        }

        private void EnsureProperty(CustomPropertyManager propMgr, string name, string value)
        {
            propMgr.Add3(
                name,
                (int)swCustomInfoType_e.swCustomInfoText,
                value ?? string.Empty,
                (int)swCustomPropertyAddOption_e.swCustomPropertyReplaceValue);
        }
    }

    public enum ActiveFileState
    {
        None,
        Derived,
        Managed,
        Conflict
    }

    public class EcoRowViewModel
    {

        public int PartId { get; set; }
        public int RevisionId { get; set; }

        public string CategoryCode { get; set; }
        public int PartNumberInt { get; set; }
        public string RevisionCode { get; set; }
        public string Description { get; set; }
        public string EcoNumber { get; set; }

        public string FilePath { get; set; }

        // 🔥 NEW
        public string ProjectDisplay { get; set; }  // "PRIOG-0001 - PrioVision"

        public string DisplayPartNumber
        {
            get { return $"{CategoryCode}-{PartNumberInt:D7}"; }
        }
    }

    public class ActiveFileStatus
    {
        public string FileName { get; set; }
        public string StateText { get; set; }

        public string PartNumber { get; set; }
        public string Revision { get; set; }

        // 🔥 NEW
        public string Project { get; set; }

        public string Note { get; set; }
    }
}