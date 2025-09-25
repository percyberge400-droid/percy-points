using Microsoft.AspNetCore.Http.Extensions;
using POSPRA.Application.Services;
using POSPRA.Application.Services.LiveService;
using POSPRA.DTOs.InvoiceDtos;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Windows.Forms;            
using POSPRA.DTOs;
using ClosedXML.Excel;
using System.IO;

namespace POSPRA_WinFormsUI.Forms
{
    public partial class ExportInvoiceForm : Form
    {
        private readonly ILiveService _liveService;
        //private ProgressBar progressBarExport;
        public ExportInvoiceForm(ILiveService liveService)
        {
            InitializeComponent();
            _liveService = liveService ?? throw new ArgumentNullException(nameof(liveService));

            dateTimePickerTo.MaxDate = DateTime.Today;
            dateTimePickerFrom.MaxDate = DateTime.Today;

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

            return true; //  valid
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
           
            if (!ValidateDateRange())
            {
                lblExportStatus.Text = "❌ Invalid date range. Please select a range within 1 month.";
                lblExportStatus.ForeColor = Color.Red;
                return; // stop if invalid
            }


            
            try
            {
                progressBarExport.Visible = true;
                progressBarExport.Style = ProgressBarStyle.Marquee; // continuous style while working
                ExportInvoiceBtn.Enabled = false; // disable button to prevent double click

                await Task.Delay(100);
                progressBarExport.Refresh();

                ExportInvoiceBtn.Enabled = false;
                progressBarExport.Visible = true;
                progressBarExport.Style = ProgressBarStyle.Marquee;


                // Read POSID from App.config
                var posId = 0;
                _ = int.TryParse(ConfigurationManager.AppSettings["Username"], out posId);
                // Build DTO (POSID is auto-handled in service)
                    var filter = new InvoiceFilterDto
                {
                    PosId = posId,
                    FromDate = dateTimePickerFrom.Value.Date,
                    ToDate = dateTimePickerTo.Value.Date
                };

                // ✅ Call the service method    
                var response = await _liveService.GetInvoicesCsvAsync(filter);



                // ✅ Handle the response
                if (response == null)
                {
                    lblExportStatus.Text = "❌ No response from service.";
                    lblExportStatus.ForeColor = Color.Red;
                }
                else if (string.IsNullOrWhiteSpace(response.Data))
                {
                    lblExportStatus.Text = "⚠ No invoices found for the selected date range.";
                    lblExportStatus.ForeColor = Color.Orange;
                }
                else
                {
                    // Split response into lines
                    //var lines = response.Data.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                    var lines = response.Data.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                    

                    // ✅ Check if only header is present
                    if (lines.Length <= 1)
                    {
                        lblExportStatus.Text = "⚠ No invoices found for the selected date range.";
                        lblExportStatus.ForeColor = Color.Orange;
                        return; // exit here, don’t open SaveFileDialog
                    }

                    //  if actual rows exist
                    using (SaveFileDialog saveFileDialog = new SaveFileDialog())
                    {
                        saveFileDialog.Filter = "Excel Workbook (*.xlsx)|*.xlsx";
                        saveFileDialog.Title = "Save Invoices Excel";
                        saveFileDialog.FileName = "Invoices.xlsx";

                        if (saveFileDialog.ShowDialog() == DialogResult.OK)
                        {
                            try
                            {
                                string path = saveFileDialog.FileName;

                                using (var workbook = new XLWorkbook())
                                {
                                    var worksheet = workbook.Worksheets.Add("Invoices");

                                    for (int i = 0; i < lines.Length; i++)
                                    {
                                        var values = lines[i].Split(',');
                                        for (int j = 0; j < values.Length; j++)
                                        {
                                            worksheet.Cell(i + 1, j + 1).Value = values[j].Trim();
                                        }
                                    }

                                    workbook.SaveAs(path);
                                }

                                lblExportStatus.Text = $"✅ Invoices exported successfully to:\n{path}";
                                lblExportStatus.ForeColor = Color.Green;
                            }
                            catch (Exception ex)
                            {
                                lblExportStatus.Text = $"❌ Error saving file: {ex.Message}";
                                lblExportStatus.ForeColor = Color.Red;
                            }
                        }
                        else
                        {
                            lblExportStatus.Text = "⚠ Export canceled by user.";
                            lblExportStatus.ForeColor = Color.Orange;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                lblExportStatus.Text = $"❌ Error: {ex.Message}";
                lblExportStatus.ForeColor = Color.Red;
            }
            finally
            {
                progressBarExport.Visible = false;
                ExportInvoiceBtn.Enabled = true;
            }



           

            //lblExportStatus.Text =
            //    $"Invoices data from {dateTimePickerFrom.Value:dd-MMM-yyyy} to {dateTimePickerTo.Value:dd-MMM-yyyy} exported successfully!";
            //lblExportStatus.ForeColor = Color.Green;
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
