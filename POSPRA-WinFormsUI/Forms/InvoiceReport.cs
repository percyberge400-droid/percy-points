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

            LoadReport();

            // ✅ Thermal printer display mode
            _reportViewer.SetDisplayMode(DisplayMode.PrintLayout);
            _reportViewer.ZoomMode = ZoomMode.PageWidth;

            // ✅ Apply thermal printer paper size (5.8cm × 15cm)
            ApplyThermalPaperSize();
        }

        private void ApplyThermalPaperSize()
        {
            try
            {
                // Convert cm to hundredths of inch: 1 inch = 2.54 cm → 100 * cm / 2.54
                int width = (int)(5.8 / 2.54 * 100);  // ≈ 228
                int height = (int)(15 / 2.54 * 100); // ≈ 591

                var pageSettings = new PageSettings
                {
                    PaperSize = new PaperSize("Thermal58x150", width, height),
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
                new DataColumn("LogoImage", typeof(byte[])),
                new DataColumn("QRCodeImage", typeof(byte[])),
                new DataColumn("PRALogo", typeof(byte[])),
                new DataColumn("NTN", typeof(string)),
                new DataColumn("Address", typeof(string)),
                new DataColumn("STRN", typeof(string))
            });

            var bodyTable = new DataTable("BodyDataSet");
            bodyTable.Columns.AddRange(new[]
            {
                new DataColumn("InvoiceNo", typeof(string)),
                new DataColumn("SerialNo", typeof(int)),
                new DataColumn("ItemName", typeof(string)),
                new DataColumn("TaxRate", typeof(decimal)),
                new DataColumn("Qty", typeof(decimal)),
                new DataColumn("Price", typeof(decimal)),
                new DataColumn("POSID", typeof(string)),
                new DataColumn("Discount", typeof(decimal)),
                new DataColumn("Total", typeof(decimal)),
                new DataColumn("Tax", typeof(decimal))
            });

            byte[] logo = LoadCompanyLogo();
            byte[] praLogo = LoadPraLogo();
            byte[] qr = GenerateQRCode(dto.USIN);

            var headerRow = headerTable.NewRow();
            headerRow["BusinessName"] = dto.BuyerName;
            headerRow["DateCreated"] = dto.DateTime;
            string paymentModeText = dto.PaymentMode switch
            {
                1 => "Cash",
                2 => "Credit",
                3 => "Online",
                _ => "N/A"
            };
            headerRow["ModeOfPayment"] = paymentModeText;
            //headerRow["ModeOfPayment"] = dto.PaymentMode.ToString() ?? "N/A";
            headerRow["LogoImage"] = logo;
            headerRow["QRCodeImage"] = qr;
            headerRow["PRALogo"] = praLogo;
            headerRow["NTN"] = dto.BuyerNTN ?? string.Empty;
            headerRow["Address"] = dto.BuyerName+ ", City, Pakistan";
            headerRow["STRN"] = dto.FBRInvoiceNumber ?? string.Empty;
            headerTable.Rows.Add(headerRow);

            int serial = 1;
            foreach (var item in dto.InvoiceItemDto ?? Enumerable.Empty<dynamic>())
            {
                var row = bodyTable.NewRow();
                row["InvoiceNo"] = dto.FBRInvoiceNumber ?? string.Empty;
                row["SerialNo"] = serial++;
                row["ItemName"] = item.ItemName ?? string.Empty;
                row["TaxRate"] = Math.Round(item.TaxRate, 2);
                row["Qty"] = item.Quantity;
                row["Price"] = Math.Round(item.SaleValue,2);
                row["POSID"] = dto.POSID.ToString();
                row["Discount"] = Math.Round(item.Discount, 2);
                row["Total"] = Math.Round(item.TotalAmount, 2);
                row["Tax"] = Math.Round(item.TaxCharged, 2);
                bodyTable.Rows.Add(row);
            }

            return (headerTable, bodyTable);
        }

        private byte[] LoadCompanyLogo()
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

            using var qrGen = new QRCodeGenerator();
            using var qrData = qrGen.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q);
            using var qrCode = new QRCode(qrData);
            using var bmp = qrCode.GetGraphic(10);
            using var ms = new MemoryStream();
            bmp.Save(ms, ImageFormat.Png);
            return ms.ToArray();
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
