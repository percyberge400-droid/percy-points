using System.Net.NetworkInformation;
using System.ServiceProcess;
using Microsoft.Extensions.DependencyInjection;
using POSPRA.Application.Services.LogService;
using POSPRA.Domain.Entities;
using POSPRA_WinFormsUI.AlertClasses;
using AlertType = POSPRA.Application.Utility.AlertType;

namespace POSPRA_WinFormsUI.Forms
{
    public partial class Main : Form
    {
        private readonly IServiceProvider _provider;
        private readonly IFiscalService _fiscalService;
        private readonly ILogService _logService;
        private CancellationTokenSource _internetCheckCts;
        private CancellationTokenSource _workerServiceCts;
        private readonly List<Form> _independentForms = new List<Form>();
        private DateTime? offlineSince = null;
        private bool? wasOnline = null;
        private DateTime lastOfflineAlertTime = DateTime.MinValue;
        private bool _workerServiceAlertShown = false;

        public Main(IServiceProvider provider, ILogService logService)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
            _fiscalService = fiscalService ?? throw new ArgumentNullException(nameof(fiscalService));
            _logService = logService ?? throw new ArgumentNullException(nameof(logService));

            InitializeComponent();
            this.IsMdiContainer = true;

            panInvoiceSelection.Visible = false;
            panExportInvoice.Visible = false;
            panCatalogView.Visible = false;

            btnDashboard.ForeColor = ColorTranslator.FromHtml("#48A787");

            Form childForm = _provider.GetRequiredService<DashboardForm>();
            childForm.MdiParent = this;
            childForm.Dock = DockStyle.Fill;
            childForm.Show();

            this.Resize += Main_Resize;
            StartInternetStatusChecker();
            StartWorkerServiceStatusChecker();

            posStatus.Font = new Font(posStatus.Font, FontStyle.Italic);
            posStatus.ForeColor = Color.Gray;

            internetStatus.Font = new Font(internetStatus.Font, FontStyle.Italic);
            internetStatus.ForeColor = Color.Gray;
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

        private void StartInternetStatusChecker()
        {
            _internetCheckCts = new CancellationTokenSource();
            CancellationToken ct = _internetCheckCts.Token;

            _ = Task.Run(async () =>
            {
                while (!ct.IsCancellationRequested)
                {
                    try
                    {
                        bool online = await CheckInternetConnectivityAsync();

                        if (internetStatus != null && internetStatus.IsHandleCreated)
                        {
                            internetStatus.BeginInvoke(new Action(() =>
                            {
                                // Update main label
                                internetStatus.Text = $"{(online ? "● Online" : "● Offline")}";
                                internetStatus.Font = new Font(internetStatus.Font, FontStyle.Bold);
                                internetStatus.ForeColor = online ? Color.Green : Color.Red;
                            }));
                        }

                        if (wasOnline != null && wasOnline != online)
                        {
                            if (!online)
                            {
                                offlineSince = DateTime.Now;
                                ShowAlert("Internet connection lost!", "Error", true);
                                _ = CreateLog("Internet connection lost", AlertType.Error);
                                lastOfflineAlertTime = DateTime.Now;
                            }
                            else
                            {
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
                        else if (!online)
                        {
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
                        await Task.Delay(2000, ct);
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

                        if (posStatus != null && posStatus.IsHandleCreated)
                        {
                            posStatus.BeginInvoke(new Action(() =>
                            {
                                // Update main label
                                posStatus.Text = $"{(isRunning ? "● Active" : "● Inactive")}";
                                posStatus.Font = new Font(posStatus.Font, FontStyle.Bold);
                                posStatus.ForeColor = isRunning ? Color.Green : Color.Red;
                            }));
                        }

                        if (!isRunning && wasRunning)
                        {
                            WindowsLocalAppNotification.Show("POS Service Alert", "POS service is inactive!");
                            _workerServiceAlertShown = true;
                        }
                        else if (isRunning && !wasRunning)
                        {
                            WindowsLocalAppNotification.Show("POS Service Alert", "POS service restored!");
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

        private async Task CreateLog(string message, string type)
        {
            var log = new Logs { Message = message, Type = type };
            await _logService.LogAsync(log);
        }

        private void ShowAlert(string message, string alertType, bool isShowWindowsNotification, string title = "Alert")
        {
            if (!this.IsHandleCreated) return;

            this.BeginInvoke(new Action(() =>
            {
                try
                {
                    if (isShowWindowsNotification)
                        WindowsLocalAppNotification.Show(title, message);

                    if (alertType == nameof(AlertType.Error))
                        AlertManager.ShowError(message);
                    else if (alertType == nameof(AlertType.Success))
                        AlertManager.ShowSuccess(message);
                }
                catch
                {
                    MessageBox.Show(this, message, title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }));
        }

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
            btnDashboard.ForeColor = ColorTranslator.FromHtml("#48A787");
            panDashboard.Visible = true;
            LoadView("Dashboard");
        }

        private void btnInvoiceSelection_Click(object sender, EventArgs e)
        {
            ResetNavStyles();
            btnInvoiceSelection.ForeColor = ColorTranslator.FromHtml("#48A787");
            panInvoiceSelection.Visible = true;
            LoadView("Invoice Entry");
        }

        private void btnExportInvoice_Click(object sender, EventArgs e)
        {
            ResetNavStyles();
            btnExportInvoice.ForeColor = ColorTranslator.FromHtml("#48A787");
            panExportInvoice.Visible = true;
            LoadView("Export Invoice");
        }

        private void btnCatalogView_Click(object sender, EventArgs e)
        {
            ResetNavStyles();
            btnCatalogView.ForeColor = ColorTranslator.FromHtml("#48A787");
            panCatalogView.Visible = true;
            LoadView("Catalog View");
        }

        public void LoadView(string v)
        {
            if (this.ActiveMdiChild != null)
            {
                var activeChild = this.ActiveMdiChild;
                activeChild.Close();
                activeChild.Dispose();
            }
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

