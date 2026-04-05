using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;

namespace ForgePLM.SolidWorks.Addin
{
    [ComVisible(true)]
    [Guid("A7E8C2D1-5F3B-4D91-9A6F-2C4D8B7E2201")]
    [ProgId("ForgePLM.SolidWorks.Addin.HelloTaskPaneControl")]
    public partial class HelloTaskPaneControl : UserControl
    {
        private SldWorks _swApp;

        private Panel _topPanel;
        private Button _btnHello;
        private Button _btnRead;
        private Button _btnWrite;

        private Panel _contentPanel;
        private TableLayoutPanel _propertyTable;

        private readonly Dictionary<string, TextBox> _propertyTextBoxes = new Dictionary<string, TextBox>();

        private readonly string[] _plmPropertyNames =
        {
            "PLM_GUID",
            "PLM_PartNumber",
            "PLM_Revision",
            "PLM_Description",
            "PLM_Project",
            "PLM_ECO",
            "PLM_Status",
            "PLM_ReleaseLevel",
            "PLM_Category",
            "PLM_Material",
            "PLM_Finish",
            "PLM_UnitOfMeasure",
            "PLM_Source",
            "PLM_Vendor",
            "PLM_VendorPartNumber",
            "PLM_LeadTime",
            "PLM_UnitCost",
            "PLM_SalePrice",
            "PLM_ArtifactCounter",
            "PLM_LastSyncUtc"
        };

        public HelloTaskPaneControl()
        {
            InitializeComponent();
            BuildUi();
        }

        public void Initialize(SldWorks swApp)
        {
            _swApp = swApp;
            MessageBox.Show("HelloTaskPaneControl initialized.", "ForgePLM");
        }

        private void BuildUi()
        {
            Controls.Clear();
            Dock = DockStyle.Fill;

            _topPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 42
            };

            _btnHello = new Button
            {
                Text = "Hello",
                Left = 8,
                Top = 8,
                Width = 70
            };
            _btnHello.Click += BtnHello_Click;

            _btnRead = new Button
            {
                Text = "Read",
                Left = 86,
                Top = 8,
                Width = 70
            };
            _btnRead.Click += BtnRead_Click;

            _btnWrite = new Button
            {
                Text = "Write Test",
                Left = 164,
                Top = 8,
                Width = 90
            };
            _btnWrite.Click += BtnWrite_Click;

            _topPanel.Controls.Add(_btnHello);
            _topPanel.Controls.Add(_btnRead);
            _topPanel.Controls.Add(_btnWrite);

            _contentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true
            };

