using POSPRA.Application.Services.FiscalService;
using POSPRA.Application.Services.LogService;
using POSPRA_WinFormsUI.AlertClasses;
using System;
using System.Data;
using System.Diagnostics;
using System.Windows.Forms;
using System.Net.NetworkInformation;

namespace POSPRA_WinFormsUI.Forms
{
    public partial class DashboardForm : Form
    {
        private readonly IServiceProvider _provider;
        private readonly ILogService _logService;
        private readonly IFiscalService _fiscalService;

        public object ServerConnectionChecker { get; private set; }
        private ServerConnectionChecker networkWatcher;

        public DashboardForm(IServiceProvider provider, ILogService logService, IFiscalService fiscalService)
        {
            InitializeComponent();
            this.Load += DashboardForm_Load;

            this.AutoScaleMode = AutoScaleMode.Dpi;
            this.AutoScroll = true;
            this.MinimumSize = new Size(1024, 600);

            _provider = provider;
            _fiscalService = fiscalService ?? throw new ArgumentNullException(nameof(fiscalService));
            _logService = logService ?? throw new ArgumentNullException(nameof(logService));

            // Remove form chrome
            this.FormBorderStyle = FormBorderStyle.None;
            this.ControlBox = false;
            this.ShowIcon = false;
            this.Text = string.Empty;
            InitializeComponent();

            _fiscalService = fiscalService;
            _logService = logService ?? throw new ArgumentNullException(nameof(logService));

                InitializeConnectionChecker();
        }

