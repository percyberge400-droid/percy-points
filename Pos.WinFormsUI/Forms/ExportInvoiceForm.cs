using ClosedXML.Excel;
using Pos.Application.DTOs;
using Pos.Application.DTOs.InvoiceDtos;
using Pos.Application.DTOs.LogDTOs;
using Pos.Application.DTOs.ReferenceDtos;
using Pos.Application.Services.LiveService;
using Pos.Application.Services.LogService;
using Pos.Application.Services.ReferenceService.InvoiceTypeService;
using Pos.Application.Services.ReferenceService.PaymentService;
using Pos.Application.Utility;
using Pos.SecurityEncryption;
using Pos.WinFormsUI.AlertClasses;
using System.Configuration;
using System.Runtime.InteropServices;
using System.Text;

namespace Pos.WinFormsUI.Forms
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
        private readonly AppSettings _appSettings;

        // For Dynamic Payment Method and Invoice Type
        private readonly IPaymentService _paymentService;
        private readonly IInvoiceTypeService _invoiceTypeService;
        private Dictionary<string, string>? _paymentModeMap;
        private Dictionary<string, string>? _invoiceTypeMap;

        public ExportInvoiceForm(
            ILiveService liveService,
            ILogService logService,
            IPaymentService paymentService,
            IInvoiceTypeService invoiceTypeService)
        {
            InitializeComponent();

            _liveService = liveService ?? throw new ArgumentNullException(nameof(liveService));
            _logService = logService ?? throw new ArgumentNullException(nameof(logService));
            _paymentService = paymentService ?? throw new ArgumentNullException(nameof(paymentService));
            _invoiceTypeService = invoiceTypeService ?? throw new ArgumentNullException(nameof(invoiceTypeService));

            _appSettings = new AppSettings
            {
                BaseUrl = ConfigurationManager.AppSettings["BaseUrl"] ?? string.Empty,
                Token = ConfigurationManager.AppSettings["Token"] ?? string.Empty,
                EC = ConfigurationManager.AppSettings["EC"] ?? string.Empty,
                POS = int.TryParse(ConfigurationManager.AppSettings["POS"], out int posId) ? posId : 0,
                Environment = ConfigurationManager.AppSettings["Environment"] ?? string.Empty
            };
            environment = ConfigurationManager.AppSettings["Environment"];

            // --- Date setup ---
            dateTimePickerTo.MaxDate = DateTime.Today;
            dateTimePickerFrom.MaxDate = DateTime.Today;
            dateTimePickerFrom.Format = DateTimePickerFormat.Custom;
            dateTimePickerTo.Format = DateTimePickerFormat.Custom;
            dateTimePickerFrom.CustomFormat = "dd MMM yyyy";
            dateTimePickerTo.CustomFormat = "dd MMM yyyy";
            this.ActiveControl = ExportInvoiceBtn;

            // --- Button hover styling ---
            ExportInvoiceBtn.MouseEnter += (s, e) => ExportInvoiceBtn.BackColor = System.Drawing.Color.FromArgb(60, 179, 113);
            ExportInvoiceBtn.MouseLeave += (s, e) => ExportInvoiceBtn.BackColor = System.Drawing.Color.MediumSeaGreen;
            ExportInvoiceBtn.Click += async (s, e) => await ExportInvoiceBtn_ClickAsync(s, e);

            // --- Rounded panel ---
            panelPending.Region = Region.FromHrgn(
                CreateRoundRectRgn(0, 0, panelPending.Width, panelPending.Height, 20, 20));

            _httpClient = new HttpClient();
        }

        private async Task EnsureLookupsLoadedAsync()
        {
            if (_paymentModeMap is not null && _invoiceTypeMap is not null)
                return;

            ApiResponse<List<ReferenceDto>> paymentResponse = await _paymentService.GetPaymentMethodsAsync();
            _paymentModeMap = paymentResponse.Data
                .ToDictionary(p => p.Id.ToString(), p => p.Name, StringComparer.OrdinalIgnoreCase);

            ApiResponse<List<ReferenceDto>> invoiceTypeResponse = await _invoiceTypeService.GetInvoiceTypesAsync();
            _invoiceTypeMap = invoiceTypeResponse.Data
                .ToDictionary(t => t.Id.ToString(), t => t.Name, StringComparer.OrdinalIgnoreCase);
        }

        private string ResolveColumnValue(string header, string value) => header switch
        {
            "Payment Mode" => _paymentModeMap!.TryGetValue(value.Trim(), out string? p) ? p : value,
            "Invoice Type" => _invoiceTypeMap!.TryGetValue(value.Trim(), out string? t) ? t : value,
            _ => value
        };

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

                await EnsureLookupsLoadedAsync();

                string? decryptedPOSID = ConfigurationManager.AppSettings["Username"];
                int encryptedPOSID = Convert.ToInt32(AesEncryptionHelper.Decrypt(decryptedPOSID!));

                // Construct full URL with environment query string
                string fullUrlWithEnv = $"{_appSettings.BaseUrl.TrimEnd('/')}/{Endpoints.ExportCSV.TrimStart('/')}?environment={_appSettings.Environment}";

                // Prepare request body
                InvoiceFilterDto requestBody = new()
                {
                    PosId = encryptedPOSID,
                    FromDate = dateTimePickerFrom.Value.Date,
                    ToDate = dateTimePickerTo.Value.Date,
                    RegistrationNumber = 0
                };

                // ── Call API via HttpClientHelper ─────────────────────────────
                ApiResponse<string> apiResponse = await HttpClientHelper.PostAsyncRawString(
                    url: fullUrlWithEnv,
                    body: requestBody,
                    bearerToken: _appSettings.Token,
                    logService: _logService,
                    appSettings: _appSettings
                );

                // ── Validate response ─────────────────────────────────────────
                if (apiResponse == null)
                {
                    await CreateLog("No response from service", AlertType.Error);
                    AlertManager.ShowError("No response from service.");
                    lblExportStatus.Text = "❌ No response from service.";
                    lblExportStatus.ForeColor = System.Drawing.Color.Red;
                    return;
                }

                if (apiResponse.StatusCode != "200")
                {
                    await CreateLog($"API error: {apiResponse.Message}", AlertType.Error);
                    AlertManager.ShowError($"Export failed: {apiResponse.Message}");
                    lblExportStatus.Text = $"❌ {apiResponse.Message}";
                    lblExportStatus.ForeColor = System.Drawing.Color.Red;
                    return;
                }

                if (string.IsNullOrWhiteSpace(apiResponse.Data))
                {
                    lblExportStatus.Text = "⚠ No invoices found for the selected range.";
                    lblExportStatus.ForeColor = System.Drawing.Color.Orange;
                    AlertManager.ShowInfo("No invoices found for the selected date range.");
                    await CreateLog("No invoices found in selected range", AlertType.Info);
                    return;
                }

                // ── Parse CSV lines from response ─────────────────────────────
                string[] lines = apiResponse.Data.Split(
                    new[] { "\r\n", "\n" },
                    StringSplitOptions.RemoveEmptyEntries);

                if (lines.Length <= 1)
                {
                    lblExportStatus.Text = "⚠ No invoice records to export.";
                    lblExportStatus.ForeColor = System.Drawing.Color.Orange;
                    AlertManager.ShowInfo("No invoice records found.");
                    return;
                }

                // ── Column mapping (CSV index → Excel header) ─────────────────
                (int Index, string Header)[] columnMap = new (int Index, string Header)[]
                {
                    (1,  "Invoice Number"),
                    (3,  "USIN"),
                    (2,  "POSID"),
                    (21, "Buyer NTN"),
                    (23, "Buyer CNIC"),
                    (5,  "Buyer Name"),
                    (6,  "Buyer Phone Number"),
                    (7,  "Total Sale Value"),
                    (8,  "Total Quantity"),
                    (9,  "Total Tax Charged"),
                    (10, "Discount"),
                    (11, "Total Bill Amount"),
                    (12, "Payment Mode"),
                    (4,  "Invoice Entry DateTime"),
                    (13, "Synced DateTime"),
                    (16, "Invoice Type"),
                    (17, "RefUSIN"),
                    (22, "Further Tax"),
                };

                // ── Ask user: CSV or XLSX ─────────────────────────────────────
                using SaveFileDialog sfd = new()
                {
                    Filter = "Excel Workbook (*.xlsx)|*.xlsx|CSV File (*.csv)|*.csv",
                    Title = "Save Exported Invoices",
                    FileName = $"Invoices_{DateTime.Now:yyyyMMdd_HHmmss}"
                };

                if (sfd.ShowDialog() != DialogResult.OK)
                {
                    await CreateLog("Export canceled by user", AlertType.Info);
                    lblExportStatus.Text = "⚠ Export canceled by user.";
                    lblExportStatus.ForeColor = System.Drawing.Color.Orange;
                    return;
                }

                try
                {
                    bool isCsv = sfd.FilterIndex == 2; // 1 = xlsx, 2 = csv

                    if (isCsv)
                    {
                        // ── Export as CSV ─────────────────────────────────────
                        List<string> csvLines = new()
                {
                    string.Join(",", columnMap.Select(c => $"\"{c.Header}\""))
                };

                        for (int i = 1; i < lines.Length; i++) // skip original header row
                        {
                            string[] cols = lines[i].Split(',');

                            if (cols.Length <= 1 || string.IsNullOrWhiteSpace(cols[1]))
                                continue;

                            List<string> rowValues = new();

                            foreach ((int index, string header) in columnMap)
                            {
                                string value = index < cols.Length ? cols[index].Trim() : string.Empty;
                                rowValues.Add($"\"{ResolveColumnValue(header, value)}\"");
                            }

                            csvLines.Add(string.Join(",", rowValues));
                        }

                        string csvPath = sfd.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase)
                            ? sfd.FileName
                            : sfd.FileName + ".csv";

                        await File.WriteAllLinesAsync(csvPath, csvLines, Encoding.UTF8);

                        await CreateLog("Invoices exported successfully as CSV", AlertType.Success);
                        AlertManager.ShowSuccess("Invoices exported successfully!");
                        lblExportStatus.Text = $"✅ Exported successfully:\n{csvPath}";
                        lblExportStatus.ForeColor = System.Drawing.Color.Green;
                    }
                    else
                    {
                        // ── Export as XLSX ────────────────────────────────────
                        using XLWorkbook workbook = new();
                        IXLWorksheet sheet = workbook.Worksheets.Add("Invoices");
                        int excelRow = 1;

                        for (int i = 0; i < lines.Length; i++)
                        {
                            string[] cols = lines[i].Split(',');

                            if (i == 0)
                            {
                                for (int j = 0; j < columnMap.Length; j++)
                                    sheet.Cell(excelRow, j + 1).Value = columnMap[j].Header;

                                excelRow++;
                            }
                            else
                            {
                                if (cols.Length <= 1 || string.IsNullOrWhiteSpace(cols[1]))
                                    continue;

                                for (int j = 0; j < columnMap.Length; j++)
                                {
                                    int index = columnMap[j].Index;
                                    string header = columnMap[j].Header;

                                    if (index < cols.Length)
                                    {
                                        string value = cols[index].Trim();
                                        sheet.Cell(excelRow, j + 1).Value = ResolveColumnValue(header, value);
                                    }
                                }

                                excelRow++;
                            }
                        }

                        string xlsxPath = sfd.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase)
                            ? sfd.FileName
                            : sfd.FileName + ".xlsx";

                        workbook.SaveAs(xlsxPath);

                        await CreateLog("Invoices exported successfully as XLSX", AlertType.Success);
                        AlertManager.ShowSuccess("Invoices exported successfully!");
                        lblExportStatus.Text = $"✅ Exported successfully:\n{xlsxPath}";
                        lblExportStatus.ForeColor = System.Drawing.Color.Green;
                    }
                }
                catch (Exception ex)
                {
                    await CreateLog($"Export failed: {ex.Message}", AlertType.Error);
                    AlertManager.ShowError("Export failed!");
                    lblExportStatus.Text = "❌ Export failed.";
                    lblExportStatus.ForeColor = System.Drawing.Color.Red;
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
    }
}
