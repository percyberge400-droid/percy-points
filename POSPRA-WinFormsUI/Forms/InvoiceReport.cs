using Microsoft.Reporting.WinForms;
using POSPRA.DTOs.InvoiceDtos;
using POSPRA_WinFormsUI.AlertClasses;
using POSPRA_WinFormsUI.Forms.Logo;
using QRCoder;
using System.Configuration;
using System.Data;
using System.Drawing.Imaging;
using System.Drawing.Printing;

namespace POSPRA_WinFormsUI.Forms
{
    public partial class InvoiceReport : Form
    {
        public InvoiceReport()
        {
            InitializeComponent();
        }
        private readonly InvoiceDto _invoiceDto;
        private readonly string _invoiceNumber;
        private readonly bool _isFromDashboard;
        private readonly bool _printDirectly; // New flag for direct printing
        private ReportViewer _reportViewer;
        private static readonly string businessname = ConfigurationManager.AppSettings["businessName"]; //
        private static readonly string branchName = ConfigurationManager.AppSettings["branchName"];     //
        private static readonly string branchAddress = ConfigurationManager.AppSettings["branchAddress"]; //
        private static readonly Dictionary<string, byte[]> _qrCache = new();
        private static LocalReport _cachedReportTemplate;
        private int _itemCount; // To store the number of invoice items for dynamic height
        public event EventHandler ReportLoaded;

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
        public InvoiceReport(InvoiceDto invoiceDto, bool printDirectly = false)
        {
            InitializeComponent();
            _invoiceDto = invoiceDto ?? throw new ArgumentNullException(nameof(invoiceDto));
            _isFromDashboard = false;
            _printDirectly = printDirectly;
            InitializeReportViewer();
        }

        // Constructor for Dashboard print
        public InvoiceReport(string invoiceNumber, bool printDirectly = false)
        {
            InitializeComponent();
            _invoiceNumber = invoiceNumber ?? throw new ArgumentNullException(nameof(invoiceNumber));
            _isFromDashboard = true;
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

                var pageSettings = new PageSettings
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
            var headerTable = new DataTable("HeaderDataSet");
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
                new DataColumn("POSID", typeof(string)),
                new DataColumn("Discount", typeof(decimal)),
                new DataColumn("TotalTax", typeof(decimal)),
                new DataColumn("TotalQty", typeof(int)),
                new DataColumn("Total", typeof(decimal))
            });

            var bodyTable = new DataTable("BodyDataSet");
            bodyTable.Columns.AddRange(new[]
            {
                new DataColumn("Amount", typeof(int)),
                new DataColumn("ItemName", typeof(string)),
                new DataColumn("TaxRate", typeof(decimal)),
                new DataColumn("Qty", typeof(decimal)),
                new DataColumn("Price", typeof(decimal)),
                new DataColumn("Tax", typeof(decimal))
            });
            byte[] logo = LoadCompanyLogo();
            byte[] praLogo = LoadPraLogo();

            byte[] qr = GenerateQRCode(dto.FBRInvoiceNumber);

            var headerRow = headerTable.NewRow();
            headerRow["BusinessName"] = businessname;
            headerRow["DateCreated"] = dto.DateTime;
            string paymentModeText = dto.PaymentMode switch
            {
                1 => "Credit Card",
                2 => "Cash",
                3 => "Online",
                _ => "N/A"
            };
            headerRow["ModeOfPayment"] = paymentModeText;
            string InvoiceType = dto.InvoiceType switch
            {
                1 => "Sale",
                2 => "Purchase",
                3 => "Debit",
                4 => "Credit",
                _ => "N/A"
            };
            headerRow["InvoiceType"] = InvoiceType;
            headerRow["LogoImage"] = logo;
            headerRow["QRCodeImage"] = qr;
            headerRow["PRALogo"] = praLogo;
            headerRow["NTN"] = dto.BuyerNTN ?? string.Empty;
            headerRow["Address"] = branchName + ",  " + branchAddress; //dto.BuyerName;// + ", City, Pakistan";
            headerRow["STRN"] = dto.USIN ?? string.Empty;
            headerRow["InvoiceNo"] = dto.FBRInvoiceNumber ?? string.Empty;
            headerRow["Total"] = Math.Round(dto.TotalBillAmount, 0, MidpointRounding.AwayFromZero);
            headerRow["POSID"] = dto.POSID.ToString();
            headerRow["TotalTax"] = Math.Round(dto.TotalTaxCharged, 0, MidpointRounding.AwayFromZero);
            headerRow["Discount"] = Math.Round(dto.Discount, 0, MidpointRounding.AwayFromZero);
            headerRow["TotalQty"] = dto.TotalQuantity;
            headerTable.Rows.Add(headerRow);
            //int serial = 1;
            foreach (var item in dto.InvoiceItemDto ?? Enumerable.Empty<dynamic>())
            {
                var row = bodyTable.NewRow();
                row["Amount"] = Math.Round(item.TotalAmount + item.Discount, 0, MidpointRounding.AwayFromZero);
                row["ItemName"] = item.ItemName ?? string.Empty;
                //? item.ItemName.Substring(0, 20): item.ItemName ?? string.Empty;
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
                var logo = AppResources.BusinessLogo;
                if (logo == null)
                    return Array.Empty<byte>();
                using (MemoryStream ms = new MemoryStream())
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
                var res = Resources.ResourceManager.GetObject(logoKey);
                if (res is Image img)
                {
                    using (MemoryStream ms = new MemoryStream())
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
            if (_qrCache.TryGetValue(text, out var cached)) return cached;
            using var qrGen = new QRCodeGenerator();
            using var qrData = qrGen.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q);
            using var qrCode = new QRCode(qrData);
            using var bmp = qrCode.GetGraphic(3);
            using var ms = new MemoryStream();
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

                var (header, body) = BuildInvoiceDataSets(_invoiceDto);
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
                PrinterSettings printerSettings = new PrinterSettings { PrinterName = thermalPrinterName };
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
                var installedPrinters = PrinterSettings.InstalledPrinters.Cast<string>().ToList();

                if (installedPrinters == null || installedPrinters.Count == 0)
                    throw new InvalidOperationException("No printers are installed on this system.");

                // common brand/model keywords
                var potentialThermal = installedPrinters.FirstOrDefault(p =>
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
            var pageSettings = new PageSettings
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
            var streams = new List<Stream>();
            var currentPageIndex = 0;

            report.Render("Image", deviceInfo, (name, fileNameExtension, encoding, mimeType, willSeek) =>
            {
                var stream = new MemoryStream();
                streams.Add(stream);
                return stream;
            }, out warnings);

            foreach (Stream stream in streams)
                stream.Position = 0;

            if (streams == null || streams.Count == 0)
                throw new Exception("Error: No content to print.");

            var printDocument = new PrintDocument
            {
                PrinterSettings = { PrinterName = printerName },
                DefaultPageSettings = pageSettings
            };

            if (!printDocument.PrinterSettings.IsValid)
                throw new Exception($"Error: Printer '{printerName}' not found or invalid.");

            printDocument.PrintPage += (sender, e) =>
            {
                Metafile pageImage = new Metafile(streams[currentPageIndex]);
                Rectangle adjustedRect = new Rectangle(
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