            _propertyTable = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                ColumnCount = 2,
                Padding = new Padding(8)
            };

            _propertyTable.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140));
            _propertyTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            int row = 0;
            AddInfoRow("DocumentTitle", row++);
            AddInfoRow("DocumentType", row++);
            AddInfoRow("DocumentPath", row++);

            AddSpacerRow(row++);

            foreach (string propName in _plmPropertyNames)
            {
                AddPropertyRow(propName, row++);
            }

            _contentPanel.Controls.Add(_propertyTable);

            Controls.Add(_contentPanel);
            Controls.Add(_topPanel);
        }

        private void AddInfoRow(string key, int rowIndex)
        {
            _propertyTable.RowCount++;

            _propertyTable.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            var label = new Label
            {
                Text = key,
                AutoSize = true,
                Anchor = AnchorStyles.Left,
                Margin = new Padding(3, 6, 3, 3)
            };

            var textBox = new TextBox
            {
                ReadOnly = true,
                Dock = DockStyle.Top
            };

            _propertyTextBoxes[key] = textBox;

            _propertyTable.Controls.Add(label, 0, rowIndex);
            _propertyTable.Controls.Add(textBox, 1, rowIndex);
        }

        private void AddPropertyRow(string propertyName, int rowIndex)
        {
            _propertyTable.RowCount++;

            _propertyTable.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            var label = new Label
            {
                Text = propertyName,
                AutoSize = true,
                Anchor = AnchorStyles.Left,
                Margin = new Padding(3, 6, 3, 3)
            };

            var textBox = new TextBox
            {
                Dock = DockStyle.Top
            };

            _propertyTextBoxes[propertyName] = textBox;

            _propertyTable.Controls.Add(label, 0, rowIndex);
            _propertyTable.Controls.Add(textBox, 1, rowIndex);
        }

        private void AddSpacerRow(int rowIndex)
        {
            _propertyTable.RowCount++;
            _propertyTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 12));

            var spacer = new Panel
            {
                Height = 12,
                Dock = DockStyle.Top
            };

            _propertyTable.Controls.Add(spacer, 0, rowIndex);
            _propertyTable.SetColumnSpan(spacer, 2);
        }

        private void BtnHello_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Hello World from ForgePLM!", "ForgePLM");
        }

        private void BtnRead_Click(object sender, EventArgs e)
        {
            ReadPropertiesFromActiveDocument();
        }

        private void BtnWrite_Click(object sender, EventArgs e)
        {
            WriteTestPropertiesToActiveDocument();
        }

        private void ReadPropertiesFromActiveDocument()
        {
            try
            {
                if (_swApp == null)
                {
                    MessageBox.Show("_swApp is null. Task pane was not initialized.", "ForgePLM");
                    SetStatusNoDocument();
                    return;
                }

                ModelDoc2 model = _swApp.ActiveDoc;

                if (model == null)
                {
                    MessageBox.Show("SolidWorks is connected, but there is no active document.", "ForgePLM");
                    SetStatusNoDocument();
                    return;
                }

                _propertyTextBoxes["DocumentTitle"].Text = model.GetTitle();
                _propertyTextBoxes["DocumentType"].Text = GetDocumentTypeName(model);
                _propertyTextBoxes["DocumentPath"].Text = model.GetPathName();

                CustomPropertyManager propMgr = model.Extension.CustomPropertyManager[""];

                foreach (string propName in _plmPropertyNames)
                {
                    string rawValue;
                    string resolvedValue;
                    bool wasResolved;
                    bool linkToProp;

                    propMgr.Get6(
                        propName,
                        false,
                        out rawValue,
                        out resolvedValue,
                        out wasResolved,
                        out linkToProp);

                    string displayValue = string.IsNullOrWhiteSpace(resolvedValue) ? rawValue : resolvedValue;
                    _propertyTextBoxes[propName].Text = displayValue ?? string.Empty;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error reading properties:\n{ex.Message}", "ForgePLM");
            }
        }

        private void WriteTestPropertiesToActiveDocument()
        {
            try
            {
                ModelDoc2 model = _swApp?.ActiveDoc;

                if (model == null)
                {
                    MessageBox.Show("No active document.", "ForgePLM");
                    return;
                }

                CustomPropertyManager propMgr = model.Extension.CustomPropertyManager[""];

                EnsureProperty(propMgr, "PLM_GUID", GetOrCreateGuid(propMgr));
                EnsureProperty(propMgr, "PLM_Status", "Draft");
                EnsureProperty(propMgr, "PLM_LastSyncUtc", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"));
                EnsureProperty(propMgr, "PLM_ArtifactCounter", "0");

                model.ForceRebuild3(false);

                ReadPropertiesFromActiveDocument();
                MessageBox.Show("Test properties written.", "ForgePLM");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error writing properties:\n{ex.Message}", "ForgePLM");
            }
        }

        private void EnsureProperty(CustomPropertyManager propMgr, string name, string value)
        {
            propMgr.Add3(
                name,
                (int)swCustomInfoType_e.swCustomInfoText,
                value,
                (int)swCustomPropertyAddOption_e.swCustomPropertyReplaceValue);
        }

        private string GetOrCreateGuid(CustomPropertyManager propMgr)
        {
            string rawValue;
            string resolvedValue;
            bool wasResolved;
            bool linkToProp;

            propMgr.Get6("PLM_GUID", false, out rawValue, out resolvedValue, out wasResolved, out linkToProp);

            string existing = string.IsNullOrWhiteSpace(resolvedValue) ? rawValue : resolvedValue;

            if (!string.IsNullOrWhiteSpace(existing))
            {
                return existing;
            }

            return Guid.NewGuid().ToString().ToUpper();
        }

        private void SetStatusNoDocument()
        {
            _propertyTextBoxes["DocumentTitle"].Text = string.Empty;
            _propertyTextBoxes["DocumentType"].Text = "No active document";
            _propertyTextBoxes["DocumentPath"].Text = string.Empty;

            foreach (string propName in _plmPropertyNames)
            {
                _propertyTextBoxes[propName].Text = string.Empty;
            }
        }

        private string GetDocumentTypeName(ModelDoc2 model)
        {
            switch ((swDocumentTypes_e)model.GetType())
            {
                case swDocumentTypes_e.swDocPART:
                    return "Part";
                case swDocumentTypes_e.swDocASSEMBLY:
                    return "Assembly";
                case swDocumentTypes_e.swDocDRAWING:
                    return "Drawing";
                default:
                    return "Unknown";
            }
        }
    }
}