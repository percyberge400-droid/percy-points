using POSPRA.Application.Services.FiscalService;
using POSPRA.Domain.Entities;
using POSPRA_WinFormsUI.Forms;
using System.Drawing.Drawing2D;
using System.ComponentModel;

namespace POSPRA_WinFormsUI
{
    public partial class InvoiceEntry : Form
    {
        private readonly IServiceProvider _provider;

        // original bounds and parent sizes (captured once at startup)
        private readonly Dictionary<Control, Rectangle> _originalBounds = new Dictionary<Control, Rectangle>();
        private readonly Dictionary<Control, Size> _originalParentSizes = new Dictionary<Control, Size>();
        private readonly Dictionary<Control, Font> _originalFonts = new Dictionary<Control, Font>();
        private Size _originalClientSize = Size.Empty;
        private bool _originalLayoutCaptured = false;
        private readonly IFiscalService _fiscalService;
        private bool IsFormMaximized()
        {
            return this.WindowState == FormWindowState.Maximized;
        }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public static bool Proceeded { get; set; } = false;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public static Invoice CurrentInvoice { get; set; }


        public InvoiceEntry(IServiceProvider provider)
        {
            InitializeComponent();
            _provider = provider;


            // keep behavior you had
            InvoiceEntry_Load();
         


            // Event handlers
            btnProceed.Click += BtnProceed_Click;
            chkSaleInvoice.CheckedChanged += ChkSaleInvoice_CheckedChanged;
            chkDebitInvoice.CheckedChanged += ChkDebitInvoice_CheckedChanged;
            chkRegistered.CheckedChanged += ChkRegistered_CheckedChanged;
            chkUnregistered.CheckedChanged += ChkUnregistered_CheckedChanged;

            // Hook resize event
            this.Resize += InvoiceEntry_Resize;

            // Capture original layout AFTER InitializeComponent and initial load
            CaptureOriginalLayout();

            // Apply rounded corner (will operate if control has non-zero size)
            MakeRoundedControl(btnProceed, 25);
            pnlBasicInfo.Resize += (s, e) => MakeRoundedControl(pnlBasicInfo, 25);

        }

        private Invoice CollectInvoiceData()
        {
            return new Invoice
            {
                // Convert InvoiceType string to short (example: Sale = 1, Debit = 2)
                InvoiceType = chkSaleInvoice.Checked ? (short)1 : (short)2,

                // Invoice date
                InvoiceDate = txtInvoiceDate.Value,

                // Buyer/Seller info
                BuyerSellerName = txtBuyerBusiness.Text,        // You may combine Seller/Buyer logic
                DestinationAddress = txtBuyerAddress.Text,      // Assuming Buyer address goes here
                NTN_CNIC = txtBuyerRegNo.Text,                  // Assuming buyer's NTN/CNIC
                DistributorName = txtSellerBusiness.Text,
                Distributor_NTN_CNIC = txtSellerRegNo.Text,

                // Sale type: Registered = 1, Unregistered = 2 (example)
                SaleType = chkRegistered.Checked ? 1 : 2,

                // Optional: provinces (you may store separately if needed)
                // SellerProvince = cmbSellerProvince.SelectedItem?.ToString() ?? "",
                // BuyerProvince = cmbBuyerProvince.SelectedItem?.ToString() ?? "",

                // Other numeric fields can be calculated or left null if not yet filled
                TotalRetailPrice = 0,       // Replace with actual calculation
                TotalSalesTaxApplicable = null,
                TotalSTWithheldAtSource = null,
                TotalExtraTax = null,
                TotalFEDPayable = null,
                TotalWithheldIncomeTax = null,
                TotalCVT = null
            };
        }

        private void item_entry_Load(object sender, EventArgs e)
        {
            txtInvoiceDate.Value = DateTime.Now;
        }


