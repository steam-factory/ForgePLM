using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace ForgePLM.SolidWorks.Addin
{
    public class SaveToVaultDialog : Form
    {
        private TextBox _txtProject;
        private TextBox _txtFileName;
        private TextBox _txtExtension;
        private TextBox _txtFullPath;
        private Button _btnSave;
        private Button _btnCancel;

        public string ProjectDisplay { get; }
        public string FileNameOnly { get; }
        public string ExtensionOnly { get; }
        public string FullPathValue { get; }

        public SaveToVaultDialog(string projectDisplay, string fileNameOnly, string extensionOnly, string fullPath)
        {
            ProjectDisplay = projectDisplay ?? string.Empty;
            FileNameOnly = fileNameOnly ?? string.Empty;
            ExtensionOnly = extensionOnly ?? string.Empty;
            FullPathValue = fullPath ?? string.Empty;

            BuildUi();
        }

        private void BuildUi()
        {
            Text = "Save to Vault";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            ClientSize = new Size(640, 260);

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 5,
                Padding = new Padding(12)
            };

            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            _txtProject = CreateReadOnlyTextBox(ProjectDisplay);
            _txtFileName = CreateReadOnlyTextBox(FileNameOnly);
            _txtExtension = CreateReadOnlyTextBox(ExtensionOnly);

            _txtFullPath = new TextBox
            {
                Multiline = true,
                ReadOnly = true,
                Dock = DockStyle.Fill,
                ScrollBars = ScrollBars.Vertical,
                Text = FullPathValue
            };

            layout.Controls.Add(new Label { Text = "Project", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 0);
            layout.Controls.Add(_txtProject, 1, 0);

            layout.Controls.Add(new Label { Text = "Filename", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 1);
            layout.Controls.Add(_txtFileName, 1, 1);

            layout.Controls.Add(new Label { Text = "Extension", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 2);
            layout.Controls.Add(_txtExtension, 1, 2);

            layout.Controls.Add(new Label { Text = "Path", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 3);
            layout.Controls.Add(_txtFullPath, 1, 3);

            var buttonPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                AutoSize = true,
                WrapContents = false
            };

            _btnSave = new Button
            {
                Text = "Save to Vault",
                AutoSize = true,
                DialogResult = DialogResult.OK
            };

            _btnCancel = new Button
            {
                Text = "Cancel",
                AutoSize = true,
                DialogResult = DialogResult.Cancel
            };

            buttonPanel.Controls.Add(_btnSave);
            buttonPanel.Controls.Add(_btnCancel);

            layout.Controls.Add(buttonPanel, 1, 4);

            AcceptButton = _btnSave;
            CancelButton = _btnCancel;

            Controls.Add(layout);
        }
       

        private TextBox CreateReadOnlyTextBox(string value)
        {
            return new TextBox
            {
                ReadOnly = true,
                Dock = DockStyle.Top,
                Text = value
            };
        }
    }
}