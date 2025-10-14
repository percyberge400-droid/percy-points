using Microsoft.Reporting.WinForms;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using POSPRA.DTOs.InvoiceDtos;
using QRCoder;
using System;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Security;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace POSPRA_WinFormsUI.Forms
{
    public partial class InvoiceReport : Form
    {
        private readonly InvoiceDto _invoiceDto;  // for Save button use
        private readonly string _invoiceNumber;   // for Dashboard button use
        private readonly bool _isFromDashboard;   // flag to handle two data sources
        private ReportViewer _reportViewer;

        
        // Primary constructor (called from BtnSave_Click)
        public InvoiceReport(InvoiceDto invoiceDto)
        {
            InitializeComponent();
            _invoiceDto = invoiceDto ?? throw new ArgumentNullException(nameof(invoiceDto));
            _isFromDashboard = false;
            InitializeReportViewer();
        }

        // Constructor #2 → called when printing from Dashboard (only invoice number is available)
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
            // optional — set zoom mode
            _reportViewer.SetDisplayMode(DisplayMode.PrintLayout);
            _reportViewer.ZoomMode = ZoomMode.PageWidth;
    
            Controls.Add(_reportViewer);
            LoadReport();
        }

        
        #endregion



        #region Data Preparation

        // Build two datasets for RDLC: HeaderDataSet + BodyDataSet
        // Builds two DataTables: HeaderDataSet + BodyDataSet for RDLC binding
        private (DataTable Header, DataTable Body) BuildInvoiceDataSets(InvoiceDto dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto), "Invoice data is missing.");

            // === HEADER TABLE ===
            var headerTable = new DataTable("HeaderDataSet");
            headerTable.Columns.Add("BusinessName", typeof(string));
            headerTable.Columns.Add("DateCreated", typeof(DateTime));
            headerTable.Columns.Add("ModeOfPayment", typeof(string));
            headerTable.Columns.Add("LogoImage", typeof(byte[]));
            headerTable.Columns.Add("QRCodeImage", typeof(byte[]));
            headerTable.Columns.Add("PRALogo", typeof(byte[]));
            headerTable.Columns.Add("NTN", typeof(string));
            headerTable.Columns.Add("Address", typeof(string));
            headerTable.Columns.Add("STRN", typeof(string));

            // === BODY TABLE ===
            var bodyTable = new DataTable("BodyDataSet");
            bodyTable.Columns.Add("InvoiceNo", typeof(string));
            bodyTable.Columns.Add("SerialNo", typeof(int));
            bodyTable.Columns.Add("itemName", typeof(string));
            bodyTable.Columns.Add("TaxRate", typeof(decimal));
            bodyTable.Columns.Add("Qty", typeof(decimal));
            bodyTable.Columns.Add("Price", typeof(decimal));
            bodyTable.Columns.Add("POSID", typeof(string));
            bodyTable.Columns.Add("Discount", typeof(decimal));
            bodyTable.Columns.Add("Total", typeof(decimal));
            bodyTable.Columns.Add("Tax", typeof(decimal));

            // === LOGO + QR ===
            byte[] logoImage = LoadCompanyLogo();
            byte[] qrImage = GenerateQRCode(dto.USIN.ToString());
            byte[] PRALogo = LoadPraLogo();

            // === HEADER ROW ===
            var headerRow = headerTable.NewRow();
            headerRow["BusinessName"] = "Your Business Name"; // TODO: replace with actual
            headerRow["DateCreated"] = dto.DateTime;
            headerRow["ModeOfPayment"] = dto.PaymentMode.ToString() ?? "N/A";
            headerRow["LogoImage"] = logoImage;
            headerRow["QRCodeImage"] = qrImage;
            headerRow["PRALogo"] = PRALogo;
            headerRow["NTN"] = dto.BuyerNTN ?? string.Empty;
            headerRow["Address"] = "Your Business Address, City, Pakistan";
            headerRow["STRN"] = dto.USIN ?? string.Empty;
            headerRow["STRN"] = dto.FBRInvoiceNumber ?? string.Empty;
            headerTable.Rows.Add(headerRow);

            // === BODY ROWS ===
            if (dto.InvoiceItemDto != null && dto.InvoiceItemDto.Any())
            {
                int serial = 1;
                foreach (var item in dto.InvoiceItemDto)
                {
                    var row = bodyTable.NewRow();
                    row["InvoiceNo"] = dto.USIN ?? string.Empty;
                    row["SerialNo"] = serial++;
                    row["itemName"] = item.ItemName ?? string.Empty;
                    row["TaxRate"] = item.TaxCharged;
                    row["Qty"] = item.Quantity;
                    row["Price"] = item.SaleValue;
                    row["POSID"] = dto.POSID.ToString() ?? string.Empty;
                    row["Discount"] = item.Discount;
                    row["Total"] = item.TotalAmount;
                    row["Tax"] = item.TaxCharged;
                    bodyTable.Rows.Add(row);
                }
            }

            return (headerTable, bodyTable);
        }


        // === Helper: Load your logo from resources or disk ===
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
            if (string.IsNullOrEmpty(text)) return null;

            using var qrGen = new QRCodeGenerator();
            using var qrData = qrGen.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q);
            using var qrCode = new QRCode(qrData);
            using var bmp = qrCode.GetGraphic(20);
            using var ms = new MemoryStream();
            bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            return ms.ToArray();
        }
        #endregion
        
        #region Report Loading
        private void LoadReport()
        {
            try
            {
                // 1️⃣ Find the RDLC file path
                string reportPath = GetReportPath();
                if (string.IsNullOrEmpty(reportPath))
                {
                    MessageBox.Show("⚠️ Report file not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // 2️⃣ Build both datasets from the InvoiceDto
                var (header, body) = BuildInvoiceDataSets(_invoiceDto);

                if (header.Rows.Count == 0 || body.Rows.Count == 0)
                {
                    MessageBox.Show("⚠️ No invoice data available.", "No Data", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // 3️⃣ Configure and bind the ReportViewer
                _reportViewer.LocalReport.ReportPath = reportPath;
                _reportViewer.LocalReport.DataSources.Clear();

                _reportViewer.LocalReport.DataSources.Add(new ReportDataSource("HeaderDataSet", header));
                _reportViewer.LocalReport.DataSources.Add(new ReportDataSource("BodyDataSet", body));

                // 4️⃣ Refresh to display the data
                _reportViewer.RefreshReport();
                MessageBox.Show($"Header rows: {header.Rows.Count}, Body rows: {body.Rows.Count},Invoice Number {_invoiceNumber}");

            }
            catch (Exception ex)
            {
                MessageBox.Show($"⚠️ Failed to load report:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string GetReportPath(string reportFileName = "InvoiceReport.rdlc")
        {
            try
            {
                //  App base path (build or published folder)
                string basePath = AppDomain.CurrentDomain.BaseDirectory;

                // Look in "Reports" folder if exists (recommended folder structure)
                string reportDir = Path.Combine(basePath, "Forms");
                string reportPath = Path.Combine(reportDir, reportFileName);

                // Try direct path inside base directory
                if (!File.Exists(reportPath))
                    reportPath = Path.Combine(basePath, reportFileName);

                // Fallback to known dev location
                string fallbackPath = @"D:\pra-pos\POSPRA-WinFormsUI\Forms\" + reportFileName;

                if (File.Exists(reportPath))
                    return reportPath;
                else if (File.Exists(fallbackPath))
                    return fallbackPath;

                // File not found case
                MessageBox.Show(
                    $"❌ Report file not found.\n\nSearched paths:\n{reportPath}\n{fallbackPath}",
                    "Missing Report",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );

                return string.Empty;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error resolving report path:\n{ex.Message}",
                    "Path Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return string.Empty;
            }
        }

        #endregion

        #region Rendering & Printing
        private void OnRenderingComplete(object sender, RenderingCompleteEventArgs e)
        {
            try
            {
                _reportViewer.RenderingComplete -= OnRenderingComplete;
                //PrintReport();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"⚠️ Rendering failed: {ex.Message}", "Render Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        
        // Methods for Thermal printer
        private byte[] RenderReportToImage()
        {
            const string deviceInfo = @"
        <DeviceInfo>
            <OutputFormat>PNG</OutputFormat>
            <DpiX>200</DpiX>
            <DpiY>200</DpiY>
            <PageWidth>5.8cm</PageWidth>
            <PageHeight>10cm</PageHeight>
            <MarginTop>0.2cm</MarginTop>
            <MarginLeft>0.2cm</MarginLeft>
            <MarginRight>0.2cm</MarginRight>
            <MarginBottom>0.2cm</MarginBottom>
        </DeviceInfo>";

            Warning[] warnings;
            string[] streamIds;
            string mimeType, encoding, extension;

            // Render report to PNG image format
            var renderedBytes = _reportViewer.LocalReport.Render(
                "Image",
                deviceInfo,
                out mimeType,
                out encoding,
                out extension,
                out streamIds,
                out warnings
            );

            if (renderedBytes == null || renderedBytes.Length == 0)
                throw new InvalidOperationException("Rendered report is empty.");

            return renderedBytes;
        }

        private string SaveReportImage(byte[] imageBytes)
        {
            try
            {
                string fileName = $"Invoice_{_invoiceDto?.USIN ?? DateTime.Now.Ticks.ToString()}.png";
                string folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "POS_Invoices");

                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                string imagePath = Path.Combine(folder, fileName);
                File.WriteAllBytes(imagePath, imageBytes);

                return imagePath;
            }
            catch
            {
                return "N/A"; // skip saving failure silently
            }
        }

        private PrintDocument CreatePrintDocument(Image image)
        {
            var document = new PrintDocument();

            // Use default system printer (or change here if needed)
            document.PrinterSettings = new PrinterSettings
            {
                PrinterName = new PrinterSettings().PrinterName
            };

            // Configure for 58mm thermal printer
            int widthHundredths = (int)Math.Round(58.0 / 25.4 * 100.0); // 58mm → hundredths of inch
            var paperSize = new PaperSize("Thermal58", widthHundredths, 20000); // arbitrary long height
            document.DefaultPageSettings.PaperSize = paperSize;
            document.DefaultPageSettings.Landscape = false;

            document.PrintPage += (s, e) =>
            {
                // Calculate scaling to maintain aspect ratio
                float scale = (float)e.PageBounds.Width / image.Width;
                int scaledHeight = (int)(image.Height * scale);

                e.Graphics.DrawImage(image, 0, 0, e.PageBounds.Width, scaledHeight);
            };

            return document;
        }

        private void ShowPrintPreview(PrintDocument document)
        {
            using var preview = new PrintPreviewDialog
            {
                Document = document,
                Width = 900,
                Height = 700,
                StartPosition = FormStartPosition.CenterScreen
            };

            preview.ShowIcon = false;
            preview.Text = "Invoice Print Preview";

            preview.ShowDialog();

            if (MessageBox.Show("🖨️ Print now?", "Confirm Print",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {
                    document.Print();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Printer Error: {ex.Message}", "Print Failed",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        #endregion
    }
}