using Microsoft.Extensions.DependencyInjection;
using POSPRA.Application.Services.FiscalService;
using POSPRA.Application.Services.LogService;
using POSPRA.Domain.Entities;
using POSPRA_WinFormsUI.AlertClasses;
using System.Net.NetworkInformation;
using System.ServiceProcess;
using AlertType = POSPRA.Application.Utility.AlertType;

namespace POSPRA_WinFormsUI.Forms
{
    public partial class Main : Form
    {
        private readonly IServiceProvider _provider;
        private readonly IFiscalService _fiscalService;
        //logs
        private readonly ILogService _logService;
        // -----------------------------
        // Checkers cancellation tokens
        // -----------------------------
        private CancellationTokenSource _internetCheckCts;
        private CancellationTokenSource _workerServiceCts;

        // Track non-MDI forms separately
        private readonly List<Form> _independentForms = new List<Form>();

        public Main(IServiceProvider provider, IFiscalService fiscalService, ILogService logService)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
            _fiscalService = fiscalService ?? throw new ArgumentNullException(nameof(fiscalService));

            InitializeComponent();
            this.IsMdiContainer = true;

            panInvoiceSelection.Visible = false;
            panExportInvoice.Visible = false;
            panCatalogView.Visible = false;


            btnDashboard.ForeColor = ColorTranslator.FromHtml("#686DF4"); // Highlight color

            // Load Dashboard as default child
            Form childForm = _provider.GetRequiredService<DashboardForm>();
            childForm.MdiParent = this;
            childForm.Dock = DockStyle.Fill;
            childForm.Show();

            // Handle form state changes to manage independent forms
            this.Resize += Main_Resize;

            // Start internet and worker service checkers
            StartInternetStatusChecker();
            StartWorkerServiceStatusChecker();

            // Placeholder for worker service
            lblWorkerService.Text = "Worker Service: -";
            lblWorkerService.Font = new Font(lblWorkerService.Font, FontStyle.Italic);
            lblWorkerService.ForeColor = Color.Gray;

            _logService = logService;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);

            StopInternetStatusChecker();
            StopWorkerServiceStatusChecker();

