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
            label2 = new Label();
            ToDateLbl = new Label();
            textBox1 = new TextBox();
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
            dateTimePickerFrom.CalendarFont = new Font("Microsoft Sans Serif", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            dateTimePickerFrom.Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            dateTimePickerFrom.Format = DateTimePickerFormat.Short;
            dateTimePickerFrom.Location = new Point(222, 74);
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
            lblExportStatus.Location = new Point(236, 136);
            lblExportStatus.Name = "lblExportStatus";
            lblExportStatus.Size = new Size(0, 18);
            lblExportStatus.TabIndex = 50;
            // 
            // DateFromLbl
            // 
            DateFromLbl.AutoSize = true;
            DateFromLbl.Font = new Font("Microsoft Sans Serif", 9F);
            DateFromLbl.Location = new Point(222, 50);
            DateFromLbl.Name = "DateFromLbl";
            DateFromLbl.Size = new Size(79, 18);
            DateFromLbl.TabIndex = 52;
            DateFromLbl.Text = "From Date";
            // 
            // progressBarExport
            // 
            progressBarExport.ForeColor = Color.FromArgb(128, 255, 128);
            progressBarExport.Location = new Point(525, 201);
            progressBarExport.Name = "progressBarExport";
            progressBarExport.Size = new Size(300, 25);
            progressBarExport.TabIndex = 58;
            progressBarExport.Visible = false;
            // 
            // dateTimePickerTo
            // 
            dateTimePickerTo.Font = new Font("Microsoft Sans Serif", 9F);
            dateTimePickerTo.Format = DateTimePickerFormat.Short;
            dateTimePickerTo.Location = new Point(398, 74);
            dateTimePickerTo.Name = "dateTimePickerTo";
            dateTimePickerTo.Size = new Size(156, 24);
            dateTimePickerTo.TabIndex = 56;
            // 
            // ExportInvoiceBtn
            // 
            ExportInvoiceBtn.BackColor = Color.RoyalBlue;
            ExportInvoiceBtn.Font = new Font("Microsoft Sans Serif", 10.2F, FontStyle.Bold);
            ExportInvoiceBtn.ForeColor = Color.Transparent;
            ExportInvoiceBtn.Location = new Point(841, 65);
            ExportInvoiceBtn.Name = "ExportInvoiceBtn";
            ExportInvoiceBtn.Size = new Size(188, 43);
            ExportInvoiceBtn.TabIndex = 51;
            ExportInvoiceBtn.Text = "📤 Export Invoices";
            ExportInvoiceBtn.UseVisualStyleBackColor = false;
            ExportInvoiceBtn.Click += ExportInvoiceBtn_Click;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Microsoft Sans Serif", 9F);
            label2.Location = new Point(572, 50);
            label2.Name = "label2";
            label2.Size = new Size(144, 18);
            label2.TabIndex = 53;
            label2.Text = "Registration Number";
            // 
            // ToDateLbl
            // 
            ToDateLbl.AutoSize = true;
            ToDateLbl.Font = new Font("Microsoft Sans Serif", 9F);
            ToDateLbl.Location = new Point(398, 50);
            ToDateLbl.Name = "ToDateLbl";
            ToDateLbl.Size = new Size(61, 18);
            ToDateLbl.TabIndex = 54;
            ToDateLbl.Text = "To Date";
            // 
            // textBox1
            // 
            textBox1.Location = new Point(572, 74);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(253, 24);
            textBox1.TabIndex = 60;
            // 
            // ExportInvoiceForm
            // 
            AutoScaleDimensions = new SizeF(9F, 18F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1597, 547);
            Controls.Add(textBox1);
            Controls.Add(lblExportStatus);
            Controls.Add(progressBarExport);
            Controls.Add(ExportInvoiceBtn);
            Controls.Add(dateTimePickerTo);
            Controls.Add(ToDateLbl);
            Controls.Add(label2);
            Controls.Add(dateTimePickerFrom);
            Controls.Add(DateFromLbl);
            Controls.Add(label1);
            Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            Name = "ExportInvoiceForm";
            Text = "ExportInvoiceForm";
            Load += ExportInvoiceForm_Load;
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
        private Label label2;
        private Label ToDateLbl;
        private TextBox textBox1;
    }
}