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
            dateTimePickerFrom = new DateTimePicker();
            DateFromLbl = new Label();
            dateTimePickerTo = new DateTimePicker();
            ExportInvoiceBtn = new Button();
            RegLbl = new Label();
            ToDateLbl = new Label();
            RegNoTxtBox = new TextBox();
            panelPending = new Panel();
            progressBarExport = new ProgressBar();
            label1 = new Label();
            panelPending.SuspendLayout();
            SuspendLayout();
            // lblExportStatus
            lblExportStatus = new Label();
            lblExportStatus.AutoSize = true;
            lblExportStatus.Font = new Font("Microsoft Sans Serif", 10F);
            lblExportStatus.ForeColor = Color.Green;
            lblExportStatus.Location = new Point(616, 150); // adjust location
            lblExportStatus.Name = "lblExportStatus";
            lblExportStatus.Size = new Size(300, 24);
            lblExportStatus.Text = ""; // start empty
            panelPending.Controls.Add(lblExportStatus);

            // 
            // dateTimePickerFrom
            // 
            dateTimePickerFrom.CalendarFont = new Font("Microsoft Sans Serif", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            dateTimePickerFrom.Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            dateTimePickerFrom.Format = DateTimePickerFormat.Short;
            dateTimePickerFrom.Location = new Point(616, 92);
            dateTimePickerFrom.MinDate = new DateTime(1947, 10, 10, 0, 0, 0, 0);
            dateTimePickerFrom.Name = "dateTimePickerFrom";
            dateTimePickerFrom.Size = new Size(156, 24);
            dateTimePickerFrom.TabIndex = 55;
            // 
            // DateFromLbl
            // 
            DateFromLbl.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            DateFromLbl.AutoSize = true;
            DateFromLbl.Font = new Font("Microsoft Sans Serif", 10.2F);
            DateFromLbl.Location = new Point(616, 69);
            DateFromLbl.Name = "DateFromLbl";
            DateFromLbl.Size = new Size(89, 20);
            DateFromLbl.TabIndex = 52;
            DateFromLbl.Text = "From Date";
            // 
            // dateTimePickerTo
            // 
            dateTimePickerTo.Font = new Font("Microsoft Sans Serif", 9F);
            dateTimePickerTo.Format = DateTimePickerFormat.Short;
            dateTimePickerTo.Location = new Point(841, 92);
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
            ExportInvoiceBtn.Location = new Point(616, 207);
            ExportInvoiceBtn.Name = "ExportInvoiceBtn";
            ExportInvoiceBtn.Size = new Size(381, 38);
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
            RegLbl.Location = new Point(731, 123);
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
            ToDateLbl.Location = new Point(844, 69);
            ToDateLbl.Name = "ToDateLbl";
            ToDateLbl.Size = new Size(69, 20);
            ToDateLbl.TabIndex = 54;
            ToDateLbl.Text = "To Date";
            // 
            // RegNoTxtBox
            // 
            RegNoTxtBox.Location = new Point(723, 146);
            RegNoTxtBox.Name = "RegNoTxtBox";
            RegNoTxtBox.Size = new Size(171, 24);
            RegNoTxtBox.TabIndex = 60;
            RegNoTxtBox.TextChanged += RegNoTxtBox_TextChanged;
            // 
            // panelPending
            // 
            panelPending.BackColor = Color.White;
            panelPending.Controls.Add(ToDateLbl);
            panelPending.Controls.Add(DateFromLbl);
            panelPending.Controls.Add(RegLbl);
            panelPending.Controls.Add(progressBarExport);
            panelPending.Controls.Add(RegNoTxtBox);
            panelPending.Controls.Add(dateTimePickerTo);
            panelPending.Controls.Add(dateTimePickerFrom);
            panelPending.Controls.Add(ExportInvoiceBtn);
            panelPending.Location = new Point(0, 60);
            panelPending.Margin = new Padding(8);
            panelPending.Name = "panelPending";
            panelPending.Padding = new Padding(11, 12, 11, 12);
            panelPending.Size = new Size(1597, 306);
            panelPending.TabIndex = 67;
            panelPending.Paint += panelPending_Paint;
            // 
            // progressBarExport
            // 
            progressBarExport.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            progressBarExport.ForeColor = Color.FromArgb(128, 255, 128);
            progressBarExport.Location = new Point(616, 176);
            progressBarExport.Name = "progressBarExport";
            progressBarExport.Size = new Size(381, 25);
            progressBarExport.TabIndex = 59;
            progressBarExport.Visible = false;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Microsoft Sans Serif", 16.2F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label1.Location = new Point(0, 20);
            label1.Name = "label1";
            label1.Size = new Size(265, 32);
            label1.TabIndex = 68;
            label1.Text = "EXPORT INVOICE";
            // 
            // ExportInvoiceForm
            // 
            AutoScaleDimensions = new SizeF(9F, 18F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1597, 547);
            Controls.Add(label1);
            Controls.Add(panelPending);
            Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            Name = "ExportInvoiceForm";
            Text = "ExportInvoiceForm";
            Load += ExportInvoiceForm_Load;
            panelPending.ResumeLayout(false);
            panelPending.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private DateTimePicker dateTimePickerFrom;
        private Label DateFromLbl;
        private DateTimePicker dateTimePickerTo;
        private Button ExportInvoiceBtn;
        private Label RegLbl;
        private Label ToDateLbl;
        private TextBox RegNoTxtBox;
        private Panel panelPending;
        private Label lblExportStatus;

        private ProgressBar progressBarExport;
        private Label label1;
    }
}