            foreach (var form in _independentForms.ToArray())
            {
                if (form != null && !form.IsDisposed)
                {
                    form.Close();
                }
            }
        }

        private void Main_Resize(object sender, EventArgs e) => HandleFormStateChange();
        private void Main_WindowStateChanged(object sender, EventArgs e) => HandleFormStateChange();

        private void HandleFormStateChange()
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                foreach (var form in _independentForms)
                {
                    if (form != null && !form.IsDisposed && form.Visible)
                    {
                        form.Hide();
                        form.Tag = "was_visible";
                    }
                }
            }
            else if (this.WindowState == FormWindowState.Normal || this.WindowState == FormWindowState.Maximized)
            {
                foreach (var form in _independentForms)
                {
                    if (form != null && !form.IsDisposed && form.Tag?.ToString() == "was_visible")
                    {
                        form.Show();
                        form.BringToFront();
                        form.Tag = null;
                    }
                }
            }
        }

        private void OpenIndependentForm<T>() where T : Form
        {
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

            var newForm = _provider.GetRequiredService<T>();
            newForm.MdiParent = null;
            newForm.ShowInTaskbar = true;
            newForm.StartPosition = FormStartPosition.CenterScreen;
            newForm.FormBorderStyle = FormBorderStyle.Sizable;
            newForm.FormClosed += (s, e) => _independentForms.Remove(newForm);

            _independentForms.Add(newForm);
            newForm.Show();
            newForm.BringToFront();
        }

        // -----------------------------
        // INTERNET STATUS CHECKER
        // -----------------------------

        private DateTime? offlineSince = null;
        private bool? wasOnline = null;
        private DateTime lastOfflineAlertTime = DateTime.MinValue;
        private void StartInternetStatusChecker()
        {
            _internetCheckCts = new CancellationTokenSource();
            CancellationToken ct = _internetCheckCts.Token;

            _ = Task.Run(async () =>
            {
                //bool wasOnline = true;
                //bool wasOnline = false;
                //DateTime? offlineSince = null;


                while (!ct.IsCancellationRequested)
                {
                    try
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

                        // Only trigger alert if offline continuously for 2 seconds
                        //if (!online && wasOnline)
                        //{
                        //    // Wait 2 seconds and check again
                        //    await Task.Delay(3000, ct);
                        //    bool stillOffline = !await CheckInternetConnectivityAsync();

                        //    if (stillOffline)
                        //        ShowAlert("Internet connection lost!");

                        //    if (!stillOffline)
                        //        ShowAlert("Internet connection restore!");
                        //}

                        // State changed
                        if (wasOnline != null && wasOnline != online)
                        {
                            if (!online)
                            {
                                // Connection just went offline
                                offlineSince = DateTime.Now;

                                ShowAlert("Internet connection lost!", "Error", true);
                                _ = CreateLog("Internet connection lost", AlertType.Error);
                                lastOfflineAlertTime = DateTime.Now; // reset timer
                            }
                            else
                            {
                                // Connection just restored
                                string downtimeMsg = "";
                                if (offlineSince.HasValue)
                                {
                                    TimeSpan downTime = DateTime.Now - offlineSince.Value;
                                    downtimeMsg = $" (Downtime: {downTime.TotalSeconds:F0} seconds)";
                                }

                                ShowAlert("Internet connection restored!", "Success", true);
                                _ = CreateLog("Internet connection restored" + downtimeMsg, AlertType.Success);

                                offlineSince = null;
                            }
                        }
                        else if (!online) // still offline
                        {
                            // Show repeated alerts every 3 seconds (no log)
                            if ((DateTime.Now - lastOfflineAlertTime).TotalSeconds >= 3)
                            {
                                string msg = "Internet connection still offline";
                                if (offlineSince.HasValue)
                                {
                                    TimeSpan downTime = DateTime.Now - offlineSince.Value;
                                    msg += $" ({downTime.TotalSeconds:F0} seconds)";
                                }

                                ShowAlert("Internet connection lost!", "Error", false);
                                lastOfflineAlertTime = DateTime.Now;
                            }
                        }


                        wasOnline = online;

                        await Task.Delay(2000, ct); // normal polling interval
                    }
                    catch (TaskCanceledException)
                    {
                        break;
                    }
                }
            }, ct);
        }



        private async Task<bool> CheckInternetConnectivityAsync()
        {
            try
            {
                using var ping = new Ping();
                var reply = await ping.SendPingAsync("8.8.8.8", 2000);
                return reply.Status == IPStatus.Success;
            }
            catch { return false; }
        }

        private void StopInternetStatusChecker()
        {
            _internetCheckCts?.Cancel();
            _internetCheckCts?.Dispose();
        }

        // -----------------------------
        // WORKER SERVICE STATUS CHECKER
        // -----------------------------
        private bool _workerServiceAlertShown = false;

        private void StartWorkerServiceStatusChecker()
        {
            _workerServiceCts = new CancellationTokenSource();
            CancellationToken ct = _workerServiceCts.Token;

            _ = Task.Run(async () =>
            {
                bool wasRunning = true;

                while (!ct.IsCancellationRequested)
                {
                    try
                    {
                        bool isRunning = await IsWorkerServiceRunningAsync();

                        if (lblWorkerService != null && lblWorkerService.IsHandleCreated)
                        {
                            lblWorkerService.BeginInvoke(new Action(() =>
                            {
                                lblWorkerService.Text = $"Worker Service: {(isRunning ? "Active" : "Inactive")}";
                                lblWorkerService.Font = new Font(lblWorkerService.Font, FontStyle.Bold);
                                lblWorkerService.ForeColor = isRunning ? Color.Green : Color.Red;
                            }));
                        }

                        // Service just went offline
                        if (!isRunning && wasRunning)
                        {
                            WindowsLocalAppNotification.Show("Worker Service Alert", "Worker service is inactive!");
                            _workerServiceAlertShown = true;
                        }
                        // Service came back online
                        else if (isRunning && !wasRunning)
                        {
                            WindowsLocalAppNotification.Show("Worker Service Alert", "Worker service restored!");
                            _workerServiceAlertShown = false;
                        }

                        wasRunning = isRunning;
                        await Task.Delay(5000, ct);
                    }
                    catch (TaskCanceledException) { break; }
                }
            }, ct);
        }


        private Task<bool> IsWorkerServiceRunningAsync()
        {
            return Task.Run(() =>
            {
                try
                {
                    using var sc = new ServiceController("POSPRAWorker");
                    return sc.Status == ServiceControllerStatus.Running;
                }
                catch { return false; }
            });
        }

        private void StopWorkerServiceStatusChecker()
        {
            _workerServiceCts?.Cancel();
            _workerServiceCts?.Dispose();
        }


        //LOGS METHOD
        private async Task CreateLog(string message, string type)
        {
            var log = new Logs
            {
                Message = message,   // pass any message
                Type = type,         // comes from AlertType constants

            };

            await _logService.LogAsync(log);
        }

        // -----------------------------
        // ALERT METHOD
        // -----------------------------
        private void ShowAlert(string message, string alertType, bool isShowWindowsNotification, string title = "Alert")
        {
            if (!this.IsHandleCreated) return;

            this.BeginInvoke(new Action(() =>
            {
                try
                {
                    // Option 1: Windows toast notification (non-blocking)
                    if (isShowWindowsNotification)
                    {
                        WindowsLocalAppNotification.Show(title, message);
                    }

                    // Option 2: Custom alert manager (non-blocking)
                    if (alertType == nameof(AlertType.Error))
                        AlertManager.ShowError(message);

                    // Option 2: Custom alert manager (non-blocking)
                    if (alertType == nameof(AlertType.Success))
                        AlertManager.ShowSuccess(message);

                    // Option 3: Fallback MessageBox (blocking, optional)
                    // MessageBox.Show(this, message, title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                catch
                {
                    // Fallback in case notifications fail
                    MessageBox.Show(this, message, title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }));
        }


        // -----------------------------
        // NAVIGATION AND VIEWS
        // -----------------------------
        private void ResetNavStyles()
        {
            btnDashboard.ForeColor = Color.Black;
            btnInvoiceSelection.ForeColor = Color.Black;
            btnExportInvoice.ForeColor = Color.Black;
            btnCatalogView.ForeColor = Color.Black;

            panDashboard.Visible = false;
            panInvoiceSelection.Visible = false;
            panExportInvoice.Visible = false;
            panCatalogView.Visible = false;
        }

        private void btnDashboard_Click(object sender, EventArgs e)
        {
            ResetNavStyles();
            btnDashboard.ForeColor = ColorTranslator.FromHtml("#686DF4");
            panDashboard.Visible = true;
            LoadView("Dashboard");
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
            LoadView("Export Invoice");
        }

        private void btnCatalogView_Click(object sender, EventArgs e)
        {
            ResetNavStyles();
            btnCatalogView.ForeColor = ColorTranslator.FromHtml("#686DF4");
            panCatalogView.Visible = true;
            LoadView("Catalog View");
        }


        public void LoadView(string v)
        {
            // Properly close and dispose the active MDI child
            if (this.ActiveMdiChild != null)
            {
                var activeChild = this.ActiveMdiChild;
                activeChild.Close();
                activeChild.Dispose();
            }

            // Dispose all MDI children to ensure clean state
            foreach (Form child in this.MdiChildren)
            {
                child.Close();
                child.Dispose();
            }

            Form childForm = v switch
            {
                "Dashboard" => _provider.GetRequiredService<DashboardForm>(),
                "Invoice Entry" => _provider.GetRequiredService<item_entry>(),
                "Export Invoice" => _provider.GetRequiredService<ExportInvoiceForm>(),
                "Catalog View" => _provider.GetRequiredService<CatalogView>(),
                _ => throw new NotImplementedException()
            };

            if (childForm == null) return;

            childForm.TopLevel = false;
            childForm.FormBorderStyle = FormBorderStyle.None;
            childForm.MdiParent = this;
            childForm.Dock = DockStyle.Fill;
            childForm.WindowState = FormWindowState.Maximized;
            childForm.Show();
        }
    }
}