        private void LogoutUser(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void OpenSettings(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private async void DashboardForm_Load(object sender, EventArgs e)
        {
            try
            {
                // Load data into grids
                await LoadAndShowInvoicesAsync();
                await LoadAndShowLogsAsync();

                // Handle grid errors gracefully
                InvoicesDataGridView.DataError += dataGridView_DataError;
                LogsDataGridView.DataError += dataGridView_DataError;
            }
            catch (Exception ex)
            {
                WindowsLocalAppNotification.Show("Dashboard Error", $"Error loading dashboard: {ex.Message}");
                AlertManager.ShowError($"Error loading dashboard: {ex.Message}");
            }
        }

        private async Task LoadAndShowInvoicesAsync()
        {
            try
            {
                var response = await _fiscalService.GetAllAsync();

                //Local App notiofication call
                WindowsLocalAppNotification.Show("Invoices Loaded", $"Successfully loaded Invoice entries");
                AlertManager.ShowSuccess("Successfuly loaded Invoice data");

                if (response?.Data == null || !response.Data.Any())
                {
                    MessageBox.Show("No invoices found.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    //Local App notiofication call
                    WindowsLocalAppNotification.Show("Error", $"Invoice Entries Not Loaded");
                    AlertManager.ShowError("Failed to load Invoice data");

                    return;
                }

                InvoicesDataGridView.Rows.Clear();

                foreach (var inv in response.Data.OrderByDescending(i => i.DateCreated))
                {
                    int rowIndex = InvoicesDataGridView.Rows.Add();
                    DataGridViewRow row = InvoicesDataGridView.Rows[rowIndex];

                    // Map entity fields to grid columns
                    row.Cells["colId"].Value = inv.ID;
                    row.Cells["colPosId"].Value = inv.POSID;
                    row.Cells["colInvoiceData"].Value = inv.InvoiceData ?? "N/A";
                    row.Cells["colInvoiceNumber"].Value = inv.InvoiceNumber ?? "N/A";
                    row.Cells["colIsSynced"].Value = inv.IsSynced == 1 ? "Yes" : "No";
                    row.Cells["colAttemptCount"].Value = inv.AttemptCount;
                    row.Cells["colDateCreated"].Value = inv.DateCreated.ToString("yyyy-MM-dd");

                    // Keep useful flags in row.Tag
                    row.Tag = new { inv.IsSynced, inv.AttemptCount };
                }

                // Update dashboard summary labels
                labelAllInvoices.Text = InvoicesDataGridView.Rows.Count.ToString();

                labelPendingInvoice.Text = InvoicesDataGridView.Rows
                    .Cast<DataGridViewRow>()
                    .Count(r => ((dynamic)r.Tag).IsSynced == 0)
                    .ToString();

                labelPaidInvoices.Text = InvoicesDataGridView.Rows
                    .Cast<DataGridViewRow>()
                    .Count(r => ((dynamic)r.Tag).IsSynced == 1)
                    .ToString();

                labelInProgressInvc.Text = InvoicesDataGridView.Rows
                    .Cast<DataGridViewRow>()
                    .Count(r => ((dynamic)r.Tag).AttemptCount > 0)
                    .ToString();

                WindowsLocalAppNotification.Show("Invoices", "Invoices loaded successfully");
                AlertManager.ShowSuccess("Invoices loaded successfully");
            }
            catch (Exception ex)
            {
                WindowsLocalAppNotification.Show("Invoices Error", $"Error loading invoices: {ex.Message}");
                AlertManager.ShowError($"Error loading invoices: {ex.Message}");
            }
        }

        private async Task LoadAndShowLogsAsync()
        {
            try
            {
                var response = await _logService.GetAllAsync();

                if (response?.Data != null && response.Data.Any())
                {
                    LogsDataGridView.Rows.Clear();

                    // Order logs by Id descending
                    foreach (var log in response.Data.OrderByDescending(l => l.Id))
                    {
                        int rowIndex = LogsDataGridView.Rows.Add();
                        var row = LogsDataGridView.Rows[rowIndex];
                        row.Cells["colID"].Value = log.Id;
                        row.Cells["colMessage"].Value = log.Message ?? "No message";
                        row.Cells["colException"].Value = log.Type ?? "N/A";
                        row.Cells["logdatetime"].Value = log.CreatedAtPk.ToString("dd-MM-yyyy HH:mm:ss");
                    }

                    WindowsLocalAppNotification.Show("Logs", "Logs loaded successfully");
                    AlertManager.ShowSuccess("Logs loaded successfully");
                }
                else
                {
                    LogsDataGridView.Rows.Clear(); // better than null to avoid DataError
                    WindowsLocalAppNotification.Show("Logs", "No logs available to display");
                    AlertManager.ShowWarning("No logs available to display");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading logs:\n{ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);


                //Local App notiofication call
                WindowsLocalAppNotification.Show("Error", $"Failed to Load Log entries");
                AlertManager.ShowSuccess("Failed to Load Log entires");

                //var logEntry = _logService.BuildLog(msg, AlertType.Error, "Dashboard");
                //await _logService.LogAsync(logEntry);
            }
        }

        private void InitializeConnectionChecker()
        {
            var checker = new ServerConnectionChecker();

            checker.OnNetworkAvailable += msg =>
                _ = LogAndNotifyAsync("Network Status", msg, AlertClasses.AlertType.Success);

            checker.OnNetworkUnavailable += msg =>
                _ = LogAndNotifyAsync("Network Status", msg, AlertClasses.AlertType.Error);

            checker.OnInternetConnected += msg =>
                _ = LogAndNotifyAsync("Internet Status", msg, AlertClasses.AlertType.Success);

            checker.OnInternetDisconnected += msg =>
                _ = LogAndNotifyAsync("Internet Status", msg, AlertClasses.AlertType.Warning);

            checker.OnIpChanged += msg =>
                _ = LogAndNotifyAsync("Network Status", msg, AlertClasses.AlertType.Info);
        }
        private async Task LogAndNotifyAsync(string title, string message, AlertClasses.AlertType alertType)
        {
            if (this.InvokeRequired)
            {
                //this.Invoke(new Action(() => _ = LogAndNotifyAsync(title, message, alertType)));
                this.BeginInvoke(new Action(async () =>
                    await LogAndNotifyAsync(title, message, alertType)));
                return;
            }

            // 1. Show Windows notification
            WindowsLocalAppNotification.Show(title, message);

            switch (alertType)
            {
                case AlertClasses.AlertType.Success:
                    AlertManager.ShowSuccess(message);
                    break;
                case AlertClasses.AlertType.Error:
                    AlertManager.ShowError(message);
                    break;
                case AlertClasses.AlertType.Warning:
                    AlertManager.ShowWarning(message);
                    break;
                case AlertClasses.AlertType.Info:
                    AlertManager.ShowInfo(message);
                    break;
                case AlertClasses.AlertType.Critical:
                    AlertManager.ShowCritical(message);
                    break;
                case AlertClasses.AlertType.Update:
                    AlertManager.ShowUpdate(message);
                    break;
                
            }
            await Task.CompletedTask;

            // 3. Log

        }


        private void dataGridView_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            e.ThrowException = false;

            if (sender is DataGridView grid && e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                grid.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = "N/A";
            }
        }
    }
}
