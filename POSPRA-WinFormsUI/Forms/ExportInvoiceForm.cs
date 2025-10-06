using ClosedXML.Excel;
using POSPRA.Application.Services.LiveService;
using POSPRA.Application.Services.LogService;
using POSPRA.Domain.Entities;
using POSPRA.DTOs.InvoiceDtos;
using POSPRA_WinFormsUI.AlertClasses;
using System.Configuration;
using System.Runtime.InteropServices;
using AlertType = POSPRA.Application.Utility.AlertType;

namespace POSPRA_WinFormsUI.Forms
{

    public partial class ExportInvoiceForm : Form
    {
        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn(
    int nLeftRect, int nTopRect, int nRightRect, int nBottomRect,
    int nWidthEllipse, int nHeightEllipse);

        private readonly ILiveService _liveService;
        //private ProgressBar progressBarExport;

        //logs
        private readonly ILogService _logService;
        public ExportInvoiceForm(ILiveService liveService, ILogService logService)
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
            _logService = logService;
            //this.Controls.Add(progressBarExport);
            // 🌿 Smooth hover effect for the Export button
            ExportInvoiceBtn.MouseEnter += (s, e) =>
                ExportInvoiceBtn.BackColor = Color.FromArgb(60, 179, 113); // lighter green
            ExportInvoiceBtn.MouseLeave += (s, e) =>
                ExportInvoiceBtn.BackColor = Color.MediumSeaGreen; // original color
            panelPending.Region = Region.FromHrgn(
        CreateRoundRectRgn(0, 0, panelPending.Width, panelPending.Height, 20, 20));
            panelPending.Region = Region.FromHrgn(
                    CreateRoundRectRgn(0, 0, panelPending.Width, panelPending.Height, 20, 20));
        }


        private async Task CreateLog(string message, string type)
        {
            var log = new Logs
            {
                Message = message,   // pass any message
                Type = type,         // comes from AlertType constants

            };

            await _logService.LogAsync(log);
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
                AlertManager.ShowWarning(" ❌ Invalid date range!");
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
                    _ = CreateLog("NO Response from service", AlertType.Error);
                    AlertManager.ShowWarning("⚠ No response from service!");

                    lblExportStatus.Text = "❌ No response from service!";
                    lblExportStatus.ForeColor = Color.Red;
                }
                else if (string.IsNullOrWhiteSpace(response.Data))
                {
                    lblExportStatus.Text = "⚠ No invoices found for the selected date range.";
                    lblExportStatus.ForeColor = Color.Orange;
                    WindowsLocalAppNotification.Show("Invoices Not Found", " No invoices found for the selected date range.");
                    AlertManager.ShowInfo(" No invoices found for the selected date range.");
                    _ = CreateLog("Invoices Not Found within Selected date range", AlertType.Info);
                }
                else
                {
                    // Split response into lines
                    //var lines = response.Data.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                    var lines = response.Data.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);


                    // ✅ Check if only header is present
                    if (lines.Length <= 1)
                    {
                        //WindowsLocalAppNotification.Show("Invoices Not Found", " No invoices found for the selected date range.");
                        AlertManager.ShowInfo(" No invoices found for the selected date range.");
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
                                _ = CreateLog("Invoices exported successfully", AlertType.Success);
                                WindowsLocalAppNotification.Show("Success.", "Invoices exported successfully");
                                AlertManager.ShowSuccess($"Invoices exported successfully");
                                lblExportStatus.Text = $"✅ Invoices exported successfully to:\n{path}";
                                lblExportStatus.ForeColor = Color.Green;
                            }
                            catch (Exception ex)
                            {
                                //WindowsLocalAppNotification.Show("Invoices Error", $"Error Saving invoices: {ex.Message}");
                                AlertManager.ShowError($"Error Saving invoices: {ex.Message}");
                                lblExportStatus.Text = $"❌ Error saving file: {ex.Message}";
                                lblExportStatus.ForeColor = Color.Red;
                            }
                        }
                        else
                        {
                            //WindowsLocalAppNotification.Show("Canceled.", "⚠ Invoices Export canceled by user");
                            _ = CreateLog("Invoices exported canceled by User", AlertType.Info);
                            AlertManager.ShowError($"Invoices exported canceled by User");
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

        private void RegNoTxtBox_TextChanged(object sender, EventArgs e)
        {

        }

        private void panelPending_Paint(object sender, PaintEventArgs e)
        {

        }
    }

}