        // Proceed button click
        // Proceed button click
        private void BtnProceed_Click(object sender, EventArgs e)
        {
            //if (!AreAllFieldsFilled())
            //{
            //    MessageBox.Show("Please fill in all required fields before proceeding.",
            //        "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            //    return;
            //}

            Invoice inputData = CollectInvoiceData();

            try
            {
                // Map form inputs to modal
                var invoice = new Invoice
                {
                    InvoiceType = chkSaleInvoice.Checked ? (short)1 : (short)2,
                    InvoiceDate = txtInvoiceDate.Value,
                    SaleType = chkRegistered.Checked ? 1 : 2,
                    BuyerSellerName = txtBuyerBusiness.Text,
                    DestinationAddress = txtBuyerAddress.Text,
                    NTN_CNIC = txtBuyerRegNo.Text,
                    DistributorName = txtSellerBusiness.Text,
                    Distributor_NTN_CNIC = txtSellerRegNo.Text,
                    TotalRetailPrice = 0,              // Replace with actual calculation
                    TotalSalesTaxApplicable = null,
                    TotalSTWithheldAtSource = null,
                    TotalExtraTax = null,
                    TotalFEDPayable = null,
                    TotalWithheldIncomeTax = null,
                    TotalCVT = null
                };

                // ✅ Save session
                InvoiceEntry.CurrentInvoice = invoice;
                InvoiceEntry.Proceeded = true;

                string msg = $"Proceeding with {(invoice.InvoiceType == 1 ? "Sale" : "Debit")} Invoice\n" +
                             $"Customer Type: {(invoice.SaleType == 1 ? "Registered" : "Unregistered")}\n" +
                             $"Seller: {invoice.DistributorName}, {cmbSellerProvince.SelectedItem?.ToString() ?? ""}\n" +
                             $"Buyer: {invoice.BuyerSellerName}, {cmbBuyerProvince.SelectedItem?.ToString() ?? ""}\n" +
                             $"Invoice Date: {invoice.InvoiceDate:dd-MMM-yyyy}";

                MessageBox.Show(msg, "Invoice Data", MessageBoxButtons.OK, MessageBoxIcon.Information);

                if (this.MdiParent is Main mainForm)
                {
                    this.Close(); // close InvoiceEntry

                    // Open item entry and pass invoice
                    var itemEntryForm = new item_entry(_fiscalService);
                    //var itemEntryForm = new item_entry(_fiscalService, CurrentInvoice);

                    itemEntryForm.MdiParent = mainForm;
                    itemEntryForm.Show();
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }


        private bool AreAllFieldsFilled()
        {
            // check invoice type (at least one checkbox)
            if (!chkSaleInvoice.Checked && !chkDebitInvoice.Checked)
                return false;

            // check customer type
            if (!chkRegistered.Checked && !chkUnregistered.Checked)
                return false;

            // check seller info
            if (string.IsNullOrWhiteSpace(txtSellerBusiness.Text)) return false;
            if (string.IsNullOrWhiteSpace(txtSellerAddress.Text)) return false;
            if (cmbSellerProvince.SelectedItem == null) return false;
            if (string.IsNullOrWhiteSpace(txtSellerRegNo.Text)) return false;

            // check buyer info
            if (string.IsNullOrWhiteSpace(txtBuyerBusiness.Text)) return false;
            if (string.IsNullOrWhiteSpace(txtBuyerAddress.Text)) return false;
            if (cmbBuyerProvince.SelectedItem == null) return false;
            if (string.IsNullOrWhiteSpace(txtBuyerRegNo.Text)) return false;

            // check invoice reference
            if (string.IsNullOrWhiteSpace(txtInvoiceExtra.Text)) return false;

            return true;
        }


        // Toggle Sale Invoice
        private void ChkSaleInvoice_CheckedChanged(object sender, EventArgs e)
        {
            if (chkSaleInvoice.Checked)
                chkDebitInvoice.Checked = false;
        }

        // Toggle Debit Invoice
        private void ChkDebitInvoice_CheckedChanged(object sender, EventArgs e)
        {
            if (chkDebitInvoice.Checked)
                chkSaleInvoice.Checked = false;
        }

        // Toggle Registered
        private void ChkRegistered_CheckedChanged(object sender, EventArgs e)
        {
            if (chkRegistered.Checked)
                chkUnregistered.Checked = false;
        }

        // Toggle Unregistered
        private void ChkUnregistered_CheckedChanged(object sender, EventArgs e)
        {
            if (chkUnregistered.Checked)
                chkRegistered.Checked = false;
        }

        private void InvoiceEntry_Load()
        {
            if (InvoiceEntry.CurrentInvoice != null)
            {
                var saved = InvoiceEntry.CurrentInvoice;

                // Map numeric codes back to checkboxes
                chkSaleInvoice.Checked = saved.InvoiceType == 1;
                chkDebitInvoice.Checked = saved.InvoiceType == 2;

                chkRegistered.Checked = saved.SaleType == 1;
                chkUnregistered.Checked = saved.SaleType == 2;

                // Date and extra reference
                txtInvoiceDate.Value = saved.InvoiceDate;
                //txtInvoiceExtra.Text = saved.InvoiceRef;

                // Seller info
                txtSellerBusiness.Text = saved.DistributorName;
                txtSellerAddress.Text = ""; // No field in modal; optional
                cmbSellerProvince.SelectedItem = ""; // Optional, store separately if needed
                txtSellerRegNo.Text = saved.Distributor_NTN_CNIC;

                // Buyer info
                txtBuyerBusiness.Text = saved.BuyerSellerName;
                txtBuyerAddress.Text = saved.DestinationAddress;
                cmbBuyerProvince.SelectedItem = ""; // Optional, store separately if needed
                txtBuyerRegNo.Text = saved.NTN_CNIC;
            }
            else
            {
                // Default initialization
                if (cmbSellerProvince.Items.Count > 0) cmbSellerProvince.SelectedIndex = 0;
                if (cmbBuyerProvince.Items.Count > 0) cmbBuyerProvince.SelectedIndex = 0;

                chkSaleInvoice.Checked = true;
                chkRegistered.Checked = true;
            }

        }


        public void MakeRoundedControl(Control control, int radius)
        {
            if (control == null) return;

            var rect = control.ClientRectangle;
            if (rect.Width == 0 || rect.Height == 0) return;

            var path = new GraphicsPath();
            path.AddArc(rect.X, rect.Y, radius, radius, 180, 90);                         // Top-left
            path.AddArc(rect.Right - radius, rect.Y, radius, radius, 270, 90);            // Top-right
            path.AddArc(rect.Right - radius, rect.Bottom - radius, radius, radius, 0, 90); // Bottom-right
            path.AddArc(rect.X, rect.Bottom - radius, radius, radius, 90, 90);            // Bottom-left
            path.CloseFigure();

            control.Region = new Region(path);
        }

        /// <summary>
        /// Capture original bounds of controls and the sizes of their parents.
        /// This is used to restore or calculate scaled bounds later.
        /// Also captures original Fonts so we can restore them.
        /// </summary>
        private void CaptureOriginalLayout()
        {
            try
            {
                _originalBounds.Clear();
                _originalParentSizes.Clear();
                _originalFonts.Clear();

                _originalClientSize = this.ClientSize;

                // We'll capture layout for all controls in the form (recursively)
                foreach (var ctrl in GetAllControls(this))
                {
                    // store original bounds relative to parent
                    if (!_originalBounds.ContainsKey(ctrl))
                        _originalBounds[ctrl] = ctrl.Bounds;

                    // store original fonts
                    if (!_originalFonts.ContainsKey(ctrl) && ctrl.Font != null)
                        _originalFonts[ctrl] = ctrl.Font;

                    // store original parent client sizes (one per parent)
                    var parent = ctrl.Parent;
                    if (parent != null && !_originalParentSizes.ContainsKey(parent))
                    {
                        _originalParentSizes[parent] = parent.ClientSize;
                    }
                }

                // Also ensure the form itself is recorded as a 'parent' baseline (if needed)
                if (!_originalParentSizes.ContainsKey(this))
                    _originalParentSizes[this] = this.ClientSize;

                _originalLayoutCaptured = true;
            }
            catch
            {
                // swallow any capture errors — we still try to proceed
                _originalLayoutCaptured = false;
            }
        }

        /// <summary>
        /// Get all controls in the hierarchy (recursively).
        /// </summary>
        private IEnumerable<Control> GetAllControls(Control root)
        {
            foreach (Control c in root.Controls)
            {
                yield return c;
                foreach (var child in GetAllControls(c))
                    yield return child;
            }
        }

        /// <summary>
        /// Resize handler that scales controls proportionally when maximized,
        /// and restores original bounds when not maximized.
        /// Additionally adjusts font sizes and bumps input heights for large screens.
        /// </summary>
        private void InvoiceEntry_Resize(object sender, EventArgs e)
        {
            // ensure we captured original layout
            if (!_originalLayoutCaptured)
            {
                CaptureOriginalLayout();
                if (!_originalLayoutCaptured)
                    return; // nothing to do safely
            }

            if (IsFormMaximized())
            {
                // calculate a global scale factor based on the form client size vs original
                double formScaleX = _originalClientSize.Width > 0 ? this.ClientSize.Width / (double)_originalClientSize.Width : 1.0;
                double formScaleY = _originalClientSize.Height > 0 ? this.ClientSize.Height / (double)_originalClientSize.Height : 1.0;
                double globalScale = Math.Min(formScaleX, formScaleY);

                // For each control we captured, compute scaled bounds (parent-relative)
                foreach (var kv in _originalBounds)
                {
                    var ctrl = kv.Key;
                    var orig = kv.Value;
                    var parent = ctrl.Parent;

                    // Skip if parent disappeared for some reason
                    if (parent == null) continue;

                    // If we didn't capture parent size for this parent, skip
                    if (!_originalParentSizes.TryGetValue(parent, out Size parentOrigSize) || parentOrigSize.Width == 0 || parentOrigSize.Height == 0)
                        continue;

                    // current parent size
                    var parentCurrentSize = parent.ClientSize;

                    double scaleX = parentCurrentSize.Width / (double)parentOrigSize.Width;
                    double scaleY = parentCurrentSize.Height / (double)parentOrigSize.Height;

                    // compute new bounds relative to same parent
                    int newX = (int)Math.Round(orig.X * scaleX);
                    int newY = (int)Math.Round(orig.Y * scaleY);
                    int newW = Math.Max(1, (int)Math.Round(orig.Width * scaleX));
                    int newH = Math.Max(1, (int)Math.Round(orig.Height * scaleY));

                    // bump input control heights a bit more for readability on large screens
                    if (ctrl == btnProceed)
                    {
                        // scale but shrink height a little
                        int minH = Math.Max(1, (int)Math.Round(orig.Height * globalScale));
                        int reducedH = (int)Math.Round(minH * 0.9); // 90% of normal scale
                        newH = Math.Max(1, reducedH);
                    }
                    else if (ctrl is TextBox || ctrl is ComboBox || ctrl is DateTimePicker || ctrl is Button)
                    {
                        int minH = Math.Max(1, (int)Math.Round(orig.Height * globalScale));
                        int paddedH = (int)Math.Round(minH * 1.05);
                        newH = Math.Max(newH, paddedH);
                    }

                    // apply bounds
                    try
                    {
                        ctrl.Bounds = new Rectangle(newX, newY, newW, newH);
                    }
                    catch
                    {
                        // ignore single-control failures
                    }

                    // adjust font size using the global scale so fonts remain proportional
                    try
                    {
                        if (_originalFonts.TryGetValue(ctrl, out Font origFont) && origFont != null)
                        {
                            // compute new font size (points)
                            float newFontSize = (float)(origFont.Size * globalScale);

                            // clamp font size to reasonable range
                            if (newFontSize < 6f) newFontSize = 6f;
                            if (newFontSize > 72f) newFontSize = 72f;

                            // assign scaled font
                            ctrl.Font = new Font(origFont.FontFamily, newFontSize, origFont.Style);
                        }
                    }
                    catch
                    {
                        // ignore font assignment problems
                    }
                }

                // reapply rounded corners (sizes have changed)
                try { MakeRoundedControl(btnProceed, 25); } catch { }
            }
            else
            {
                // Restore original bounds precisely and restore original fonts
                foreach (var kv in _originalBounds)
                {
                    var ctrl = kv.Key;
                    var orig = kv.Value;

                    try
                    {
                        ctrl.Bounds = orig;
                    }
                    catch
                    {
                        // ignore individual restore errors
                    }
                }

                // restore original fonts
                foreach (var kv in _originalFonts)
                {
                    var ctrl = kv.Key;
                    var origFont = kv.Value;
                    try
                    {
                        if (origFont != null)
                            ctrl.Font = origFont;
                    }
                    catch
                    {
                        // ignore
                    }
                }

                // reapply rounded corners (size may be original now)
                try { MakeRoundedControl(btnProceed, 25); } catch { }
            }
        }
    }
}
