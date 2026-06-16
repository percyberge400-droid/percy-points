using Microsoft.Reporting.WinForms;
using Pos.Application.DTOs.InvoiceDtos;
using Pos.WinFormsUI.AlertClasses;
using Pos.WinFormsUI.Forms.Logo;
using QRCoder;
using System.Configuration;
using System.Data;
using System.Drawing.Imaging;
using System.Drawing.Printing;

namespace Pos.WinFormsUI.Forms
{
    public partial class InvoiceReport : Form
    {
        private readonly InvoiceDto _invoiceDto;
        private readonly string _invoiceNumber;
        private readonly bool _isFromDashboard;
        private readonly bool _printDirectly; // New flag for direct printing
        private ReportViewer _reportViewer;
        private static readonly string businessname = ConfigurationManager.AppSettings["businessName"]!; //
        private static readonly string branchName = ConfigurationManager.AppSettings["branchName"]!;     //
        private static readonly string branchAddress = ConfigurationManager.AppSettings["branchAddress"]!; //
        private static readonly string PhoneNumber = ConfigurationManager.AppSettings["phoneNumber"]!; //
        private static readonly string NTN = ConfigurationManager.AppSettings["NTN"]!;
        private static readonly Dictionary<string, byte[]> _qrCache = new();
        private readonly bool _isOffline;
        private int _itemCount; // To store the number of invoice items for dynamic height
        public event EventHandler ReportLoaded;
        private const string OfflineDisclaimerText =
    "⚠ OFFLINE INVOICE\n" +
    "This invoice has been generated through the PRA Offline eIMS Component " +
    "and is subject to verification by PRA systems. Invoice verification and " +
    "synchronization may take up to two (2) hours from the time of issuance.";

        private void InvoiceReport_Shown(object? sender, EventArgs e)
        {
            // Bring window in front without keeping it topmost
            if (!this.IsDisposed && this.Visible)
            {
                this.TopMost = true;
                this.TopMost = false;
                this.Activate();
            }
        }

        // Constructor for Save button
        public InvoiceReport(
            InvoiceDto invoiceDto,
            bool printDirectly = false,
            bool isOffline = false)
        {
            InitializeComponent();
            _invoiceDto = invoiceDto ?? throw new ArgumentNullException(nameof(invoiceDto));
            _isFromDashboard = false;
            _printDirectly = printDirectly;
            _isOffline = isOffline;
            InitializeReportViewer();
        }

        // Constructor for Dashboard print
        public InvoiceReport(string invoiceNumber, bool printDirectly = false)
        {
            InitializeComponent();
            _invoiceNumber = invoiceNumber ?? throw new ArgumentNullException(nameof(invoiceNumber));
            _isFromDashboard = true;
            _isOffline = false;
            _printDirectly = printDirectly;
            InitializeReportViewer();
        }

        #region Initialization

