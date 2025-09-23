using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace POSPRA_WinFormsUI.Forms
{
    public partial class ExportInvoiceForm : Form
    {

        //private ProgressBar progressBarExport;
        public ExportInvoiceForm()
        {
            InitializeComponent();

            dateTimePickerTo.MaxDate = DateTime.Today;


            dateTimePickerFrom.Format = DateTimePickerFormat.Custom;
            dateTimePickerFrom.CustomFormat = "dd MMM yyyy";
            //dateTimePickerFrom.EnableAutoDropDown();
            dateTimePickerTo.Format = DateTimePickerFormat.Custom;
            dateTimePickerTo.CustomFormat = "dd MMM yyyy";
            //dateTimePickerTo.EnableAutoDropDown();
            //Progress Bar
            ProgressBar progressBarExport;
            //progressBarExport.Visible = false;

            progressBarExport = new ProgressBar();
            progressBarExport.Location = new Point(20, 70);
            progressBarExport.Size = new Size(300, 20);
            progressBarExport.Visible = false; // hidden by default
            //this.Controls.Add(progressBarExport);
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {

        }


        private bool ValidateDateRange()
        {
            DateTime from = dateTimePickerFrom.Value.Date;
            DateTime to = dateTimePickerTo.Value.Date;


            // ensure To <= Today
            if (to > DateTime.Today)
            {
                lblExportStatus.Text = "❌ To date cannot exceed today's date.";
                lblExportStatus.ForeColor = Color.Red;
                return false;
            }
            // ensure To >= From
            if (to < from)
            {
                lblExportStatus.Text = "❌ End date cannot be earlier than start date.";
                lblExportStatus.ForeColor = Color.Red;
                return false;
            }

            // ensure duration ≤ 31 days
            if ((to - from).TotalDays > 31)
            {
                lblExportStatus.Text = "❌ Date range cannot exceed one month.";
                lblExportStatus.ForeColor = Color.Red;
                return false;
            }

            return true; // ✅ valid
        }


        private void dateTimePickerFrom_ValueChanged(object sender, EventArgs e)
        {
            ValidateDateRange();
        }

        private void dateTimePickerTo_ValueChanged(object sender, EventArgs e)
        {
            ValidateDateRange();
        }

        private async void ExportInvoiceBtn_Click(object sender, EventArgs e)
        {
            // Step 1: Validation
            if (!ValidateDateRange())
            {
                lblExportStatus.Text = "❌ Invalid date range.";
                lblExportStatus.ForeColor = Color.Red;
                return; // stop if invalid
            }


            progressBarExport.Visible = true;
            progressBarExport.Style = ProgressBarStyle.Marquee; // continuous style while working
            ExportInvoiceBtn.Enabled = false; // disable button to prevent double click

            await Task.Delay(100);
            progressBarExport.Refresh();

            await Task.Delay(2000);
            // Step 4: Hide progress bar
            progressBarExport.Visible = false;
            ExportInvoiceBtn.Enabled = true;


            // Step 3: Simulate API/Export work
            //await Task.Run(() =>
            //{
            //    // simulate long task (e.g., API call)
            //    System.Threading.Thread.Sleep(3000);
            //});




            //MessageBox.Show(
            //    $"Invoice data from {dateTimePickerFrom.Value:dd-MM-yyyy} " +
            //    $"to {dateTimePickerTo.Value:dd-MM-yyyy} exported successfully."
            //    );

            lblExportStatus.Text =
                $"Invoices data from {dateTimePickerFrom.Value:dd-MMM-yyyy} to {dateTimePickerTo.Value:dd-MMM-yyyy} exported successfully!";
            lblExportStatus.ForeColor = Color.Green;
        }

        private void ExportInvoiceForm_Load(object sender, EventArgs e)
        {

        }

        private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }
    }

}
