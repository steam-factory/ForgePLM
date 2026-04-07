
using System;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;


namespace ForgePLM.SolidWorks.Addin
{
    [ComVisible(true)]
    [Guid("A7E8C2D1-5F3B-4D91-9A6F-2C4D8B7E2201")]
    [ProgId("ForgePLM.SolidWorks.Addin.ForgePlmTaskPaneControl")]
    public class ForgePlmTaskPaneControl : UserControl
    {
        private SldWorks _swApp;

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
        private BindingList<EcoRowViewModel> _ecoRows = new BindingList<EcoRowViewModel>();

        // Active file
        private GroupBox _activeFileGroup;
        private TableLayoutPanel _activeFileLayout;
        private Label _lblActiveFileValue;
        private Label _lblActiveStateValue;
        private Label _lblActivePartValue;
        private Label _lblActiveRevValue;
        private Label _lblActiveNoteValue;

        private const string PropGuid = "PLM_GUID";
        private const string PropPartId = "PLM_PartId";
        private const string PropRevisionId = "PLM_RevisionId";
        private const string PropPartNumber = "PLM_PartNumber";
        private const string PropRevision = "PLM_Revision";
        private const string PropDescription = "PLM_Description";
        private const string PropEco = "PLM_ECO";

        public ForgePlmTaskPaneControl()
        {
            BuildUi();
            WireEvents();
            SeedMockData();
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
            _rootLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));      // context
            _rootLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));  // eco contents
            _rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 210)); // active file

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
                BorderStyle = BorderStyle.FixedSingle
            };

            BuildEcoGridColumns();

            _ecoLayout.Controls.Add(_filterPanel, 0, 0);
            _ecoLayout.Controls.Add(_gridEcoContents, 0, 1);

            _ecoGroup.Controls.Add(_ecoLayout);
        }

        private void BuildEcoGridColumns()
        {
            _gridEcoContents.Columns.Clear();
            _gridEcoContents.ShowCellToolTips = true;

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
                        var state = GetMockActiveFileState(row);

                        if (state != ActiveFileState.Derived)
                        {
                            e.ToolTipText = "New not available (file is already managed)";
                        }
                        else
                        {
                            e.ToolTipText = $"Create new file for {row.DisplayPartNumber}";
                        }
                        break;
                    }

                case "Assign":
                    {
                        var state = GetMockActiveFileState(row);

                        if (state != ActiveFileState.Derived)
                        {
                            e.ToolTipText = "Assign not available (file is already managed)";
                        }
                        else
                        {
                            e.ToolTipText = $"Assign active file to {row.DisplayPartNumber}";
                        }
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
                RowCount = 5,
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

            _activeFileLayout.Controls.Add(new Label { Text = "Note", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 4);
            _activeFileLayout.Controls.Add(_lblActiveNoteValue, 1, 4);

            _activeFileGroup.Controls.Add(_activeFileLayout);
        }

        private void WireEvents()
        {
            _cmbCategoryFilter.SelectedIndexChanged += (_, __) => ApplyGridViewState();
            _cmbPartSort.SelectedIndexChanged += (_, __) => ApplyGridViewState();
            _cmbRevSort.SelectedIndexChanged += (_, __) => ApplyGridViewState();
            _cmbEco.SelectedIndexChanged += (_, __) => ReloadEcoContentsForSelectedEco();

            _gridEcoContents.CellContentClick += GridEcoContents_CellContentClick;
            _gridEcoContents.CellFormatting += GridEcoContents_CellFormatting;
            _gridEcoContents.SelectionChanged += (_, __) => EvaluateActiveDocument();
            _gridEcoContents.CellToolTipTextNeeded += GridEcoContents_CellToolTipTextNeeded;
        }

        private void SeedMockData()
        {
            _cmbCustomer.Items.AddRange(new object[] { "Acme Corp", "Globex", "Initech" });
            _cmbProject.Items.AddRange(new object[] { "Project Falcon", "Project Atlas" });
            _cmbEco.Items.AddRange(new object[] { "ECO-000123", "ECO-000124" });

            _cmbCategoryFilter.Items.AddRange(new object[] { "All", "CM", "HW", "EL" });
            _cmbPartSort.Items.AddRange(new object[] { "Ascending", "Descending" });
            _cmbRevSort.Items.AddRange(new object[] { "Ascending", "Descending" });

            _cmbCustomer.SelectedIndex = 0;
            _cmbProject.SelectedIndex = 0;
            _cmbEco.SelectedIndex = 0;
            _cmbCategoryFilter.SelectedIndex = 0;
            _cmbPartSort.SelectedIndex = 0;
            _cmbRevSort.SelectedIndex = 0;

            _ecoRows = new BindingList<EcoRowViewModel>
            {
                new EcoRowViewModel { PartId = 510, RevisionId = 1001, CategoryCode = "CM", PartNumberInt = 510, RevisionCode = "101", Description = "Enclosure Cover", EcoNumber = "ECO-000123", FilePath = @"C:\ForgePLM\Dev\CM-0000510.sldprt" },
                new EcoRowViewModel { PartId = 511, RevisionId = 1002, CategoryCode = "CM", PartNumberInt = 511, RevisionCode = "101", Description = "Enclosure Base", EcoNumber = "ECO-000123", FilePath = @"C:\ForgePLM\Dev\CM-0000511.sldprt" }
             };

            _gridEcoContents.DataSource = _ecoRows;
        }

        private void AssignSelectedRow(EcoRowViewModel row)
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

            var propMgr = model.Extension.CustomPropertyManager[""];

            EnsureProperty(propMgr, PropGuid, Guid.NewGuid().ToString().ToUpperInvariant());
            EnsureProperty(propMgr, PropPartId, row.PartId.ToString());
            EnsureProperty(propMgr, PropRevisionId, row.RevisionId.ToString());
            EnsureProperty(propMgr, PropPartNumber, row.DisplayPartNumber);
            EnsureProperty(propMgr, PropRevision, row.RevisionCode);
            EnsureProperty(propMgr, PropDescription, row.Description ?? string.Empty);
            EnsureProperty(propMgr, PropEco, row.EcoNumber ?? string.Empty);

            model.ForceRebuild3(false);
            model.Save3((int)swSaveAsOptions_e.swSaveAsOptions_Silent, 0, 0);

            EvaluateActiveDocument();
        }

        private void ReloadEcoContentsForSelectedEco()
        {
            // TODO:
            // Replace with API call by selected ECO.
            // For now, re-apply current filters/sorts to existing mock data.
            ApplyGridViewState();
        }

        private ActiveFileState GetActiveFileState(EcoRowViewModel selectedRow)
        {
            return EvaluateActiveFile(selectedRow).State;
        }
        private void ApplyGridViewState()
        {
            var allRows = new List<EcoRowViewModel>
            {
                new EcoRowViewModel { CategoryCode = "CM", PartNumberInt = 510, RevisionCode = "101", Description = "Enclosure Cover" },
                new EcoRowViewModel { CategoryCode = "CM", PartNumberInt = 511, RevisionCode = "101", Description = "Enclosure Base" },
                new EcoRowViewModel { CategoryCode = "HW", PartNumberInt = 124, RevisionCode = "201", Description = "Shoulder Bolt, 1/4-20 x 1.00" },
                new EcoRowViewModel { CategoryCode = "EL", PartNumberInt = 88, RevisionCode = "102", Description = "Power Entry Module" }
            };

            string category = _cmbCategoryFilter.SelectedItem?.ToString() ?? "All";
            string partSort = _cmbPartSort.SelectedItem?.ToString() ?? "Ascending";
            string revSort = _cmbRevSort.SelectedItem?.ToString() ?? "Ascending";

            if (category != "All")
            {
                allRows = allRows.FindAll(x => x.CategoryCode == category);
            }

            allRows.Sort((a, b) =>
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

            _ecoRows = new BindingList<EcoRowViewModel>(allRows);
            _gridEcoContents.DataSource = _ecoRows;

            EvaluateActiveDocument();
        }

        private void GridEcoContents_CellContentClick(object sender, DataGridViewCellEventArgs e)
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
                    OpenSelectedRow(row);
                    break;

                case "New":
                    CreateNewForRow(row);
                    break;

                case "Assign":
                    AssignSelectedRow(row);
                    break;
            }
        }

        private void OpenSelectedRow(EcoRowViewModel row)
        {
            if (_swApp == null)
                return;

            if (string.IsNullOrWhiteSpace(row.FilePath))
            {
                MessageBox.Show("No file path is defined for this row.", "ForgePLM");
                return;
            }

            int errors = 0;
            int warnings = 0;

            _swApp.OpenDoc6(
                row.FilePath,
                (int)swDocumentTypes_e.swDocPART,
                (int)swOpenDocOptions_e.swOpenDocOptions_Silent,
                "",
                ref errors,
                ref warnings);

            EvaluateActiveDocument();
        }

        private void CreateNewForRow(EcoRowViewModel row)
        {
            MessageBox.Show($"Create new file for {row.DisplayPartNumber} coming next.", "ForgePLM");
        }

        private void GridEcoContents_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0)
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

        private ActiveFileState GetMockActiveFileState(EcoRowViewModel selectedRow)
        {
            // TEMP MOCK LOGIC:
            // You’ll replace this with real GUID/property inspection.
            // This is only to make the skeleton feel alive.

            if (selectedRow == null)
                return ActiveFileState.None;

            if (selectedRow.CategoryCode == "CM")
                return ActiveFileState.Derived;

            if (selectedRow.CategoryCode == "HW")
                return ActiveFileState.Managed;

            if (selectedRow.CategoryCode == "EL")
                return ActiveFileState.Conflict;

            return ActiveFileState.None;
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

            string fileName = model.GetTitle() ?? "—";
            string guid = GetCustomProperty(model, PropGuid);
            string partIdText = GetCustomProperty(model, PropPartId);
            string revisionIdText = GetCustomProperty(model, PropRevisionId);
            string partNumber = GetCustomProperty(model, PropPartNumber);
            string revision = GetCustomProperty(model, PropRevision);

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
        public string Note { get; set; }
    }


}

