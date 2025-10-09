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
        private readonly ILogService _logService;

        public ExportInvoiceForm(ILiveService liveService, ILogService logService)
        {
            InitializeComponent();

            _liveService = liveService ?? throw new ArgumentNullException(nameof(liveService));
            _logService = logService ?? throw new ArgumentNullException(nameof(logService));

            // --- Date setup ---
            dateTimePickerTo.MaxDate = DateTime.Today;
            dateTimePickerFrom.MaxDate = DateTime.Today;
            dateTimePickerFrom.Format = DateTimePickerFormat.Custom;
            dateTimePickerTo.Format = DateTimePickerFormat.Custom;
            dateTimePickerFrom.CustomFormat = "dd MMM yyyy";
            dateTimePickerTo.CustomFormat = "dd MMM yyyy";

            // --- Button hover styling ---
            ExportInvoiceBtn.MouseEnter += (s, e) => ExportInvoiceBtn.BackColor = Color.FromArgb(60, 179, 113);
            ExportInvoiceBtn.MouseLeave += (s, e) => ExportInvoiceBtn.BackColor = Color.MediumSeaGreen;

            // --- Rounded panel ---
            panelPending.Region = Region.FromHrgn(
                CreateRoundRectRgn(0, 0, panelPending.Width, panelPending.Height, 20, 20));
        }

        private async Task CreateLog(string message, string type)
        {
            await _logService.CreateLogAsync(new Logs { Message = message, Type = type });
        }

        private bool ValidateDateRange()
        {
            DateTime from = dateTimePickerFrom.Value.Date;
            DateTime to = dateTimePickerTo.Value.Date;

            if (to > DateTime.Today)
            {
                lblExportStatus.Text = "❌ To date cannot exceed today's date.";
                lblExportStatus.ForeColor = Color.Red;
                return false;
            }

            if (to < from)
            {
                lblExportStatus.Text = "❌ End date cannot be earlier than start date.";
                lblExportStatus.ForeColor = Color.Red;
                return false;
            }

            if ((to - from).TotalDays > 31)
            {
                lblExportStatus.Text = "❌ Date range cannot exceed one month.";
                lblExportStatus.ForeColor = Color.Red;
                return false;
            }

            return true;
        }

        private void dateTimePickerFrom_ValueChanged(object sender, EventArgs e) => ValidateDateRange();
        private void dateTimePickerTo_ValueChanged(object sender, EventArgs e) => ValidateDateRange();

        private async void ExportInvoiceBtn_Click(object sender, EventArgs e)
        {
            if (!ValidateDateRange())
            {
                AlertManager.ShowWarning(" ❌ Invalid date range!");
                return;
            }

            try
            {
                ExportInvoiceBtn.Enabled = false;
                progressBarExport.Visible = true;
                progressBarExport.Style = ProgressBarStyle.Marquee;
                lblExportStatus.Text = "⏳ Exporting invoices, please wait...";
                lblExportStatus.ForeColor = Color.DimGray;

                await Task.Delay(100); // small delay for smooth UI

                int.TryParse(ConfigurationManager.AppSettings["Username"], out var posId);

                var filter = new InvoiceFilterDto
                {
                    PosId = posId,
                    FromDate = dateTimePickerFrom.Value.Date,
                    ToDate = dateTimePickerTo.Value.Date
                };

                var response = await _liveService.GetInvoicesCsvAsync(filter);

                if (response == null)
                {
                    await CreateLog("No response from service", AlertType.Error);
                    AlertManager.ShowError("No response from service.");
                    lblExportStatus.Text = "❌ No response from service.";
                    lblExportStatus.ForeColor = Color.Red;
                    return;
                }

                if (string.IsNullOrWhiteSpace(response.Data))
                {
                    lblExportStatus.Text = "⚠ No invoices found for the selected range.";
                    lblExportStatus.ForeColor = Color.Orange;
                    AlertManager.ShowInfo("No invoices found for the selected date range.");
                    await CreateLog("No invoices found in selected range", AlertType.Info);
                    return;
                }

                var lines = response.Data.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                if (lines.Length <= 1)
                {
                    lblExportStatus.Text = "⚠ No invoice records to export.";
                    lblExportStatus.ForeColor = Color.Orange;
                    AlertManager.ShowInfo("No invoice records found.");
                    return;
                }

                // --- Auto filename ---
                using (var sfd = new SaveFileDialog())
                {
                    sfd.Filter = "Excel Workbook (*.xlsx)|*.xlsx";
                    sfd.Title = "Save Exported Invoices";
                    sfd.FileName = $"Invoices_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

                    if (sfd.ShowDialog() == DialogResult.OK)
                    {
                        try
                        {
                            using var workbook = new XLWorkbook();
                            var sheet = workbook.Worksheets.Add("Invoices");

                            for (int i = 0; i < lines.Length; i++)
                            {
                                var cols = lines[i].Split(',');
                                for (int j = 0; j < cols.Length; j++)
                                    sheet.Cell(i + 1, j + 1).Value = cols[j].Trim();
                            }

                            workbook.SaveAs(sfd.FileName);

                            await CreateLog("Invoices exported successfully", AlertType.Success);
                            AlertManager.ShowSuccess("Invoices exported successfully!");

                            lblExportStatus.Text = $"✅ Exported successfully:\n{sfd.FileName}";
                            lblExportStatus.ForeColor = Color.Green;
                        }
                        catch (Exception ex)
                        {
                            await CreateLog($"Error saving file: {ex.Message}", AlertType.Error);
                            AlertManager.ShowError($"Error saving file: {ex.Message}");
                            lblExportStatus.Text = $"❌ Error saving file: {ex.Message}";
                            lblExportStatus.ForeColor = Color.Red;
                        }
                    }
                    else
                    {
                        await CreateLog("Export canceled by user", AlertType.Info);
                        lblExportStatus.Text = "⚠ Export canceled by user.";
                        lblExportStatus.ForeColor = Color.Orange;
                    }
                }
            }
            catch (Exception ex)
            {
                await CreateLog($"Error exporting invoices: {ex.Message}", AlertType.Error);
                lblExportStatus.Text = $"❌ Error: {ex.Message}";
                lblExportStatus.ForeColor = Color.Red;
            }
            finally
            {
                progressBarExport.Visible = false;
                ExportInvoiceBtn.Enabled = true;
            }
        }

        private void ExportInvoiceForm_Load(object sender, EventArgs e) { }
    }
}
