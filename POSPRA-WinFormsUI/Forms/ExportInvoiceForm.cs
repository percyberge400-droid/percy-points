using ClosedXML.Excel;
using DocumentFormat.OpenXml.Wordprocessing;
using Pos.Application.DTOs;
using Pos.Application.DTOs.InvoiceDtos;
using Pos.Application.DTOs.LogDTOs;
using Pos.Application.Services.LiveService;
using Pos.Application.Services.LogService;
using Pos.Application.Utility;
using POSPRA.SecurityEncryption;
using POSPRA_WinFormsUI.AlertClasses;
using System.Configuration;
using System.Net.Http.Json;
using System.Runtime.InteropServices;

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
        private readonly string environment;
        private readonly HttpClient _httpClient;


        public ExportInvoiceForm(ILiveService liveService, ILogService logService)
        {
            InitializeComponent();

            _liveService = liveService ?? throw new ArgumentNullException(nameof(liveService));
            _logService = logService ?? throw new ArgumentNullException(nameof(logService));

            environment = ConfigurationManager.AppSettings["Environment"];

            // --- Date setup ---
            dateTimePickerTo.MaxDate = DateTime.Today;
            dateTimePickerFrom.MaxDate = DateTime.Today;
            dateTimePickerFrom.Format = DateTimePickerFormat.Custom;
            dateTimePickerTo.Format = DateTimePickerFormat.Custom;
            dateTimePickerFrom.CustomFormat = "dd MMM yyyy";
            dateTimePickerTo.CustomFormat = "dd MMM yyyy";

            // --- Button hover styling ---
            ExportInvoiceBtn.MouseEnter += (s, e) => ExportInvoiceBtn.BackColor = System.Drawing.Color.FromArgb(60, 179, 113);
            ExportInvoiceBtn.MouseLeave += (s, e) => ExportInvoiceBtn.BackColor = System.Drawing.Color.MediumSeaGreen;
            ExportInvoiceBtn.Click += async (s, e) => await ExportInvoiceBtn_ClickAsync(s, e);

            // --- Rounded panel ---
            panelPending.Region = Region.FromHrgn(
                CreateRoundRectRgn(0, 0, panelPending.Width, panelPending.Height, 20, 20));

            _httpClient = new HttpClient();
        }

        private async Task CreateLog(string message, string type)
        {
            await _logService.CreateLogAsync(new CreateLogDto { Message = message, Type = type });
        }

        private bool ValidateDateRange()
        {
            DateTime from = dateTimePickerFrom.Value.Date;
            DateTime to = dateTimePickerTo.Value.Date;

            if (to > DateTime.Today)
            {
                lblExportStatus.Text = "❌ To date cannot exceed today's date.";
                lblExportStatus.ForeColor = System.Drawing.Color.Red;
                return false;
            }

            if (to < from)
            {
                lblExportStatus.Text = "❌ End date cannot be earlier than start date.";
                lblExportStatus.ForeColor = System.Drawing.Color.Red;
                return false;
            }

            if ((to - from).TotalDays > 31)
            {
                lblExportStatus.Text = "❌ Date range cannot exceed one month.";
                lblExportStatus.ForeColor = System.Drawing.Color.Red;
                return false;
            }

            return true;
        }

        private void dateTimePickerFrom_ValueChanged(object sender, EventArgs e) => ValidateDateRange();
        private void dateTimePickerTo_ValueChanged(object sender, EventArgs e) => ValidateDateRange();

        private async Task ExportInvoiceBtn_ClickAsync(object sender, EventArgs e)
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
                lblExportStatus.ForeColor = System.Drawing.Color.DimGray;

                await Task.Delay(100); // small delay for smooth UI

                int.TryParse(ConfigurationManager.AppSettings["Username"], out var posId);
                var DecriptedPOSID = ConfigurationManager.AppSettings["Username"];
                int EncriptedPOSID = Convert.ToInt32(AesEncryptionHelper.Decrypt(DecriptedPOSID));

                // Get selected environment from config
                string baseUrl = ConfigurationManager.AppSettings["BaseUrl"];
                // Construct the full URL for the export CSV API
                var fullUrl = $"{baseUrl}/{Endpoints.ExportCSV}";

                // Prepare the request body
                var requestBody = new InvoiceFilterDto
                {
                    PosId = EncriptedPOSID,
                    FromDate = dateTimePickerFrom.Value.Date,
                    ToDate = dateTimePickerTo.Value.Date
                };

                // Create JSON content
                var json = JsonContent.Create(requestBody);

                // Add environment as query parameter
                var urlWithEnv = $"{fullUrl}?environment={environment}";

                // Make the POST request
                var responseMessage = await _httpClient.PostAsync(urlWithEnv, json);
                // Handle null response
                if (responseMessage == null)
                {
                    await CreateLog("No response from service", AlertType.Error);
                    AlertManager.ShowError("No response from service.");
                    lblExportStatus.Text = "❌ No response from service.";
                    lblExportStatus.ForeColor = System.Drawing.Color.Red;
                    return;
                }

                // Read and deserialize the response
                var response = await responseMessage.Content.ReadFromJsonAsync<ApiResponse<string>>();

                // Handle empty or missing data
                if (string.IsNullOrWhiteSpace(response?.Data))
                {
                    lblExportStatus.Text = "⚠ No invoices found for the selected range.";
                    lblExportStatus.ForeColor = System.Drawing.Color.Orange;
                    AlertManager.ShowInfo("No invoices found for the selected date range.");
                    await CreateLog("No invoices found in selected range", AlertType.Info);
                    return;
                }

                if (response == null)
                {
                    await CreateLog("No response from service", AlertType.Error);
                    AlertManager.ShowError("No response from service.");
                    lblExportStatus.Text = "❌ No response from service.";
                    lblExportStatus.ForeColor = System.Drawing.Color.Red;
                    return;
                }

                var lines = response.Data.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                if (lines.Length <= 1)
                {
                    lblExportStatus.Text = "⚠ No invoice records to export.";
                    lblExportStatus.ForeColor = System.Drawing.Color.Orange;
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

                            // Map CSV column indexes (0-based) to custom headers
                            var columnMap = new (int Index, string Header)[]
                            {
                                (1, "Invoice Number"),         // FBRInvoiceNumber
                                (3, "USIN"),                   // USIN
                                (2, "POSID"),                  // POSID
                                (21, "Buyer NTN"),             // BuyerNTN
                                (23, "Buyer CNIC"),            // BuyerCNIC
                                (5, "Buyer Name"),             // BuyerName
                                (6, "Buyer Phone Number"),     // BuyerPhoneNumber
                                (7, "Total Sale Value"),       // TotalSaleValue
                                (8, "Total Quantity"),         // TotalQuantity
                                (9, "Total Tax Charged"),      // TotalTaxCharged
                                (10, "Discount"),              // Discount
                                (11, "Total Bill Amount"),     // TotalBillAmount
                                (12, "Payment Mode"),          // PaymentMode
                                (4, "Invoice Entry DateTime"), // EntryDate
                                (13, "Synced DateTime"),       // DateTime
                                (16, "Invoice Type"),          // InvoiceType
                                (17, "RefUSIN"),               // RefUSIN
                                (22, "Further Tax"),           // FurtherTax
                            };

                            int excelRow = 1; // Start at 1 for headers

                            for (int i = 0; i < lines.Length; i++)
                            {
                                var cols = lines[i].Split(',');

                                if (i == 0)
                                {
                                    // Write custom headers
                                    for (int j = 0; j < columnMap.Length; j++)
                                        sheet.Cell(excelRow, j + 1).Value = columnMap[j].Header;

                                    excelRow++; // Move to first data row
                                }
                                else
                                {
                                    // --- Skip rows without Invoice Number ---
                                    if (cols.Length <= 1 || string.IsNullOrWhiteSpace(cols[1]))
                                        continue;

                                    for (int j = 0; j < columnMap.Length; j++)
                                    {
                                        int index = columnMap[j].Index;
                                        if (index < cols.Length)
                                        {
                                            string value = cols[index].Trim();

                                            // --- Replace Payment Mode values ---
                                            if (columnMap[j].Header == "Payment Mode")
                                            {
                                                value = value switch
                                                {
                                                    "1" => "Card",
                                                    "2" => "Cash",
                                                    "3" => "Online",
                                                    _ => value
                                                };
                                            }

                                            // --- Replace Invoice Type values ---
                                            if (columnMap[j].Header == "Invoice Type")
                                            {
                                                value = value switch
                                                {
                                                    "1" => "New",
                                                    "2" => "Debit Invoice",
                                                    "3" => "Credit Invoice",
                                                    _ => value
                                                };
                                            }

                                            sheet.Cell(excelRow, j + 1).Value = value;
                                        }
                                    }

                                    excelRow++; // Increment only after writing a row
                                }
                            }

                            workbook.SaveAs(sfd.FileName);

                            await CreateLog("Invoices exported successfully", AlertType.Success);
                            AlertManager.ShowSuccess("Invoices exported successfully!");
                            lblExportStatus.Text = $"✅ Exported successfully:\n{sfd.FileName}";
                            lblExportStatus.ForeColor = System.Drawing.Color.Green;
                        }
                        catch (Exception ex)
                        {
                            await CreateLog($"Export failed: {ex.Message}", AlertType.Error);
                            AlertManager.ShowError("Export failed!");
                            lblExportStatus.Text = "❌ Export failed.";
                            lblExportStatus.ForeColor = System.Drawing.Color.Red;
                        }
                    }
                    else
                    {
                        await CreateLog("Export canceled by user", AlertType.Info);
                        lblExportStatus.Text = "⚠ Export canceled by user.";
                        lblExportStatus.ForeColor = System.Drawing.Color.Orange;
                    }
                }
            }
            catch (Exception ex)
            {
                await CreateLog($"Error exporting invoices: {ex.Message}", AlertType.Error);
                lblExportStatus.Text = $"❌ Error: {ex.Message}";
                lblExportStatus.ForeColor = System.Drawing.Color.Red;
            }
            finally
            {
                progressBarExport.Visible = false;
                progressBarExport.Style = ProgressBarStyle.Blocks;
                ExportInvoiceBtn.Enabled = true;
            }
        }

        private void ExportInvoiceForm_Load(object sender, EventArgs e) { }

        private void label2_Click(object sender, EventArgs e)
        {

        }
    }
}