        private void InitializeReportViewer()
        {
            _reportViewer = new ReportViewer
            {
                Dock = DockStyle.Fill,
                ProcessingMode = ProcessingMode.Local
            };

            _reportViewer.LocalReport.EnableExternalImages = true;

            Controls.Add(_reportViewer);

            // ✅ Apply thermal printer paper size (8cm width, variable height for roll)

            LoadReport();

            // ✅ Thermal printer display mode
            _reportViewer.SetDisplayMode(DisplayMode.PrintLayout);
            _reportViewer.ZoomMode = ZoomMode.PageWidth;

        }
        private void ApplyThermalPaperSize() // Default fixed for preview, dynamic for print
        {
            try
            {
                // Convert cm to hundredths of inch: 1 inch = 2.54 cm → 100 * cm / 2.54
                int width = (int)(8.0 / 2.54 * 100);  // ≈ 315 for 80mm
                //int height = (int)(totalHeightCm / 2.54 * 100); // Dynamic

                PageSettings pageSettings = new()
                {
                    PaperSize = new PaperSize("Thermal 80mm", width, 0),
                    Margins = new Margins(5, 5, 5, 5) // 0.1 inch margins
                };

                _reportViewer.SetPageSettings(pageSettings);
                _reportViewer.RefreshReport();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to apply thermal page settings: {ex.Message}",
                    "Page Setup Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion

        #region Data Preparation

        private (DataTable Header, DataTable Body) BuildInvoiceDataSets(InvoiceDto dto)
        {
            DataTable headerTable = new("HeaderDataSet");
            headerTable.Columns.AddRange(new[]
            {
        new DataColumn("BusinessName", typeof(string)),
        new DataColumn("DateCreated", typeof(DateTime)),
        new DataColumn("ModeOfPayment", typeof(string)),
        new DataColumn("InvoiceType", typeof(string)),
        new DataColumn("LogoImage", typeof(byte[])),
        new DataColumn("QRCodeImage", typeof(byte[])),
        new DataColumn("PRALogo", typeof(byte[])),
        new DataColumn("NTN", typeof(string)),
        new DataColumn("Address", typeof(string)),
        new DataColumn("STRN", typeof(string)),
        new DataColumn("InvoiceNo", typeof(string)),
        new DataColumn("PhoneNumber", typeof(string)),
        new DataColumn("POSID", typeof(string)),
        new DataColumn("Discount", typeof(decimal)),
        new DataColumn("TotalTax", typeof(decimal)),
        new DataColumn("TotalQty", typeof(decimal)),
        new DataColumn("Total", typeof(decimal)),
        new DataColumn("OfflineDisclaimer", typeof(string))
    });

            DataTable bodyTable = new("BodyDataSet");
            bodyTable.Columns.AddRange(new[]
            {
        // ✅ changed Amount to decimal to avoid overflow
        new DataColumn("Amount", typeof(decimal)),
        new DataColumn("ItemName", typeof(string)),
        new DataColumn("TaxRate", typeof(decimal)),
        new DataColumn("Qty", typeof(decimal)),
        new DataColumn("Price", typeof(decimal)),
        new DataColumn("Tax", typeof(decimal))
    });

            byte[] logo = LoadCompanyLogo();
            byte[] praLogo = LoadPraLogo();
            byte[] qr = GenerateQRCode(dto.InvoiceNumber);

            DataRow headerRow = headerTable.NewRow();
            headerRow["BusinessName"] = businessname;
            headerRow["PhoneNumber"] = PhoneNumber;
            headerRow["DateCreated"] = dto.DateTime;

            string paymentModeText = dto.PaymentMode switch
            {
                1 => "Credit Card",
                2 => "Cash",
                3 => "Online",
                _ => "N/A"
            };
            headerRow["ModeOfPayment"] = paymentModeText;

            string invoiceType = dto.InvoiceType switch
            {
                1 => "Sale",
                2 => "Purchase",
                3 => "Debit",
                4 => "Credit",
                _ => "N/A"
            };
            headerRow["InvoiceType"] = invoiceType;

            headerRow["LogoImage"] = logo;
            headerRow["QRCodeImage"] = qr;
            headerRow["PRALogo"] = praLogo;
            headerRow["NTN"] = NTN ?? string.Empty;
            headerRow["Address"] = $"{branchName}, {branchAddress}";
            headerRow["STRN"] = dto.USIN ?? string.Empty;
            headerRow["InvoiceNo"] = dto.InvoiceNumber ?? string.Empty;
            headerRow["POSID"] = dto.POSID.ToString();
            //headerRow["PhoneNumber"] = dto.BuyerPhoneNumber ?? string.Empty;
            headerRow["Discount"] = dto.Discount;//Math.Round(dto.Discount, 2, MidpointRounding.AwayFromZero);
            headerRow["TotalTax"] = dto.TotalTaxCharged;//Math.Round(dto.TotalTaxCharged, 2, MidpointRounding.AwayFromZero);
            headerRow["TotalQty"] = dto.TotalQuantity;
            headerRow["Total"] = dto.TotalBillAmount;// Math.Round(, 2, MidpointRounding.AwayFromZero);
            headerRow["OfflineDisclaimer"] = _isOffline
                ? OfflineDisclaimerText
                : string.Empty;

            headerTable.Rows.Add(headerRow);

            foreach (dynamic item in dto.Items ?? Enumerable.Empty<dynamic>())
            {
                DataRow row = bodyTable.NewRow();
                row["Amount"] = item.TotalAmount; //Math.Round(item.TotalAmount + item.Discount, 0, MidpointRounding.AwayFromZero);
                row["ItemName"] = item.ItemName ?? string.Empty;
                row["TaxRate"] = item.TaxRate;
                row["Qty"] = item.Quantity;
                row["Price"] = item.SaleValue;
                row["Tax"] = item.TaxCharged;
                bodyTable.Rows.Add(row);
            }

            return (headerTable, bodyTable);
        }


        private byte[] LoadCompanyLogo()
        {
            try
            {
                AppResources.RefreshLogo();
                Image logo = AppResources.BusinessLogo;
                if (logo == null)
                    return Array.Empty<byte>();
                using (MemoryStream ms = new())
                {
                    logo.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                    return ms.ToArray();
                }
            }
            catch
            {
                return Array.Empty<byte>();
            }
        }


        private byte[] LoadPraLogo()
        {
            string logoKey = ConfigurationManager.AppSettings["LOGO-new"];
            if (!string.IsNullOrEmpty(logoKey))
            {
                object? res = Resources.ResourceManager.GetObject(logoKey);
                if (res is Image img)
                {
                    using (MemoryStream ms = new())
                    {
                        img.Save(ms, ImageFormat.Png);
                        return ms.ToArray();
                    }
                }
            }
            return SafeReadImage(logoKey);
        }

        private byte[] SafeReadImage(string path)
        {
            try
            {
                if (File.Exists(path))
                    return File.ReadAllBytes(path);
                else
                    MessageBox.Show($"⚠️ Image not found:\n{path}", "Missing Resource",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading image:\n{ex.Message}", "Image Load Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return Array.Empty<byte>();
        }

        private byte[] GenerateQRCode(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return null;
            if (_qrCache.TryGetValue(text, out byte[]? cached)) return cached;
            using QRCodeGenerator qrGen = new();
            using QRCodeData qrData = qrGen.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q);
            using QRCode qrCode = new(qrData);
            using Bitmap bmp = qrCode.GetGraphic(3);
            using MemoryStream ms = new();
            bmp.Save(ms, ImageFormat.Png);
            _qrCache[text] = ms.ToArray();
            return _qrCache[text];
        }

        #endregion

        #region Report Loading

        private void LoadReport()
        {
            try
            {
                string reportPath = GetReportPath();
                if (string.IsNullOrEmpty(reportPath))
                    return;

                (DataTable? header, DataTable? body) = BuildInvoiceDataSets(_invoiceDto);
                _itemCount = body.Rows.Count; // Store item count for dynamic height

                _reportViewer.LocalReport.ReportPath = reportPath;
                _reportViewer.LocalReport.DataSources.Clear();
                _reportViewer.LocalReport.DataSources.Add(new ReportDataSource("HeaderDataSet", header));
                _reportViewer.LocalReport.DataSources.Add(new ReportDataSource("BodyDataSet", body));

                // Apply thermal settings before refresh (use default height for preview)
                _reportViewer.RefreshReport();

                // ✅ Fire event when RDLC report finishes rendering
                _reportViewer.RenderingComplete += (s, e) =>
                {
                    try
                    {
                        // Let dashboard know the report is ready (for hiding progress bar or printing)
                        ReportLoaded?.Invoke(this, EventArgs.Empty);
                    }
                    catch { /* Safely ignore any UI thread timing issues */ }
                };
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading report: {ex.Message}", "Report Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private string GetReportPath(string reportFileName = "InvoiceReport.rdlc")
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string reportPath = Path.Combine(baseDir, "Reports", reportFileName);

            if (File.Exists(reportPath))
                return reportPath;

            MessageBox.Show($"Report not found: {reportPath}", "Missing RDLC", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return string.Empty;
        }

        #endregion

        #region Direct Printing

        // Override Load event to handle direct printing
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (_printDirectly)
            {
                this.Visible = false; // Don't show the form
                PrintDirectlyToThermal();
                this.Close(); // Close after printing
            }
        }

        //method to handle direct printing to thermal printer
        public void PrintDirectlyToThermal()
        {
            try
            {
                //Find and select the thermal printer
                string thermalPrinterName = FindThermalPrinter();

                if (string.IsNullOrEmpty(thermalPrinterName))
                {
                    AlertManager.ShowError("Thermal printer not found or not selected.");
                    return;
                }

                // printer properties
                PrinterSettings printerSettings = new() { PrinterName = thermalPrinterName };
                if (!printerSettings.IsValid)
                {
                    AlertManager.ShowError($"Invalid printer: {thermalPrinterName}");
                    return;
                }

                // Example: supported paper sizes
                string propertiesInfo = $"Printer: {thermalPrinterName}\n" +
                                        $"Default Page Size: {printerSettings.DefaultPageSettings.PaperSize.Kind}\n" +
                                        $"Landscape: {printerSettings.DefaultPageSettings.Landscape}";


                // Step 5: Print the report directly in background without preview
                _reportViewer.LocalReport.PrintToThermal(thermalPrinterName, 3.15); // 80mm width, dynamic height
            }
            catch (Exception ex)
            {
                AlertManager.ShowError($"Error during direct printing: {ex.Message}");
            }
        }


        // Helper to find thermal printer
        public static string? FindThermalPrinter()
        {

            try
            {
                List<string> installedPrinters = PrinterSettings.InstalledPrinters.Cast<string>().ToList();

                if (installedPrinters == null || installedPrinters.Count == 0)
                    throw new InvalidOperationException("No printers are installed on this system.");

                // common brand/model keywords
                string? potentialThermal = installedPrinters.FirstOrDefault(p =>
                    p.Contains("80", StringComparison.OrdinalIgnoreCase) ||
                    p.Contains("85", StringComparison.OrdinalIgnoreCase) ||
                    p.Contains("Thermal", StringComparison.OrdinalIgnoreCase) ||
                    p.Contains("POS", StringComparison.OrdinalIgnoreCase) ||
                    p.Contains("XP-", StringComparison.OrdinalIgnoreCase) ||
                    p.Contains("XPrinter", StringComparison.OrdinalIgnoreCase) ||
                    p.Contains("BlackCopper", StringComparison.OrdinalIgnoreCase) ||
                    p.Contains("BC-", StringComparison.OrdinalIgnoreCase) ||
                    p.Contains("Rongta", StringComparison.OrdinalIgnoreCase) ||
                    p.Contains("RP", StringComparison.OrdinalIgnoreCase) ||
                    p.Contains("Epson", StringComparison.OrdinalIgnoreCase) ||
                    p.Contains("TM-", StringComparison.OrdinalIgnoreCase) ||
                    p.Contains("Bixolon", StringComparison.OrdinalIgnoreCase) ||
                    p.Contains("Citizen", StringComparison.OrdinalIgnoreCase) ||
                    p.Contains("GP-", StringComparison.OrdinalIgnoreCase) ||
                    p.Contains("Gprinter", StringComparison.OrdinalIgnoreCase) ||
                    p.Contains("Speed", StringComparison.OrdinalIgnoreCase) ||
                    p.Contains("Winspeed", StringComparison.OrdinalIgnoreCase) ||
                    p.Contains("Zjiang", StringComparison.OrdinalIgnoreCase) ||
                    p.Contains("Xypos", StringComparison.OrdinalIgnoreCase)
                );


                if (!string.IsNullOrEmpty(potentialThermal))
                    return potentialThermal;

                // None found or selected
                throw new InvalidOperationException("No suitable thermal printer was found.");
            }
            catch (Exception ex)
            {
                AlertManager.ShowError($"Printer detection failed:\n{ex.Message}");
                throw;
            }
        }
    }
    #endregion


    // Extension class for direct printing 

    public static class LocalReportExtensions
    {
        public static void PrintToThermal(this LocalReport report, string printerName, double widthInche = 3.15) // Default 80mm width, dynamic height passed in
        {
            const double HeightInches = 100;
            PageSettings pageSettings = new()
            {
                PaperSize = new PaperSize("Thermal 80mm", (int)(widthInche * 100), (int)(HeightInches * 100)), // Hundredths of inch
                Margins = new Margins(2, 2, 2, 2), // Small margins: 0.1in each
                Landscape = false // Portrait for receipts
            };

            // Device info for rendering (matches thermal size)
            string deviceInfo = $@"
                    <DeviceInfo>
                        <OutputFormat>EMF</OutputFormat>
                        <PageWidth>{widthInche}in</PageWidth>
                        <PageHeight>{HeightInches}in</PageHeight>
                        <MarginTop>0.002in</MarginTop>
                        <MarginLeft>0.002in</MarginLeft>
                        <MarginRight>0.002in</MarginRight>
                        <MarginBottom>0.002in</MarginBottom>
                    </DeviceInfo>";

            Warning[] warnings;
            List<Stream> streams = new();
            int currentPageIndex = 0;

            report.Render("Image", deviceInfo, (name, fileNameExtension, encoding, mimeType, willSeek) =>
            {
                MemoryStream stream = new();
                streams.Add(stream);
                return stream;
            }, out warnings);

            foreach (Stream stream in streams)
                stream.Position = 0;

            if (streams == null || streams.Count == 0)
                throw new Exception("Error: No content to print.");

            PrintDocument printDocument = new()
            {
                PrinterSettings = { PrinterName = printerName },
                DefaultPageSettings = pageSettings
            };

            if (!printDocument.PrinterSettings.IsValid)
                throw new Exception($"Error: Printer '{printerName}' not found or invalid.");

            printDocument.PrintPage += (sender, e) =>
            {
                Metafile pageImage = new(streams[currentPageIndex]);
                Rectangle adjustedRect = new(
                    e.PageBounds.Left - (int)e.PageSettings.HardMarginX,
                    e.PageBounds.Top - (int)e.PageSettings.HardMarginY,
                    e.PageBounds.Width,
                    e.PageBounds.Height);
                e.Graphics.FillRectangle(Brushes.White, adjustedRect);
                e.Graphics.DrawImage(pageImage, adjustedRect);
                currentPageIndex++;
                e.HasMorePages = (currentPageIndex < streams.Count);
            };

            printDocument.EndPrint += (sender, e) =>
            {
                if (streams != null)
                {
                    foreach (Stream stream in streams) stream.Close();
                    streams.Clear();
                }
            };

            printDocument.Print(); // Prints in background,
        }
    }
}