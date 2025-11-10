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

        private void InitializeComponent()
        {
            label1 = new Label();
            lblExportStatus = new Label();
            panelPending = new Panel();
            ToDateLbl = new Label();
            DateFromLbl = new Label();
            progressBarExport = new ProgressBar();
            dateTimePickerTo = new DateTimePicker();
            dateTimePickerFrom = new DateTimePicker();
            ExportInvoiceBtn = new Button();
            panelPending.SuspendLayout();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Dock = DockStyle.Top;
            label1.Font = new Font("Segoe UI Semibold", 18F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label1.ForeColor = Color.FromArgb(64, 64, 64);
            label1.Location = new Point(0, 0);
            label1.Name = "label1";
            label1.Size = new Size(271, 41);
            label1.TabIndex = 0;
            label1.Text = "EXPORT INVOICES";
            label1.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblExportStatus
            // 
            lblExportStatus.Font = new Font("Segoe UI", 9F, FontStyle.Italic);
            lblExportStatus.ForeColor = Color.FromArgb(100, 100, 100);
            lblExportStatus.Location = new Point(60, 186);
            lblExportStatus.Name = "lblExportStatus";
            lblExportStatus.Size = new Size(500, 40);
            lblExportStatus.TabIndex = 0;
            lblExportStatus.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // panelPending
            // 
            panelPending.Anchor = AnchorStyles.None;
            panelPending.BackColor = Color.Gainsboro;
            panelPending.Controls.Add(lblExportStatus);
            panelPending.Controls.Add(ToDateLbl);
            panelPending.Controls.Add(DateFromLbl);
            panelPending.Controls.Add(progressBarExport);
            panelPending.Controls.Add(dateTimePickerTo);
            panelPending.Controls.Add(dateTimePickerFrom);
            panelPending.Controls.Add(ExportInvoiceBtn);
            panelPending.Location = new Point(203, 148);
            panelPending.Name = "panelPending";
            panelPending.Padding = new Padding(20);
            panelPending.Size = new Size(617, 261);
            panelPending.TabIndex = 1;
            // 
            // ToDateLbl
            // 
            ToDateLbl.Font = new Font("Segoe UI", 10F);
            ToDateLbl.Location = new Point(340, 41);
            ToDateLbl.Name = "ToDateLbl";
            ToDateLbl.Size = new Size(71, 23);
            ToDateLbl.TabIndex = 1;
            ToDateLbl.Text = "To Date";
            // 
            // DateFromLbl
            // 
            DateFromLbl.Font = new Font("Segoe UI", 10F);
            DateFromLbl.Location = new Point(60, 41);
            DateFromLbl.Name = "DateFromLbl";
            DateFromLbl.Size = new Size(93, 23);
            DateFromLbl.TabIndex = 2;
            DateFromLbl.Text = "From Date";
            // 
            // progressBarExport
            // 
            progressBarExport.ForeColor = Color.MediumSeaGreen;
            progressBarExport.Location = new Point(60, 156);
            progressBarExport.MarqueeAnimationSpeed = 40;
            progressBarExport.Name = "progressBarExport";
            progressBarExport.Size = new Size(500, 20);
            progressBarExport.Style = ProgressBarStyle.Marquee;
            progressBarExport.TabIndex = 4;
            progressBarExport.Visible = false;
            // 
            // dateTimePickerTo
            // 
            dateTimePickerTo.CustomFormat = "dd MMM yyyy";
            dateTimePickerTo.Font = new Font("Segoe UI", 9.5F);
            dateTimePickerTo.Format = DateTimePickerFormat.Custom;
            dateTimePickerTo.Location = new Point(417, 39);
            dateTimePickerTo.Name = "dateTimePickerTo";
            dateTimePickerTo.Size = new Size(143, 29);
            dateTimePickerTo.TabIndex = 6;
            // 
            // dateTimePickerFrom
            // 
            dateTimePickerFrom.CalendarForeColor = Color.Black;
            dateTimePickerFrom.CalendarMonthBackground = Color.White;
            dateTimePickerFrom.CustomFormat = "dd MMM yyyy";
            dateTimePickerFrom.Font = new Font("Segoe UI", 9.5F);
            dateTimePickerFrom.Format = DateTimePickerFormat.Custom;
            dateTimePickerFrom.Location = new Point(159, 39);
            dateTimePickerFrom.Name = "dateTimePickerFrom";
            dateTimePickerFrom.Size = new Size(143, 29);
            dateTimePickerFrom.TabIndex = 7;
            // 
            // ExportInvoiceBtn
            // 
            ExportInvoiceBtn.BackColor = Color.MediumSeaGreen;
            ExportInvoiceBtn.Cursor = Cursors.Hand;
            ExportInvoiceBtn.FlatAppearance.BorderSize = 0;
            ExportInvoiceBtn.FlatStyle = FlatStyle.Flat;
            ExportInvoiceBtn.Font = new Font("Segoe UI Semibold", 11F, FontStyle.Bold);
            ExportInvoiceBtn.ForeColor = Color.White;
            ExportInvoiceBtn.Location = new Point(60, 96);
            ExportInvoiceBtn.Name = "ExportInvoiceBtn";
            ExportInvoiceBtn.Size = new Size(500, 40);
            ExportInvoiceBtn.TabIndex = 8;
            ExportInvoiceBtn.Text = "📤  Export Invoices";
            ExportInvoiceBtn.UseVisualStyleBackColor = false;
            // 
            // ExportInvoiceForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(245, 247, 250);
            ClientSize = new Size(980, 540);
            Controls.Add(label1);
            Controls.Add(panelPending);
            Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Name = "ExportInvoiceForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "📤 Export Invoices";
            panelPending.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }
        private Label label1;
        private Label lblExportStatus;
        private Panel panelPending;
        private Label ToDateLbl;
        private Label DateFromLbl;
        private Label RegLbl;
        private ProgressBar progressBarExport;
        private TextBox RegNoTxtBox;
        private DateTimePicker dateTimePickerTo;
        private DateTimePicker dateTimePickerFrom;
        private Button ExportInvoiceBtn;
    }
}