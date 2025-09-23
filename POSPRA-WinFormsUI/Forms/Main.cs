using Microsoft.Extensions.DependencyInjection;
using POSPRA.Application.Services.FiscalService;
using System.Net.NetworkInformation;

namespace POSPRA_WinFormsUI.Forms
{
    public partial class Main : Form
    {
        private readonly IServiceProvider _provider;
        private readonly IFiscalService _fiscalService;

        private CancellationTokenSource _internetCheckCts;

        // Track non-MDI forms separately
        private readonly List<Form> _independentForms = new List<Form>();

        public Main(IServiceProvider provider, IFiscalService fiscalService)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
            _fiscalService = fiscalService ?? throw new ArgumentNullException(nameof(fiscalService));

            InitializeComponent();
            this.IsMdiContainer = true;

            panInvoiceSelection.Visible = false;
            panExportInvoice.Visible = false;

            btnDashboard.ForeColor = ColorTranslator.FromHtml("#686DF4"); // Highlight color

            // Load Dashboard as default child
            Form childForm = _provider.GetRequiredService<DashboardForm>();
            childForm.MdiParent = this;
            childForm.Dock = DockStyle.Fill;
            childForm.Show();

            // Handle form state changes to manage independent forms
            this.Resize += Main_Resize;

            // Start internet checker
            StartInternetStatusChecker();

            // Placeholder for worker service
            lblWorkerService.Text = "Worker Service: -";
            lblWorkerService.Font = new Font(lblWorkerService.Font, FontStyle.Italic);
            lblWorkerService.ForeColor = Color.Gray;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            _internetCheckCts?.Cancel();

            // Close all independent forms
            foreach (var form in _independentForms.ToArray())
            {
                if (form != null && !form.IsDisposed)
                {
                    form.Close();
                }
            }
        }

        private void Main_Resize(object sender, EventArgs e)
        {
            HandleFormStateChange();
        }

        private void Main_WindowStateChanged(object sender, EventArgs e)
        {
            HandleFormStateChange();
        }

