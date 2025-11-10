using Microsoft.Extensions.DependencyInjection;
using POSPRA.SecurityEncryption;
using POSPRA_WinFormsUI.AlertClasses;
using System.Configuration;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.ServiceProcess;
using System.Text.RegularExpressions;

namespace POSPRA_WinFormsUI.Forms
{
    public partial class LoginForm2 : Form
    {
        private readonly IServiceProvider _provider;
        private const string SERVICE_NAME = "POSPRAWorker";
        public LoginForm2(IServiceProvider provider)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
            InitializeComponent();

            // Transparent background
            this.FormBorderStyle = FormBorderStyle.None;
            btnClose.Click += btnClose_Click;

            this.AcceptButton = btnLogin;

            // Rounded form edges
            MakeRoundedControl(this, 30);

            // Rounded buttons with no focus border
            MakeRoundedControl(btnLogin, 25);
            btnLogin.FlatAppearance.BorderSize = 0;
            btnLogin.FlatAppearance.BorderColor = btnLogin.BackColor;
            btnLogin.NotifyDefault(false);
            btnLogin.TabStop = true;

            this.Load += LoginForm2_Load;
            // Create rounded transparent username box
            //MakeStyledTextBox(txtUsername);
            //MakeStyledTextBox(txtPassword);

            // ✅ Load logo dynamically from App.config
            string logoKey = ConfigurationManager.AppSettings["LOGO"];
            if (!string.IsNullOrEmpty(logoKey))
            {
                var res = Resources.ResourceManager.GetObject(logoKey);
                if (res is Image img)
                {
                    picLogo.Image = img;
                    //picLogo.SizeMode = PictureBoxSizeMode.Zoom;
                }
            }
            string posCOMP = ConfigurationManager.AppSettings["pos"];
            if (!string.IsNullOrEmpty(posCOMP))
            {
                var res = Resources.ResourceManager.GetObject(posCOMP);
                if (res is Image img)
                {
                    pictureBox2.Image = img;
                    //picLogo.SizeMode = PictureBoxSizeMode.Zoom;
                }
            }

            // ✅ Make labels dynamic from App.config
            string inquiryNo = ConfigurationManager.AppSettings["generalinquiryNo"];
            string servicesNo = ConfigurationManager.AppSettings["eServicesNo"];
            string website = ConfigurationManager.AppSettings["website"];

            txtUsername.TextChanged += NumericOnlyWithLength_TextChanged;
            txtUsername.KeyPress += NumericOnlyWithLength_KeyPress;


            if (!string.IsNullOrEmpty(inquiryNo))
                generalinquiryNo.Text = inquiryNo;

            if (!string.IsNullOrEmpty(servicesNo))
                eServicesNo.Text = servicesNo;
            if (!string.IsNullOrEmpty(website))
                prawebsite.Text = website;
        }

        private async void LoginForm2_Load(object sender, EventArgs e)
        {
            try
            {
                // Run your updater check
                await CheckAndPromptUpdatesAsync();

                // Existing code: restart your worker service
                await RestartServiceAlwaysAsync(SERVICE_NAME);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Startup routine failed: {ex.Message}",
                    "Startup Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }


        /// <summary>
        /// Restarts the given service whether it is running or stopped.
        /// </summary>
        private async Task RestartServiceAlwaysAsync(string serviceName)
        {
            await Task.Run(() =>
            {
                using (var service = new ServiceController(serviceName))
                {
                    try
                    {
                        // If service exists
                        var status = service.Status;

                        // Try stopping if running
                        if (status == ServiceControllerStatus.Running ||
                            status == ServiceControllerStatus.StartPending)
                        {
                            service.Stop();
                            service.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(30));
                        }

                        // Start regardless of previous state
                        service.Start();
                        service.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(30));
                    }
                    catch (InvalidOperationException ex)
                    {
                        throw new InvalidOperationException($"Service '{serviceName}' may not exist or is inaccessible.\n{ex.Message}");
                    }
                    catch (System.ServiceProcess.TimeoutException)
                    {
                        throw new System.TimeoutException($"Timeout while restarting '{serviceName}'.");
                    }
                }
            });
        }

        #region validations

        private void NumericOnlyWithLength_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (sender is not TextBox tb) return;

