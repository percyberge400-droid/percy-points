using Microsoft.Reporting.WinForms;
using POSPRA.DTOs.InvoiceDtos;
using QRCoder;
using System;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Collections.Generic; // Added for List

namespace POSPRA_WinFormsUI.Forms
{
    public partial class InvoiceReport : Form
    {
        private readonly InvoiceDto _invoiceDto;
        private readonly string _invoiceNumber;
        private readonly bool _isFromDashboard;
        private readonly bool _printDirectly; // New flag for direct printing
        private ReportViewer _reportViewer;
        private static readonly string businessname = ConfigurationManager.AppSettings["businessName"];
        private static readonly string branchName = ConfigurationManager.AppSettings["branchName"];
        private static readonly string branchAddress = ConfigurationManager.AppSettings["branchAddress"];
        private static readonly Dictionary<string, byte[]> _qrCache = new();
        private static LocalReport _cachedReportTemplate;
        private int _itemCount; // To store the number of invoice items for dynamic height


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
            //_reportViewer.ZoomMode = ZoomMode.PageWidth;

        }
        private void ApplyThermalPaperSize(float heightInCm = 50f) // Default fixed for preview, dynamic for print
        {
            try
            {
                // Convert cm to hundredths of inch: 1 inch = 2.54 cm → 100 * cm / 2.54
                int width = (int)(8.0 / 2.54 * 100);  // ≈ 315 for 80mm
                int height = (int)(heightInCm / 2.54 * 100); // Dynamic or default

                var pageSettings = new PageSettings
                {
                    PaperSize = new PaperSize("Thermal 80mm", width, height),
                    Margins = new Margins(10, 10, 10, 10) // 0.1 inch margins
                };

                _reportViewer.SetPageSettings(pageSettings);
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
                1 => "Cash",
                2 => "Credit",
                3 => "Online",
                _ => "N/A"
            };
            headerRow["ModeOfPayment"] = paymentModeText;
            headerRow["LogoImage"] = logo;
            headerRow["QRCodeImage"] = qr;
            headerRow["PRALogo"] = praLogo;
            headerRow["NTN"] = dto.BuyerNTN ?? string.Empty;
            headerRow["Address"] = branchName + ",  " + branchAddress; //dto.BuyerName;// + ", City, Pakistan";
            headerRow["STRN"] = dto.USIN ?? string.Empty;
            headerRow["InvoiceNo"] = dto.FBRInvoiceNumber ?? string.Empty;
            headerRow["Total"] = dto.TotalBillAmount;
            headerRow["POSID"] = dto.POSID.ToString();
            headerRow["TotalTax"] = dto.TotalTaxCharged;
            headerRow["Discount"] = dto.Discount;
            headerRow["TotalQty"] = dto.TotalQuantity;
            headerTable.Rows.Add(headerRow);
            //int serial = 1;
            foreach (var item in dto.InvoiceItemDto ?? Enumerable.Empty<dynamic>())
            {
                var row = bodyTable.NewRow();
                row["Amount"] = item.TotalAmount;
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
            string logoKey = ConfigurationManager.AppSettings["BusinessLOGO"];
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


        private byte[] LoadPraLogo()
        {
            string logoKey = ConfigurationManager.AppSettings["LOGO"];
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
                ApplyThermalPaperSize();

                _reportViewer.RefreshReport();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading report: {ex.Message}", "Report Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

        // New method to handle direct printing to thermal printer
        public void PrintDirectlyToThermal()
        {
            try
            {
                // Step 1: Find and select the thermal printer
                string thermalPrinterName = FindThermalPrinter();

                if (string.IsNullOrEmpty(thermalPrinterName))
                {
                    MessageBox.Show("Thermal printer not found or not selected.", "Printer Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Step 2: Get printer properties (for logging or validation)
                PrinterSettings printerSettings = new PrinterSettings { PrinterName = thermalPrinterName };
                if (!printerSettings.IsValid)
                {
                    MessageBox.Show($"Invalid printer: {thermalPrinterName}", "Printer Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Log properties (optional, for debugging)
                // Example: supported paper sizes
                string propertiesInfo = $"Printer: {thermalPrinterName}\n" +
                                        $"Default Page Size: {printerSettings.DefaultPageSettings.PaperSize.Kind}\n" +
                                        $"Landscape: {printerSettings.DefaultPageSettings.Landscape}";
                // You can show or log this: // MessageBox.Show(propertiesInfo); // Uncomment if needed

                // Step 3: "Connect" - In Windows, selecting the printer "connects" it via the driver.
                // No explicit connect needed if installed.

                // Step 4: Calculate dynamic height based on item count
                float heightInches = CalculateDynamicHeight(_itemCount);

                // Step 5: Print the report directly in background without preview
                _reportViewer.LocalReport.PrintToThermal(thermalPrinterName, 3.15f, heightInches); // 80mm width, dynamic height
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during direct printing: {ex.Message}", "Print Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Helper to calculate dynamic height in inches
        private float CalculateDynamicHeight(int itemCount)
        {
            float headerHeight = 3f; // Adjust based on your report design (logos, address, etc.)
            float rowHeight = 0.3f; // Adjust per item row height (including spacing)
            float footerHeight = 2f; // Adjust for totals, QR, etc.
            float buffer = 1f; // Extra space to avoid cutoff
            float calculatedHeight = headerHeight + (itemCount * rowHeight) + footerHeight + buffer;
            return Math.Max(3f, calculatedHeight); // Minimum 5 inches
        }

        // Helper to find thermal printer
        private string FindThermalPrinter()
        {
            // First, check config for predefined printer name
            string configPrinterName = ConfigurationManager.AppSettings["ThermalPrinterName"];
            if (!string.IsNullOrEmpty(configPrinterName))
            {
                if (PrinterSettings.InstalledPrinters.Cast<string>().Contains(configPrinterName))
                {
                    return configPrinterName;
                }
            }

            // If not in config, list installed printers and filter/look for thermal (e.g., contains "80mm" or "Thermal")
            var installedPrinters = PrinterSettings.InstalledPrinters.Cast<string>().ToList();
            var potentialThermal = installedPrinters.FirstOrDefault(p => p.Contains("80") || p.Contains("Thermal") || p.Contains("XP-") || p.Contains("POS-"));

            if (!string.IsNullOrEmpty(potentialThermal))
            {
                return potentialThermal;
            }

            // If none found, show dialog to select
            using (var dialog = new PrintDialog())
            {
                dialog.AllowSomePages = false;
                dialog.AllowSelection = false;
                dialog.UseEXDialog = true;

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    return dialog.PrinterSettings.PrinterName;
                }
            }

            return null; // None selected
        }

        #endregion
    }

    // Extension class for direct printing (add this in the same file or a new one)
    public static class LocalReportExtensions
    {
        public static void PrintToThermal(this LocalReport report, string printerName, float widthInches = 3.15f, float heightInches = 19.7f) // Default 80mm width, dynamic height passed in
        {
            var pageSettings = new PageSettings
            {
                PaperSize = new PaperSize("Thermal 80mm", (int)(widthInches * 100), (int)(heightInches * 100)), // Hundredths of inch
                Margins = new Margins(10, 10, 10, 10), // Small margins: 0.1in each
                Landscape = false // Portrait for receipts
            };

            // Device info for rendering (matches thermal size)
            string deviceInfo = $@"
                <DeviceInfo>
                    <OutputFormat>EMF</OutputFormat>
                    <PageWidth>{widthInches}in</PageWidth>
                    <PageHeight>{heightInches}in</PageHeight>
                    <MarginTop>0.1in</MarginTop>
                    <MarginLeft>0.1in</MarginLeft>
                    <MarginRight>0.1in</MarginRight>
                    <MarginBottom>0.1in</MarginBottom>
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

            printDocument.Print(); // Prints in background, no preview
        }
    }
}