using Microsoft.Azure.Documents;
using POSPRA.Application.Services.FiscalService;
using POSPRA.Application.Services.LogService;
using POSPRA.Application.Services.POSService;
using POSPRA.Application.Utility;
using POSPRA.Domain.Entities;
using POSPRA.DTOs.InvoiceDTOs;
using POSPRA.DTOs.LogDTOs;
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

            _provider = provider;
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
            //var logs = await _logservice.GetLogs();
            var invoices = await _fiscalService.GetAllAsync();
            await LoadAndShowInvoicesAsync();
            await LoadAndShowLogsAsync();

            InvoicesDataGridView.DataError += dataGridView_DataError;
            LogsDataGridView.DataError += dataGridView_DataError;
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

                int srNo = 1;
                foreach (var inv in response.Data.OrderByDescending(i => i.DateCreated))
                {
                    int rowIndex = InvoicesDataGridView.Rows.Add();
                    DataGridViewRow row = InvoicesDataGridView.Rows[rowIndex];


                    row.Cells["colSrNo"].Value = srNo++;
                    row.Cells["colInvoiceNo"].Value = inv.InvoiceNumber ?? "N/A";
                    row.Cells["colPOSID"].Value = inv.POSID;
                    row.Cells["colInvoiceSynced"].Value = inv.IsSynced == 1 ? "Yes" : "No";
                    row.Cells["colDueDate"].Value = inv.DateCreated.ToString("yyyy-MM-dd");
                    //row.Cells["colTotal"].Value = inv.colTotal(ToString());// filerecord DTO doesnot contain the colTotal
                    row.Cells["colStatus"].Value = inv.AttemptCount > 0 ? "Retrying" : "New";
                    //row.Cells["colSyncStatus"].Value = inv.IsSynced == 1 ? "Synced" : "Pending";


                    row.Tag = new { inv.IsSynced, inv.AttemptCount };
                }


                labelAllInvoices.Text = InvoicesDataGridView.Rows.Count.ToString();
                labelPendingInvoice.Text = InvoicesDataGridView.Rows
                    .Cast<DataGridViewRow>()
                    .Count(r => ((dynamic)r.Tag).IsSynced == 0).ToString();

                labelPaidInvoices.Text = InvoicesDataGridView.Rows
                    .Cast<DataGridViewRow>()
                    .Count(r => ((dynamic)r.Tag).IsSynced == 1).ToString();

                labelInProgressInvc.Text = InvoicesDataGridView.Rows
                    .Cast<DataGridViewRow>()
                    .Count(r => ((dynamic)r.Tag).AttemptCount > 0).ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading invoices: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task LoadAndShowLogsAsync()
        {
            try
            {
                var response = await _logService.GetAllAsync();

                //Local App notiofication call
                WindowsLocalAppNotification.Show("Logs Loaded", $"Successfully loaded log entries");
                AlertManager.ShowSuccess("Successfuly loaded Log data");



                if (response?.Data != null && response.Data.Any())
                {
                    var logGridData = response.Data
                        .Select(log => new
                        {
                            colMessage = log.Message ?? "No message",
                            SyncedStatus = "N/A" // Placeholder, adjust if LogDto has something useful
                        })
                        .ToList();

                    LogsDataGridView.AutoGenerateColumns = false;
                    LogsDataGridView.DataSource = logGridData;
                }
                else
                {
                    LogsDataGridView.DataSource = null;
                    MessageBox.Show("No logs available to display.", "Logs",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                var cell = grid.Rows[e.RowIndex].Cells[e.ColumnIndex];

                cell.Value = "N/A";

            }
        }
        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void InvoicesDataGridView_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }
    }
}