            switch (tb.Name.ToLower())
            {
                case "txtusername":
                    // Allow only digits (0–9)
                    if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
                    {
                        e.Handled = true;
                    }
                    // Limit to 6 digits
                    else if (!char.IsControl(e.KeyChar) && tb.Text.Length >= 6)
                    {
                        e.Handled = true;
                    }
                    break;
            }
        }

        private void NumericOnlyWithLength_TextChanged(object sender, EventArgs e)
        {
            if (sender is not TextBox tb) return;

            string original = tb.Text;
            string clean = original;

            switch (tb.Name.ToLower())
            {
                case "txtusername":
                    // Remove everything except digits
                    clean = Regex.Replace(original, @"[^0-9]", "");
                    // Limit to 6 digits
                    if (clean.Length > 6)
                        clean = clean.Substring(0, 6);
                    break;
            }

            // Update text if needed (fix pasted invalid text)
            if (tb.Text != clean)
            {
                int cursorPos = tb.SelectionStart - (tb.Text.Length - clean.Length);
                tb.Text = clean;
                tb.SelectionStart = Math.Max(0, Math.Min(cursorPos, tb.Text.Length));
            }
        }
        private void HighlightInvalidTextBox(TextBox tb)
        {
            tb.BackColor = Color.MistyRose;

            tb.Paint += (s, e) =>
            {
                ControlPaint.DrawBorder(e.Graphics, tb.ClientRectangle,
                    Color.Red, 2, ButtonBorderStyle.Solid,
                    Color.Red, 2, ButtonBorderStyle.Solid,
                    Color.Red, 2, ButtonBorderStyle.Solid,
                    Color.Red, 2, ButtonBorderStyle.Solid);
            };

            tb.Invalidate();
        }

        private bool HighlightEmptyTextBoxes(params TextBox[] textBoxes)
        {
            bool hasEmpty = false;

            foreach (var tb in textBoxes)
            {
                if (string.IsNullOrWhiteSpace(tb.Text))
                {
                    hasEmpty = true;
                    tb.BackColor = Color.MistyRose;

                    tb.Paint += (s, e) =>
                    {
                        ControlPaint.DrawBorder(e.Graphics, tb.ClientRectangle,
                            Color.Red, 2, ButtonBorderStyle.Solid,
                            Color.Red, 2, ButtonBorderStyle.Solid,
                            Color.Red, 2, ButtonBorderStyle.Solid,
                            Color.Red, 2, ButtonBorderStyle.Solid);
                    };
                }
                else
                {
                    tb.BackColor = Color.White;
                }

                tb.Invalidate();
            }

            return hasEmpty;
        }
        private void ResetTextBoxHighlights(params TextBox[] textBoxes)
        {
            foreach (var tb in textBoxes)
            {
                tb.BackColor = Color.White;
            }
        }

        private bool ValidateLoginFields()
        {
            // Check for empty POS ID or Access Code
            if (string.IsNullOrWhiteSpace(txtUsername.Text) || string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                if (string.IsNullOrWhiteSpace(txtUsername.Text) && string.IsNullOrWhiteSpace(txtPassword.Text))
                {
                    AlertManager.ShowError("POS ID and Access Code cannot be empty.");
                    this.BeginInvoke(new Action(() => txtUsername.Focus()));
                }
                else if (string.IsNullOrWhiteSpace(txtUsername.Text))
                {
                    AlertManager.ShowError("POS ID is required.");
                    this.BeginInvoke(new Action(() => txtUsername.Focus()));
                }
                else if (string.IsNullOrWhiteSpace(txtPassword.Text))
                {
                    AlertManager.ShowError("Access Code is required.");
                    this.BeginInvoke(new Action(() => txtPassword.Focus()));
                }

                HighlightEmptyTextBoxes(txtUsername, txtPassword);
                return false;
            }

            // Validate POS ID: must be numeric and exactly 6 digits
            if (!txtUsername.Text.All(char.IsDigit) || txtUsername.Text.Length != 6)
            {
                AlertManager.ShowError("POS ID must be exactly 6 digits.");
                this.BeginInvoke(new Action(() => txtUsername.Focus()));

                HighlightInvalidTextBox(txtUsername);
                ResetTextBoxHighlights(txtPassword);
                return false;
            }

            ResetTextBoxHighlights(txtUsername, txtPassword);
            return true;
        }

        #endregion

        private void btnLogin_Click(object sender, EventArgs e)
        {
            try
            {
                if (!ValidateLoginFields())
                    return;

                // Get AES-encrypted values from config
                string encUsername = ConfigurationManager.AppSettings["Username"];
                string encPassword = ConfigurationManager.AppSettings["Password"];

                // Decrypt values before using
                string configUsername = AesEncryptionHelper.Decrypt(encUsername);
                string configPassword = AesEncryptionHelper.Decrypt(encPassword);

                string enteredUsername = txtUsername.Text.Trim();
                string enteredPassword = txtPassword.Text.Trim();

                if (enteredUsername == configUsername && enteredPassword == configPassword)
                {
                    // Open DashboardForm
                    var dashboard = _provider.GetRequiredService<Main>();
                    dashboard.Show();

                    // Hide login form
                    this.Hide();
                }
                else
                {
                    MessageBox.Show("Invalid credentials!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Login failed: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close(); // Closes the form
        }
        private void btnClose_MouseEnter(object sender, EventArgs e)
        {
            btnClose.ForeColor = Color.White;
            btnClose.BackColor = Color.Red;
        }

        private void btnClose_MouseLeave(object sender, EventArgs e)
        {
            btnClose.ForeColor = Color.Gray;
            btnClose.BackColor = Color.Transparent;
        }

        private void loginBox_Paint(object sender, PaintEventArgs e)
        {

        }

        public void MakeRoundedControl(Control control, int radius)
        {
            if (control == null || control.Width <= 0 || control.Height <= 0)
                return;

            using (GraphicsPath path = new GraphicsPath())
            {
                path.StartFigure();
                path.AddArc(0, 0, radius, radius, 180, 90);                             // Top-left
                path.AddArc(control.Width - radius, 0, radius, radius, 270, 90);        // Top-right
                path.AddArc(control.Width - radius, control.Height - radius, radius, radius, 0, 90); // Bottom-right
                path.AddArc(0, control.Height - radius, radius, radius, 90, 90);        // Bottom-left
                path.CloseFigure();

                control.Region = new Region(path);
            }
        }

        private void loginBox_Resize(object sender, EventArgs e)
        {
            MakeRoundedControl(loginBox, 35);
        }

        public void MakeStyledTextBox(TextBox textBox)
        {
            if (textBox == null || textBox.Parent == null) return;

            int radius = 20;

            // Store original properties
            var originalLocation = textBox.Location;
            var originalSize = textBox.Size;
            var originalParent = textBox.Parent;

            // Create wrapper panel
            Panel wrapper = new Panel
            {
                Location = originalLocation,
                Size = new Size(originalSize.Width, 38),
                BackColor = Color.White
            };

            // Style the textbox
            textBox.BorderStyle = BorderStyle.None;
            textBox.BackColor = Color.White; // Solid white for textbox itself
            textBox.ForeColor = Color.FromArgb(60, 60, 60);
            textBox.Font = new Font("Segoe UI", 11F, FontStyle.Regular);

            // Position textbox inside wrapper with left padding (~4mm = 15px)
            textBox.Location = new Point(15, 9);
            textBox.Width = wrapper.Width - 30; // 15px padding on each side
            textBox.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;

            // Remove textbox from original parent
            originalParent.Controls.Remove(textBox);

            // Add textbox to wrapper
            wrapper.Controls.Add(textBox);

            // Add wrapper to original parent
            originalParent.Controls.Add(wrapper);

            // Apply rounded region to wrapper
            ApplyRoundedRegion(wrapper, radius);

            // Custom paint for wrapper (semi-transparent background + white border)
            wrapper.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

                Rectangle rect = new Rectangle(0, 0, wrapper.Width, wrapper.Height);

                // Draw semi-transparent white background (more opaque)
                using (SolidBrush bgBrush = new SolidBrush(Color.FromArgb(220, 255, 255, 255)))
                {
                    using (GraphicsPath bgPath = CreateRoundedRectPath(rect, radius))
                    {
                        e.Graphics.FillPath(bgBrush, bgPath);
                    }
                }

                // Draw white border (thicker and more visible)
                using (Pen pen = new Pen(Color.FromArgb(255, 255, 255), 3))
                {
                    Rectangle borderRect = new Rectangle(2, 2, wrapper.Width - 4, wrapper.Height - 4);
                    using (GraphicsPath borderPath = CreateRoundedRectPath(borderRect, radius - 1))
                    {
                        e.Graphics.DrawPath(pen, borderPath);
                    }
                }
            };

            // Handle wrapper resize
            wrapper.Resize += (s, e) =>
            {
                ApplyRoundedRegion(wrapper, radius);
                textBox.Width = wrapper.Width - 30;
                wrapper.Invalidate();
            };
        }

        private void ApplyRoundedRegion(Control control, int radius)
        {
            if (control.Width == 0 || control.Height == 0) return;

            using (GraphicsPath path = CreateRoundedRectPath(new Rectangle(0, 0, control.Width, control.Height), radius))
            {
                control.Region = new Region(path);
            }
        }

        private GraphicsPath CreateRoundedRectPath(Rectangle rect, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            path.StartFigure();
            path.AddArc(rect.X, rect.Y, radius, radius, 180, 90);
            path.AddArc(rect.Right - radius, rect.Y, radius, radius, 270, 90);
            path.AddArc(rect.Right - radius, rect.Bottom - radius, radius, radius, 0, 90);
            path.AddArc(rect.X, rect.Bottom - radius, radius, radius, 90, 90);
            path.CloseFigure();
            return path;
        }
        private async Task CheckAndPromptUpdatesAsync()
        {
            try
            {
                // STEP 1️⃣: Detect install path
                string installPath = GetInstallPath();
                if (!installPath.EndsWith("\\")) installPath += "\\";

                // STEP 2️⃣: Load server path
                string serverPath = GetServerPath(installPath);
                if (!serverPath.EndsWith("\\")) serverPath += "\\";

                // STEP 3️⃣: Launcher update
                await CheckLauncherUpdateAsync(installPath, serverPath);

                // STEP 4️⃣: App update
                await CheckAppUpdateAsync(installPath, serverPath);

                // NOTE: LoginForm stays open throughout — never closes
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Update check failed:\n\n{ex.Message}",
                    "Update Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string GetInstallPath()
        {
            string commonInfo = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                "PRAL", "install_info.txt");

            string installPath = "";

            if (File.Exists(commonInfo))
            {
                foreach (var line in File.ReadAllLines(commonInfo))
                {
                    if (line.StartsWith("InstallPath=", StringComparison.OrdinalIgnoreCase))
                    {
                        installPath = line.Substring("InstallPath=".Length).Trim();
                        break;
                    }
                }
            }

            if (string.IsNullOrWhiteSpace(installPath) || !Directory.Exists(installPath))
            {
                var defaultPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                    "PRAL", "POSComponent");
                installPath = Directory.Exists(defaultPath) ? defaultPath : AppDomain.CurrentDomain.BaseDirectory;
            }

            return installPath;
        }

        private string GetServerPath(string installPath)
        {
            string configPath = Path.Combine(installPath, "Updater-Version.config");
            if (!File.Exists(configPath))
            {
                MessageBox.Show($"Missing Updater-Version.config at:\n{configPath}", "Config Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw new FileNotFoundException("Updater-Version.config not found.");
            }

            var xml = System.Xml.Linq.XDocument.Load(configPath);
            var serverPathElement = xml.Descendants("add")
                .FirstOrDefault(x => (string)x.Attribute("key") == "ServerPath");

            if (serverPathElement == null)
            {
                MessageBox.Show("ServerPath not found in Updater-Version.config.",
                    "Invalid Config", MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw new Exception("ServerPath missing in Updater-Version.config.");
            }

            return (string)serverPathElement.Attribute("value") ?? "";
        }

        private async Task CheckLauncherUpdateAsync(string installPath, string serverPath)
        {
            string logFile = Path.Combine(installPath, "launcher_update_log.txt");

            void Log(string msg)
            {
                try
                {
                    File.AppendAllText(logFile, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {msg}\r\n");
                }
                catch { /* ignore logging errors */ }
            }

            try
            {
                Log("---- Checking launcher update ----");

                string localLauncherVersionFile = Path.Combine(installPath, "launcher-version.txt");
                string serverLauncherVersionFile = Path.Combine(serverPath, "Updater", "launcher-version.txt");

                string localLauncherVersion = File.Exists(localLauncherVersionFile)
                    ? File.ReadAllText(localLauncherVersionFile).Trim()
                    : "0.0.0";

                if (!File.Exists(serverLauncherVersionFile))
                {
                    Log("Server launcher-version.txt not found. Skipping update.");
                    return;
                }

                string serverLauncherVersion = File.ReadAllText(serverLauncherVersionFile).Trim();
                Log($"Local version: {localLauncherVersion}, Server version: {serverLauncherVersion}");

                if (localLauncherVersion != serverLauncherVersion)
                {
                    string launcherExe = Path.Combine(installPath, "POSPRA.Launcher.exe");

                    if (!File.Exists(launcherExe))
                    {
                        Log("Launcher executable missing. Aborting update.");
                        return;
                    }

                    Log("New launcher version detected. Launching silent update...");

                    using (var launcherEvent = new EventWaitHandle(false, EventResetMode.AutoReset, "POSPRA_LauncherDone"))
                    {
                        try
                        {
                            Process.Start(new ProcessStartInfo
                            {
                                FileName = launcherExe,
                                UseShellExecute = true,
                                Verb = "runas",
                                WindowStyle = ProcessWindowStyle.Hidden,
                                CreateNoWindow = true
                            });

                            bool completed = await WaitForEventAsync(launcherEvent, TimeSpan.FromSeconds(10));

                            Log(completed
                                ? "Launcher update completed successfully."
                                : "Launcher update timed out after 10 seconds.");
                        }
                        catch (Exception ex)
                        {
                            Log($"Failed to launch or wait for launcher update: {ex.Message}");
                        }
                    }
                }
                else
                {
                    Log("Launcher already up-to-date.");
                }

                Log("---- Launcher update check completed ----");
            }
            catch (Exception ex)
            {
                Log($"Unhandled error during launcher update check: {ex.Message}");
            }
        }


        /// <summary>
        /// Asynchronously waits for a WaitHandle to be signaled.
        /// </summary>
        private Task<bool> WaitForEventAsync(WaitHandle handle, TimeSpan timeout)
        {
            var tcs = new TaskCompletionSource<bool>();

            var registration = ThreadPool.RegisterWaitForSingleObject(
                handle,
                (state, timedOut) => tcs.TrySetResult(!timedOut),
                null,
                timeout,
                executeOnlyOnce: true);

            return tcs.Task.ContinueWith(t =>
            {
                registration.Unregister(null);
                return t.Result;
            });
        }


        private async Task CheckAppUpdateAsync(string installPath, string serverPath)
        {
            string localAppVersionFile = Path.Combine(installPath, "app-version.txt");
            string serverAppVersionFile = Path.Combine(serverPath, "app-version.txt");

            string localAppVersion = File.Exists(localAppVersionFile)
                ? File.ReadAllText(localAppVersionFile).Trim()
                : "0.0.0";

            if (!File.Exists(serverAppVersionFile))
            {
                MessageBox.Show("Server app version file not found.", "Update",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string serverAppVersion = File.ReadAllText(serverAppVersionFile).Trim();

            if (localAppVersion != serverAppVersion)
            {
                var result = MessageBox.Show(
                    $"A new application update is available.\n\n" +
                    $"Your version: {localAppVersion}\n" +
                    $"Latest version: {serverAppVersion}\n\n" +
                    $"Update now?",
                    "Application Update",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    string updaterExe = Path.Combine(installPath, "POSPRA.Updater.exe");
                    if (!File.Exists(updaterExe))
                    {
                        MessageBox.Show("POSPRA.Updater.exe not found.",
                            "Update Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    var psi = new ProcessStartInfo
                    {
                        FileName = updaterExe,
                        UseShellExecute = true,
                        Verb = "runas",
                        WindowStyle = ProcessWindowStyle.Normal
                    };
                    Process.Start(psi);
                }
            }
        }


    }
}