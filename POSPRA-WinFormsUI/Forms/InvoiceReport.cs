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

namespace POSPRA_WinFormsUI.Forms
{
    public partial class InvoiceReport : Form
    {
        private readonly InvoiceDto _invoiceDto;
        private readonly string _invoiceNumber;
        private readonly bool _isFromDashboard;
        private ReportViewer _reportViewer;
        private static readonly string businessname = ConfigurationManager.AppSettings["businessName"]; //
        private static readonly string branchName = ConfigurationManager.AppSettings["branchName"];     //
        private static readonly string branchAddress = ConfigurationManager.AppSettings["branchAddress"]; //
        private static readonly Dictionary<string, byte[]> _qrCache = new();
        private static LocalReport _cachedReportTemplate;

        // Constructor for Save button
        public InvoiceReport(InvoiceDto invoiceDto)
        {
            InitializeComponent();
            _invoiceDto = invoiceDto ?? throw new ArgumentNullException(nameof(invoiceDto));
            _isFromDashboard = false;
            InitializeReportViewer();
        }

        // Constructor for Dashboard print
        public InvoiceReport(string invoiceNumber)
        {
            InitializeComponent();
            _invoiceNumber = invoiceNumber ?? throw new ArgumentNullException(nameof(invoiceNumber));
            _isFromDashboard = true;
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

            // ✅ Apply thermal printer paper size (5.8cm × 15cm)
            ApplyThermalPaperSize();
            LoadReport();

            // ✅ Thermal printer display mode
            _reportViewer.SetDisplayMode(DisplayMode.PrintLayout);
            //_reportViewer.ZoomMode = ZoomMode.PageWidth;

        }
        private void ApplyThermalPaperSize()
        {
            try
            {
                // Convert cm to hundredths of inch: 1 inch = 2.54 cm → 100 * cm / 2.54
                int width = (int)(8.0 / 2.54 * 100);  // ≈ 228
                int height = (int)(15 / 2.54 * 100); // ≈ 591

                var pageSettings = new PageSettings
                {
                    PaperSize = new PaperSize("Thermal80x150", width, height),
                    Margins = new Margins(0, 0, 0, 0)
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
                1 => "Cash",
                2 => "Credit",
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
                 
                _reportViewer.LocalReport.ReportPath = reportPath;
                _reportViewer.LocalReport.DataSources.Clear();
                _reportViewer.LocalReport.DataSources.Add(new ReportDataSource("HeaderDataSet", header));
                _reportViewer.LocalReport.DataSources.Add(new ReportDataSource("BodyDataSet", body));
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
    }
}
