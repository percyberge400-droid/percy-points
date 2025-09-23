namespace POSPRA_WinFormsUI.Forms
{
    partial class ExportInvoiceForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            label1 = new Label();
            dateTimePickerFrom = new DateTimePicker();
            lblExportStatus = new Label();
            DateFromLbl = new Label();
            progressBarExport = new ProgressBar();
            dateTimePickerTo = new DateTimePicker();
            ExportInvoiceBtn = new Button();
            RegNumNumericUpDown = new NumericUpDown();
            label2 = new Label();
            ToDateLbl = new Label();
            ((System.ComponentModel.ISupportInitialize)RegNumNumericUpDown).BeginInit();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Microsoft Sans Serif", 16.2F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label1.Location = new Point(12, 9);
            label1.Name = "label1";
            label1.Size = new Size(222, 32);
            label1.TabIndex = 59;
            label1.Text = "Export Invoices";
            // 
            // dateTimePickerFrom
            // 
            dateTimePickerFrom.Anchor = AnchorStyles.Top;
            dateTimePickerFrom.Font = new Font("Microsoft Sans Serif", 9F);
            dateTimePickerFrom.Location = new Point(114, 83);
            dateTimePickerFrom.MinDate = new DateTime(1947, 10, 10, 0, 0, 0, 0);
            dateTimePickerFrom.Name = "dateTimePickerFrom";
            dateTimePickerFrom.Size = new Size(253, 24);
            dateTimePickerFrom.TabIndex = 55;
            // 
            // lblExportStatus
            // 
            lblExportStatus.AutoSize = true;
            lblExportStatus.Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Bold);
            lblExportStatus.ForeColor = Color.Green;
            lblExportStatus.Location = new Point(114, 175);
            lblExportStatus.Name = "lblExportStatus";
            lblExportStatus.Size = new Size(0, 18);
            lblExportStatus.TabIndex = 50;
            // 
            // DateFromLbl
            // 
            DateFromLbl.Anchor = AnchorStyles.Top;
            DateFromLbl.AutoSize = true;
            DateFromLbl.Font = new Font("Microsoft Sans Serif", 9F);
            DateFromLbl.Location = new Point(114, 62);
            DateFromLbl.Name = "DateFromLbl";
            DateFromLbl.Size = new Size(79, 18);
            DateFromLbl.TabIndex = 52;
            DateFromLbl.Text = "From Date";
            // 
            // progressBarExport
            // 
            progressBarExport.ForeColor = Color.FromArgb(128, 255, 128);
            progressBarExport.Location = new Point(784, 166);
            progressBarExport.Name = "progressBarExport";
            progressBarExport.Size = new Size(605, 27);
            progressBarExport.TabIndex = 58;
            // 
            // dateTimePickerTo
            // 
            dateTimePickerTo.Anchor = AnchorStyles.Top;
            dateTimePickerTo.Font = new Font("Microsoft Sans Serif", 9F);
            dateTimePickerTo.Location = new Point(466, 83);
            dateTimePickerTo.Name = "dateTimePickerTo";
            dateTimePickerTo.Size = new Size(253, 24);
            dateTimePickerTo.TabIndex = 56;
            // 
            // ExportInvoiceBtn
            // 
            ExportInvoiceBtn.Anchor = AnchorStyles.Top;
            ExportInvoiceBtn.BackColor = Color.RoyalBlue;
            ExportInvoiceBtn.Font = new Font("Microsoft Sans Serif", 10.2F, FontStyle.Bold);
            ExportInvoiceBtn.ForeColor = Color.Transparent;
            ExportInvoiceBtn.Location = new Point(1136, 77);
            ExportInvoiceBtn.Name = "ExportInvoiceBtn";
            ExportInvoiceBtn.Size = new Size(253, 40);
            ExportInvoiceBtn.TabIndex = 51;
            ExportInvoiceBtn.Text = "📤 Export Invoices";
            ExportInvoiceBtn.UseVisualStyleBackColor = false;
            ExportInvoiceBtn.Click += ExportInvoiceBtn_Click;
            // 
            // RegNumNumericUpDown
            // 
            RegNumNumericUpDown.Anchor = AnchorStyles.Top;
            RegNumNumericUpDown.Location = new Point(784, 85);
            RegNumNumericUpDown.Name = "RegNumNumericUpDown";
            RegNumNumericUpDown.Size = new Size(253, 27);
            RegNumNumericUpDown.TabIndex = 57;
            // 
            // label2
            // 
            label2.Anchor = AnchorStyles.Top;
            label2.AutoSize = true;
            label2.Font = new Font("Microsoft Sans Serif", 9F);
            label2.Location = new Point(784, 64);
            label2.Name = "label2";
            label2.Size = new Size(144, 18);
            label2.TabIndex = 53;
            label2.Text = "Registration Number";
            // 
            // ToDateLbl
            // 
            ToDateLbl.Anchor = AnchorStyles.Top;
            ToDateLbl.AutoSize = true;
            ToDateLbl.Font = new Font("Microsoft Sans Serif", 9F);
            ToDateLbl.Location = new Point(466, 62);
            ToDateLbl.Name = "ToDateLbl";
            ToDateLbl.Size = new Size(61, 18);
            ToDateLbl.TabIndex = 54;
            ToDateLbl.Text = "To Date";
            // 
            // ExportInvoiceForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1425, 662);
            Controls.Add(lblExportStatus);
            Controls.Add(progressBarExport);
            Controls.Add(ExportInvoiceBtn);
            Controls.Add(dateTimePickerTo);
            Controls.Add(ToDateLbl);
            Controls.Add(RegNumNumericUpDown);
            Controls.Add(label2);
            Controls.Add(dateTimePickerFrom);
            Controls.Add(DateFromLbl);
            Controls.Add(label1);
            Name = "ExportInvoiceForm";
            Text = "ExportInvoiceForm";
            Load += ExportInvoiceForm_Load;
            ((System.ComponentModel.ISupportInitialize)RegNumNumericUpDown).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private DateTimePicker dateTimePickerFrom;
        private Label lblExportStatus;
        private Label DateFromLbl;
        private ProgressBar progressBarExport;
        private DateTimePicker dateTimePickerTo;
        private Button ExportInvoiceBtn;
        private NumericUpDown RegNumNumericUpDown;
        private Label label2;
        private Label ToDateLbl;
    }
}