using Microsoft.Extensions.DependencyInjection;
using POSPRA.Application.Services.FiscalService;
using POSPRA_WinFormsUI.AlertClasses;

namespace POSPRA_WinFormsUI.Forms
{
    public partial class Main : Form
    {
        private readonly IServiceProvider _provider;
        private readonly IFiscalService _fiscalService;
        private ServerConnectionChecker _connectionChecker;

        public Main(IServiceProvider provider, IFiscalService fiscalService)
        {
            InitializeComponent();
            _provider = provider;
            _fiscalService = fiscalService;

            this.IsMdiContainer = true;

            panInvoiceSelection.Visible = false;
            panItemEntry.Visible = false;
            panExportInvoice.Visible = false;

            btnDashboard.ForeColor = ColorTranslator.FromHtml("#686DF4"); // Highlight color

            // Load Dashboard as default child
            Form childForm = _provider.GetRequiredService<DashboardForm>();
            childForm.MdiParent = this;
            childForm.Dock = DockStyle.Fill;
            childForm.Show();

            // 🔌 Initialize connection checker at app start
            InitializeConnectionChecker();
        }

        private void ResetNavStyles()
        {
            btnDashboard.ForeColor = Color.Black;
            btnInvoiceSelection.ForeColor = Color.Black;
            btnItemEntry.ForeColor = Color.Black;
            btnExportInvoice.ForeColor = Color.Black;

            panDashboard.Visible = false;
            panInvoiceSelection.Visible = false;
            panItemEntry.Visible = false;
            panExportInvoice.Visible = false;
        }

        private void btnDashboard_Click(object sender, EventArgs e)
        {
            ResetNavStyles();
            btnDashboard.ForeColor = ColorTranslator.FromHtml("#686DF4");
            panDashboard.Visible = true;

            LoadView("Dashboard");
        }

        public void LoadView(string v)
        {
            // Close currently active child form if one exists
            if (this.ActiveMdiChild != null)
            {
                this.ActiveMdiChild.Close();
            }

            Form childForm = null;

            switch (v)
            {
                case "Dashboard":
                    childForm = _provider.GetRequiredService<DashboardForm>();
                    break;

                case "Invoice Selection":
                    //childForm = new invoice_entry(_fiscalService);
                    break;

                case "Invoice Entry":
                    childForm = _provider.GetRequiredService<item_entry>();
                    break;

                case "Invoice Export":
                    //childForm = new InvoiceExportForm();
                    break;
            }

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

        private void btnInvoiceSelection_Click(object sender, EventArgs e)
        {
            ResetNavStyles();
            btnInvoiceSelection.ForeColor = ColorTranslator.FromHtml("#686DF4");
            panInvoiceSelection.Visible = true;

            LoadView("Invoice Entry");
        }

        private void btnExportInvoice_Click(object sender, EventArgs e)
        {
            ResetNavStyles();
            btnExportInvoice.ForeColor = ColorTranslator.FromHtml("#686DF4");
            panExportInvoice.Visible = true;
        }

        private void btnItemEntry_Click(object sender, EventArgs e)
        {
            ResetNavStyles();
            btnItemEntry.ForeColor = ColorTranslator.FromHtml("#686DF4");
            panItemEntry.Visible = true;

            LoadView("Item Entry");
        }

        private void Main_Load(object sender, EventArgs e)
        {
            // 🔒 Disable Item Entry at startup
            btnItemEntry.Enabled = false;
            btnItemEntry.ForeColor = Color.Gray;
        }

        // ===============================
        // 🔌 NETWORK CONNECTION HANDLER
        // ===============================
        private void InitializeConnectionChecker()
        {
            _connectionChecker = new ServerConnectionChecker();

            _connectionChecker.OnNetworkAvailable += msg =>
                _ = LogAndNotifyAsync("Network Status", msg, AlertType.Success);

            _connectionChecker.OnNetworkUnavailable += msg =>
                _ = LogAndNotifyAsync("Network Status", msg, AlertType.Error);

            _connectionChecker.OnInternetConnected += msg =>
                _ = LogAndNotifyAsync("Internet Status", msg, AlertType.Success);

            _connectionChecker.OnInternetDisconnected += msg =>
                _ = LogAndNotifyAsync("Internet Status", msg, AlertType.Warning);

            _connectionChecker.OnIpChanged += msg =>
                _ = LogAndNotifyAsync("Network Status", msg, AlertType.Info);
        }

        private async Task LogAndNotifyAsync(string title, string message, AlertType alertType)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action(async () =>
                    await LogAndNotifyAsync(title, message, alertType)));
                return;
            }

            // Windows Notification
            WindowsLocalAppNotification.Show(title, message);

            switch (alertType)
            {
                case AlertType.Success:
                    AlertManager.ShowSuccess(message);
                    break;
                case AlertType.Error:
                    AlertManager.ShowError(message);
                    break;
                case AlertType.Warning:
                    AlertManager.ShowWarning(message);
                    break;
                case AlertType.Info:
                    AlertManager.ShowInfo(message);
                    break;
                case AlertType.Critical:
                    AlertManager.ShowCritical(message);
                    break;
                case AlertType.Update:
                    AlertManager.ShowUpdate(message);
                    break;
            }

            await Task.CompletedTask;
        }
    }
}
