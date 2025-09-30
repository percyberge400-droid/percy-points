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
            RegLbl = new Label();
            ToDateLbl = new Label();
            RegNoTxtBox = new TextBox();
            tableLayoutPanel1 = new TableLayoutPanel();
            tableLayoutPanel5 = new TableLayoutPanel();
            tableLayoutPanel4 = new TableLayoutPanel();
            tableLayoutPanel3 = new TableLayoutPanel();
            tableLayoutPanel2 = new TableLayoutPanel();
            tableLayoutPanel6 = new TableLayoutPanel();
            tableLayoutPanel1.SuspendLayout();
            tableLayoutPanel5.SuspendLayout();
            tableLayoutPanel4.SuspendLayout();
            tableLayoutPanel3.SuspendLayout();
            tableLayoutPanel2.SuspendLayout();
            tableLayoutPanel6.SuspendLayout();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Microsoft Sans Serif", 16.2F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label1.Location = new Point(3, 0);
            label1.Name = "label1";
            label1.Size = new Size(126, 64);
            label1.TabIndex = 59;
            label1.Text = "Export Invoices";
            // 
            // dateTimePickerFrom
            // 
            dateTimePickerFrom.CalendarFont = new Font("Microsoft Sans Serif", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            dateTimePickerFrom.Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            dateTimePickerFrom.Format = DateTimePickerFormat.Short;
            dateTimePickerFrom.Location = new Point(3, 76);
            dateTimePickerFrom.MinDate = new DateTime(1947, 10, 10, 0, 0, 0, 0);
            dateTimePickerFrom.Name = "dateTimePickerFrom";
            dateTimePickerFrom.Size = new Size(156, 24);
            dateTimePickerFrom.TabIndex = 55;
            // 
            // lblExportStatus
            // 
            lblExportStatus.AutoSize = true;
            lblExportStatus.Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Bold);
            lblExportStatus.ForeColor = Color.Green;
            lblExportStatus.Location = new Point(3, 0);
            lblExportStatus.Name = "lblExportStatus";
            lblExportStatus.Size = new Size(0, 18);
            lblExportStatus.TabIndex = 50;
            // 
            // DateFromLbl
            // 
            DateFromLbl.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            DateFromLbl.AutoSize = true;
            DateFromLbl.Font = new Font("Microsoft Sans Serif", 10.2F);
            DateFromLbl.Location = new Point(3, 53);
            DateFromLbl.Name = "DateFromLbl";
            DateFromLbl.Size = new Size(89, 20);
            DateFromLbl.TabIndex = 52;
            DateFromLbl.Text = "From Date";
            // 
            // progressBarExport
            // 
            progressBarExport.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            progressBarExport.ForeColor = Color.FromArgb(128, 255, 128);
            progressBarExport.Location = new Point(390, 3);
            progressBarExport.Name = "progressBarExport";
            progressBarExport.Size = new Size(381, 25);
            progressBarExport.TabIndex = 58;
            progressBarExport.Visible = false;
            // 
            // dateTimePickerTo
            // 
            dateTimePickerTo.Font = new Font("Microsoft Sans Serif", 9F);
            dateTimePickerTo.Format = DateTimePickerFormat.Short;
            dateTimePickerTo.Location = new Point(3, 76);
            dateTimePickerTo.Name = "dateTimePickerTo";
            dateTimePickerTo.Size = new Size(156, 24);
            dateTimePickerTo.TabIndex = 56;
            // 
            // ExportInvoiceBtn
            // 
            ExportInvoiceBtn.BackColor = Color.MediumSeaGreen;
            ExportInvoiceBtn.FlatStyle = FlatStyle.Flat;
            ExportInvoiceBtn.Font = new Font("Microsoft Sans Serif", 10.2F, FontStyle.Bold);
            ExportInvoiceBtn.ForeColor = Color.Transparent;
            ExportInvoiceBtn.Location = new Point(3, 76);
            ExportInvoiceBtn.Name = "ExportInvoiceBtn";
            ExportInvoiceBtn.Size = new Size(175, 50);
            ExportInvoiceBtn.TabIndex = 51;
            ExportInvoiceBtn.Text = "📤 Export Invoices";
            ExportInvoiceBtn.UseVisualStyleBackColor = false;
            ExportInvoiceBtn.AutoSizeChanged += ExportInvoiceBtn_Click;
            ExportInvoiceBtn.Click += ExportInvoiceBtn_Click;
            // 
            // RegLbl
            // 
            RegLbl.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            RegLbl.AutoSize = true;
            RegLbl.Font = new Font("Microsoft Sans Serif", 10.2F);
            RegLbl.Location = new Point(3, 53);
            RegLbl.Name = "RegLbl";
            RegLbl.Size = new Size(163, 20);
            RegLbl.TabIndex = 53;
            RegLbl.Text = "Registration Number";
            // 
            // ToDateLbl
            // 
            ToDateLbl.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            ToDateLbl.AutoSize = true;
            ToDateLbl.Font = new Font("Microsoft Sans Serif", 10.2F);
            ToDateLbl.Location = new Point(3, 53);
            ToDateLbl.Name = "ToDateLbl";
            ToDateLbl.Size = new Size(69, 20);
            ToDateLbl.TabIndex = 54;
            ToDateLbl.Text = "To Date";
            // 
            // RegNoTxtBox
            // 
            RegNoTxtBox.Location = new Point(3, 76);
            RegNoTxtBox.Name = "RegNoTxtBox";
            RegNoTxtBox.Size = new Size(161, 24);
            RegNoTxtBox.TabIndex = 60;
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 6;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 12.09058F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 12.0905848F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 12.0905848F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 12.0905848F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 12.343276F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 39.2943954F));
            tableLayoutPanel1.Controls.Add(tableLayoutPanel5, 3, 0);
            tableLayoutPanel1.Controls.Add(tableLayoutPanel4, 2, 0);
            tableLayoutPanel1.Controls.Add(tableLayoutPanel3, 1, 0);
            tableLayoutPanel1.Controls.Add(label1, 0, 0);
            tableLayoutPanel1.Controls.Add(tableLayoutPanel6, 4, 0);
            tableLayoutPanel1.Dock = DockStyle.Top;
            tableLayoutPanel1.Location = new Point(0, 0);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 1;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            tableLayoutPanel1.Size = new Size(1597, 153);
            tableLayoutPanel1.TabIndex = 61;
            // 
            // tableLayoutPanel5
            // 
            tableLayoutPanel5.ColumnCount = 1;
            tableLayoutPanel5.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel5.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 20F));
            tableLayoutPanel5.Controls.Add(RegLbl, 0, 0);
            tableLayoutPanel5.Controls.Add(RegNoTxtBox, 0, 1);
            tableLayoutPanel5.Location = new Point(582, 3);
            tableLayoutPanel5.Name = "tableLayoutPanel5";
            tableLayoutPanel5.RowCount = 2;
            tableLayoutPanel5.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tableLayoutPanel5.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tableLayoutPanel5.Size = new Size(187, 147);
            tableLayoutPanel5.TabIndex = 65;
            // 
            // tableLayoutPanel4
            // 
            tableLayoutPanel4.ColumnCount = 1;
            tableLayoutPanel4.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel4.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 20F));
            tableLayoutPanel4.Controls.Add(ToDateLbl, 0, 0);
            tableLayoutPanel4.Controls.Add(dateTimePickerTo, 0, 1);
            tableLayoutPanel4.Location = new Point(389, 3);
            tableLayoutPanel4.Name = "tableLayoutPanel4";
            tableLayoutPanel4.RowCount = 2;
            tableLayoutPanel4.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tableLayoutPanel4.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tableLayoutPanel4.Size = new Size(187, 147);
            tableLayoutPanel4.TabIndex = 64;
            // 
            // tableLayoutPanel3
            // 
            tableLayoutPanel3.ColumnCount = 1;
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 20F));
            tableLayoutPanel3.Controls.Add(DateFromLbl, 0, 0);
            tableLayoutPanel3.Controls.Add(dateTimePickerFrom, 0, 1);
            tableLayoutPanel3.Location = new Point(196, 3);
            tableLayoutPanel3.Name = "tableLayoutPanel3";
            tableLayoutPanel3.RowCount = 2;
            tableLayoutPanel3.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tableLayoutPanel3.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tableLayoutPanel3.Size = new Size(187, 147);
            tableLayoutPanel3.TabIndex = 63;
            // 
            // tableLayoutPanel2
            // 
            tableLayoutPanel2.ColumnCount = 2;
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanel2.Controls.Add(lblExportStatus, 0, 0);
            tableLayoutPanel2.Controls.Add(progressBarExport, 1, 0);
            tableLayoutPanel2.Location = new Point(196, 169);
            tableLayoutPanel2.Name = "tableLayoutPanel2";
            tableLayoutPanel2.RowCount = 1;
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel2.Size = new Size(774, 130);
            tableLayoutPanel2.TabIndex = 62;
            // 
            // tableLayoutPanel6
            // 
            tableLayoutPanel6.ColumnCount = 1;
            tableLayoutPanel6.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel6.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 20F));
            tableLayoutPanel6.Controls.Add(ExportInvoiceBtn, 0, 1);
            tableLayoutPanel6.Dock = DockStyle.Fill;
            tableLayoutPanel6.Location = new Point(775, 3);
            tableLayoutPanel6.Name = "tableLayoutPanel6";
            tableLayoutPanel6.RowCount = 2;
            tableLayoutPanel6.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tableLayoutPanel6.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tableLayoutPanel6.Size = new Size(191, 147);
            tableLayoutPanel6.TabIndex = 66;
            // 
            // ExportInvoiceForm
            // 
            AutoScaleDimensions = new SizeF(9F, 18F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1597, 547);
            Controls.Add(tableLayoutPanel2);
            Controls.Add(tableLayoutPanel1);
            Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            Name = "ExportInvoiceForm";
            Text = "ExportInvoiceForm";
            Load += ExportInvoiceForm_Load;
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            tableLayoutPanel5.ResumeLayout(false);
            tableLayoutPanel5.PerformLayout();
            tableLayoutPanel4.ResumeLayout(false);
            tableLayoutPanel4.PerformLayout();
            tableLayoutPanel3.ResumeLayout(false);
            tableLayoutPanel3.PerformLayout();
            tableLayoutPanel2.ResumeLayout(false);
            tableLayoutPanel2.PerformLayout();
            tableLayoutPanel6.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private Label label1;
        private DateTimePicker dateTimePickerFrom;
        private Label lblExportStatus;
        private Label DateFromLbl;
        private ProgressBar progressBarExport;
        private DateTimePicker dateTimePickerTo;
        private Button ExportInvoiceBtn;
        private Label RegLbl;
        private Label ToDateLbl;
        private TextBox RegNoTxtBox;
        private TableLayoutPanel tableLayoutPanel1;
        private TableLayoutPanel tableLayoutPanel5;
        private TableLayoutPanel tableLayoutPanel4;
        private TableLayoutPanel tableLayoutPanel3;
        private TableLayoutPanel tableLayoutPanel2;
        private TableLayoutPanel tableLayoutPanel6;
    }
}