        private void HandleFormStateChange()
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                // When main form is minimized, hide independent forms but don't minimize them
                foreach (var form in _independentForms)
                {
                    if (form != null && !form.IsDisposed && form.Visible)
                    {
                        form.Hide();
                        form.Tag = "was_visible"; // Mark as previously visible
                    }
                }
            }
            else if (this.WindowState == FormWindowState.Normal || this.WindowState == FormWindowState.Maximized)
            {
                // When main form is restored, show previously visible independent forms
                foreach (var form in _independentForms)
                {
                    if (form != null && !form.IsDisposed && form.Tag?.ToString() == "was_visible")
                    {
                        form.Show();
                        form.BringToFront();
                        form.Tag = null; // Clear the marker
                    }
                }
            }
        }

        // Method to open forms as independent windows (non-MDI)
        private void OpenIndependentForm<T>() where T : Form
        {
            // Check if form of this type is already open
            foreach (var existingForm in _independentForms)
            {
                if (existingForm is T && !existingForm.IsDisposed)
                {
                    if (!existingForm.Visible)
                        existingForm.Show();
                    existingForm.BringToFront();
                    if (existingForm.WindowState == FormWindowState.Minimized)
                        existingForm.WindowState = FormWindowState.Normal;
                    return;
                }
            }

            // Create new instance if not found
            var newForm = _provider.GetRequiredService<T>();

            // Make the form independent (not MDI)
            newForm.MdiParent = null;
            newForm.ShowInTaskbar = true;
            newForm.StartPosition = FormStartPosition.CenterScreen;
            newForm.FormBorderStyle = FormBorderStyle.Sizable; // Allow normal window operations

            // Add event handlers to track form lifecycle
            newForm.FormClosed += (s, e) =>
            {
                _independentForms.Remove(newForm);
            };

            // Add to tracking list
            _independentForms.Add(newForm);

            // Show the form
            newForm.Show();
            newForm.BringToFront();
        }

        // -----------------------------
        // INTERNET STATUS CHECKER
        // -----------------------------

        private void StartInternetStatusChecker()
        {
            _internetCheckCts = new CancellationTokenSource();
            CancellationToken ct = _internetCheckCts.Token;

            _ = Task.Run(async () =>
            {
                // Run immediately once
                await UpdateInternetStatusAsync();

                while (!ct.IsCancellationRequested)
                {
                    try
                    {
                        await Task.Delay(2000, ct); // check every 2s
                        await UpdateInternetStatusAsync();
                    }
                    catch (TaskCanceledException)
                    {
                        break;
                    }
                }
            }, ct);
        }

        private async Task UpdateInternetStatusAsync()
        {
            bool online = await CheckInternetConnectivityAsync();

            if (lblNetworkStatus != null && lblNetworkStatus.IsHandleCreated)
            {
                lblNetworkStatus.BeginInvoke(new Action(() =>
                {
                    lblNetworkStatus.Text = $"Network Status: {(online ? "Online" : "Offline")}";
                    lblNetworkStatus.Font = new Font(lblNetworkStatus.Font, FontStyle.Bold);
                    lblNetworkStatus.ForeColor = online ? Color.Green : Color.Red;
                }));
            }
        }

        private async Task<bool> CheckInternetConnectivityAsync()
        {
            try
            {
                using var ping = new Ping();
                var reply = await ping.SendPingAsync("8.8.8.8", 2000); // 2s timeout
                return reply.Status == IPStatus.Success;
            }
            catch
            {
                return false;
            }
        }

        private void ResetNavStyles()
        {
            btnDashboard.ForeColor = Color.Black;
            btnInvoiceSelection.ForeColor = Color.Black;
            btnExportInvoice.ForeColor = Color.Black;

            panDashboard.Visible = false;
            panInvoiceSelection.Visible = false;
            panExportInvoice.Visible = false;
        }

        private void btnDashboard_Click(object sender, EventArgs e)
        {
            ResetNavStyles();
            btnDashboard.ForeColor = ColorTranslator.FromHtml("#686DF4");
            panDashboard.Visible = true;

            // Option 1: Load as MDI child (embedded in main form)
            LoadView("Dashboard");

            // Option 2: Load as independent window (uncomment the line below and comment the line above)
            // OpenIndependentForm<DashboardForm>();
        }

        public void LoadView(string v)
        {
            // Close current active MDI child if any
            this.ActiveMdiChild?.Close();

            Form childForm = null;

            switch (v)
            {
                case "Dashboard":
                    childForm = _provider.GetRequiredService<DashboardForm>();
                    break;

                case "Invoice Entry":
                    childForm = _provider.GetRequiredService<item_entry>();
                    break;

                case "Invoice Export":
                    childForm = _provider.GetRequiredService<ExportInvoiceForm>();
                    break;

                default:
                    // Unknown option → do nothing
                    return;
            }

            // ✅ Show the new child form as MDI
            if (childForm != null)
            {
                childForm.MdiParent = this;
                childForm.WindowState = FormWindowState.Maximized;
                childForm.Show();


                if (childForm != null)
                {
                    childForm.TopLevel = false; // remove title bar
                    childForm.FormBorderStyle = FormBorderStyle.None;
                    childForm.ControlBox = false;
                    childForm.MaximizeBox = false;
                    childForm.MinimizeBox = false;
                    childForm.ShowInTaskbar = false;
                    childForm.Text = "";
                    childForm.Dock = DockStyle.Fill;
                    childForm.WindowState = FormWindowState.Maximized;

                    // Set as MDI child
                    childForm.MdiParent = this;
                    childForm.Dock = DockStyle.Fill;
                    childForm.Show();
                }
            }
        }


        private void btnInvoiceSelection_Click(object sender, EventArgs e)
        {
            ResetNavStyles();
            btnInvoiceSelection.ForeColor = ColorTranslator.FromHtml("#686DF4");
            panInvoiceSelection.Visible = true;

            // Option 1: Load as MDI child (embedded in main form)
            LoadView("Invoice Entry");

            // Option 2: Load as independent window (uncomment the line below and comment the line above)
            // OpenIndependentForm<item_entry>();
        }

        private void btnExportInvoice_Click(object sender, EventArgs e)
        {
            ResetNavStyles();
            btnExportInvoice.ForeColor = ColorTranslator.FromHtml("#686DF4");
            panExportInvoice.Visible = true;
            LoadView("Invoice Export");
        }

        private void btnItemEntry_Click(object sender, EventArgs e)
        {
            ResetNavStyles();
            LoadView("Item Entry");
        }
    }
}