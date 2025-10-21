using LiteDB;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using POSPRA.Infrastructure.Context;
using POSPRA.SecurityEncryption;
using System.Configuration;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml;

namespace POSPRA.SetupUI
{
    public partial class ConfigForm : Form
    {
        // --- Win32 API to force window on top ---
        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(
            IntPtr hWnd,
            IntPtr hWndInsertAfter,
            int X,
            int Y,
            int cx,
            int cy,
            uint uFlags);

        // HWND constants
        private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        private static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);

        // Flags for SetWindowPos
        private const uint SWP_NOMOVE = 0x0002;
        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_SHOWWINDOW = 0x0040;

        // Paths for different config files
        private readonly string _xmlConfigPath;
        private readonly string _jsonWorkerPath;
        private readonly string _jsonMainPath;
        private readonly string _winformsConfigPath;
        private string _setupConfigPath;

        private int _isLoadingFlag = 0;
        private bool _isLoading = false;

        // Timer for auto-hiding messages
        private System.Windows.Forms.Timer _messageHideTimer;

        // ims file
        string defaultIMSPath = ConfigurationManager.AppSettings["DefaulIMStFilePath"];
        string defaultPassword = ConfigurationManager.AppSettings["DbPassword"];
        string backupDir = ConfigurationManager.AppSettings["backupDir"];

        public ConfigForm(string xmlConfigPath, string jsonWorkerPath, string jsonMainPath, string setupConfigPath, string winformsConfigPath)
        {
            InitializeComponent();
            // Always stay above all other windows
            this.TopMost = true;

            // Make sure it stays focused
            this.BringToFront();
            this.Activate();

            // Optional: Prevent user from sending it to back
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            _xmlConfigPath = xmlConfigPath;
            _jsonWorkerPath = jsonWorkerPath;
            _jsonMainPath = jsonMainPath;
            _setupConfigPath = setupConfigPath;
            _winformsConfigPath = winformsConfigPath;

            // Hook validation events
            txtUsername.KeyPress += txtUsername_KeyPress;
            txtPassword.KeyPress += txtPassword_KeyPress;
            txtUsername.TextChanged += ValidateForm;
            txtPassword.TextChanged += ValidateForm;

            this.AcceptButton = btnOk;

            // Hide progress bar initially
            if (progressBar != null) progressBar.Visible = false;

            // Initialize message timer
            _messageHideTimer = new System.Windows.Forms.Timer();
            _messageHideTimer.Interval = 5000; // 5 seconds
            _messageHideTimer.Tick += (s, e) => HideMessage();

            // Load logo from App.config if available
            string logoKey = ConfigurationManager.AppSettings["LOGO"];
            if (!string.IsNullOrEmpty(logoKey))
            {
                var res = Resource.ResourceManager.GetObject(logoKey);
                if (res is Image img)
                {
                    LOGO_img.Image = img;
                    LOGO_img.SizeMode = PictureBoxSizeMode.Zoom;
                    LOGO_img.AutoSize = true;
                }
            }

            ClearAllFields();

            // Load default DB path from App.config
            string defaultPath = ConfigurationManager.AppSettings["DefaultDBFilePath"];
            if (!string.IsNullOrWhiteSpace(defaultPath))
            {
                txtFilePath.Text = defaultPath;
            }

            txtOldDB.Text = defaultIMSPath;

        }

        #region Message Display Helper
        private void ShowMessage(string message, bool isSuccess, bool autoHide = true)
        {
            lblMessage.Text = message;
            lblMessage.Visible = true;
            lblMessage.BringToFront();

            if (isSuccess)
            {
                // Success: Green text on white background
                lblMessage.ForeColor = Color.FromArgb(76, 175, 80);
                lblMessage.BackColor = Color.White;
                lblMessage.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            }
            else
            {
                // Error: Red text on white background
                lblMessage.ForeColor = Color.FromArgb(244, 67, 54);
                lblMessage.BackColor = Color.White;
                lblMessage.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            }

            lblMessage.Padding = new Padding(15, 10, 15, 10);
            lblMessage.TextAlign = ContentAlignment.MiddleCenter;
            lblMessage.AutoSize = false;
            lblMessage.Height = 45;
            lblMessage.Width = 480;

            if (autoHide)
            {
                _messageHideTimer.Stop();
                _messageHideTimer.Start();
            }
        }

        private void HideMessage()
        {
            _messageHideTimer.Stop();
            lblMessage.Visible = false;
            lblMessage.Text = "";
        }
        #endregion

        #region Progress Bar Helper
        private async Task RunSingleLoad(Func<Task> work)
        {
            if (Interlocked.Exchange(ref _isLoadingFlag, 1) == 1) return;

            try
            {
                if (progressBar != null)
                {
                    progressBar.Style = ProgressBarStyle.Marquee;
                    progressBar.MarqueeAnimationSpeed = 30;
                    progressBar.Visible = true;
                    progressBar.BringToFront();
                    progressBar.Update();
                }

                await work();
            }
            finally
            {
                if (progressBar != null)
                {
                    progressBar.Visible = false;
                    progressBar.Style = ProgressBarStyle.Continuous;
                }

                Interlocked.Exchange(ref _isLoadingFlag, 0);
            }
        }
        #endregion

        // --- Ensure form opens on top of everything ---
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            this.CenterToScreen();  // center form
            this.TopMost = true;    // mark as topmost

            // Force Win32 TopMost in case another app steals focus
            SetWindowPos(this.Handle, HWND_TOPMOST, 0, 0, 0, 0,
                         SWP_NOMOVE | SWP_NOSIZE | SWP_SHOWWINDOW);

            this.Activate();        // bring focus
            this.BringToFront();    // make sure visible
        }

        // --- Browse button for selecting DB file path ---
        private void btnBrowse_Click(object sender, EventArgs e)
        {
            using (var dialog = new SaveFileDialog())
            {
                dialog.Title = "Select or create SQLite DB file";
                dialog.Filter = "SQLite DB (*.db)|*.db|All files (*.*)|*.*";
                dialog.FileName = "POSPRA.db";

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    txtFilePath.Text = dialog.FileName;
                }
            }
        }

        private void ClearAllFields()
        {
            txtUsername.Clear();
            txtPassword.Clear();
        }

        private void txtUsername_KeyPress(object sender, KeyPressEventArgs e) { }
        private void txtPassword_KeyPress(object sender, KeyPressEventArgs e) { }

        private void ValidateForm(object sender, EventArgs e)
        {
            btnOk.Enabled = !string.IsNullOrWhiteSpace(txtUsername.Text);
        }

        // --- OK button click handler ---
        private async void btnOk_Click(object sender, EventArgs e)
        {
            try
            {
                if (!ValidateInputs(out string username, out string password, out string dbPath, out string oldDbPath))
                    return;

                if (!CheckOldDatabaseExists(oldDbPath))
                    return;

                Directory.CreateDirectory(Path.GetDirectoryName(dbPath));

                var mac = TryGetMacAddress();
                var json = await AuthenticateAsync(username, password, mac);

                if (json == null)
                    return;

                var (branchName, branchAddress, businessName) = ExtractBranchDetails(json);

                SaveAllConfigs(username, password, mac, dbPath, branchName, branchAddress, businessName);
                if (!VerifyAuthentication(json))
                    return;

                SaveEnvironmentSettings();
                UpdateSetupConfig(dbPath);
                if (!InitializeDatabase(dbPath))
                    return;

                ShowMessage("✅ Configuration saved and authentication successful.", true, false);
                await Task.Delay(2000);
                Environment.Exit(0);
            }
            catch (Exception ex)
            {
                ShowMessage($"❌ Unexpected fatal error: {ex.Message}", false, false);
            }
        }

        private bool ValidateInputs(out string username, out string password, out string dbPath, out string oldDbPath)
        {
            username = txtUsername.Text.Trim();
            password = txtPassword.Text.Trim();
            dbPath = txtFilePath.Text.Trim();
            oldDbPath = txtOldDB.Text.Trim();

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ShowMessage("❌ Please enter both POS ID and Access Code.", false, true);
                return false;
            }

            if (string.IsNullOrWhiteSpace(dbPath))
            {
                ShowMessage("❌ Please select a database file path.", false, true);
                return false;
            }

            if (string.IsNullOrWhiteSpace(oldDbPath))
            {
                ShowMessage("❌ Please select an old database file path.", false, true);
                return false;
            }

            return true;
        }

        private bool CheckOldDatabaseExists(string oldDbPath)
        {
            if (!File.Exists(oldDbPath))
            {
                ShowMessage($"❌ The old database file was not found at:\n{oldDbPath}", false, true);
                txtOldDB.Text = string.Empty;
                return false;
            }
            return true;
        }

        private string TryGetMacAddress()
        {
            try
            {
                return GetMacAddress();
            }
            catch (Exception ex)
            {
                ShowMessage($"⚠️ Failed to read MAC address: {ex.Message}", false, true);
                return string.Empty;
            }
        }

        private async Task<JObject> AuthenticateAsync(string username, string password, string mac)
        {
            try
            {
                var payload = new
                {
                    posId = username,
                    macAddress = mac,
                    token = password
                };

                string apiUrl = ConfigurationManager.AppSettings["ApiUrl"];
                if (string.IsNullOrWhiteSpace(apiUrl))
                {
                    ShowMessage("❌ API URL is missing in App.config.", false, false);
                    return null;
                }

                using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
                client.DefaultRequestHeaders.Add("Accept", "application/json");

                var jsonContent = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");
                var response = await client.PostAsync(apiUrl, jsonContent);
                var responseBody = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    ShowMessage($"❌ Authentication failed. Server returned {(int)response.StatusCode}: {response.ReasonPhrase}", false, false);
                    return null;
                }

                var json = JObject.Parse(responseBody);
                if (json.Type == JTokenType.String)
                    json = JObject.Parse(json.ToString());
                else if (json["response"]?.Type == JTokenType.String)
                    json = JObject.Parse(json["response"].ToString());

                return json;
            }
            catch (Exception ex)
            {
                ShowMessage($"❌ API error: {ex.Message}", false, false);
                return null;
            }
        }

        private (string branchName, string branchAddress, string businessName) ExtractBranchDetails(JObject json)
        {
            try
            {
                var data = json["data"];
                if (data != null && data.Type == JTokenType.Object)
                {
                    return (
                        data["branchName"]?.ToString() ?? "N/A",
                        data["branchAddress"]?.ToString() ?? "N/A",
                        data["businessName"]?.ToString() ?? "N/A"
                    );
                }
            }
            catch (Exception ex)
            {
                ShowMessage($"⚠️ Error extracting branch details: {ex.Message}", false, true);
            }
            return ("N/A", "N/A", "N/A");
        }

        private void SaveAllConfigs(string username, string password, string mac, string dbPath, string branchName, string branchAddress, string businessName)
        {
            try
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.Load(_xmlConfigPath);
                UpdateOrCreateNode(xmlDoc, "Username", AesEncryptionHelper.Encrypt(username));
                UpdateOrCreateNode(xmlDoc, "Password", AesEncryptionHelper.Encrypt(password));
                UpdateOrCreateNode(xmlDoc, "MacAddress", mac);
                xmlDoc.Save(_xmlConfigPath);
            }
            catch (Exception ex)
            {
                ShowMessage($"⚠️ Failed to update XML config: {ex.Message}", false, true);
            }

            try
            {
                SaveDbPathToJson(_jsonWorkerPath, dbPath, username);
                SaveDbPathToJson(_jsonMainPath, dbPath, username);
                SaveDbPathToWinFormsConfig(dbPath, branchName, branchAddress, businessName);
            }
            catch (Exception ex)
            {
                ShowMessage($"⚠️ Failed to update DB path configs: {ex.Message}", false, true);
            }
        }

        private bool VerifyAuthentication(JObject json)
        {
            try
            {
                string statusCode = json["statusCode"]?.ToString();
                string message = json["message"]?.ToString()?.ToLower();
                string serverMsg = json["message"]?.ToString() ?? "";

                if (string.IsNullOrWhiteSpace(statusCode))
                {
                    ShowMessage("❌ Response missing status code.", false, false);
                    return false;
                }

                return statusCode switch
                {
                    "200" when message?.Contains("record found") == true || message?.Contains("success") == true
                        => true,

                    "401" => ShowError("❌ Invalid POS ID or access code."),
                    "403" => ShowError("❌ Access denied. Unauthorized device."),
                    "404" => ShowError("❌ MAC address verification failed."),
                    "500" => ShowError("❌ Internal server error."),
                    _ => ShowError($"❌ Unexpected response: {statusCode}\n{serverMsg}")
                };
            }
            catch (Exception ex)
            {
                ShowMessage($"❌ Authentication verification failed: {ex.Message}", false, false);
                return false;
            }
        }

        private bool ShowError(string msg)
        {
            ShowMessage(msg, false, false);
            return false;
        }

        private void SaveEnvironmentSettings()
        {
            try
            {
                if (rdoProduction.Checked)
                    SaveEnvironmentToApiConfig("Production");
                else if (rdoSandbox.Checked)
                    SaveEnvironmentToApiConfig("Sandbox");
            }
            catch (Exception ex)
            {
                ShowMessage($"⚠️ Failed to save environment settings: {ex.Message}", false, true);
            }
        }

        private void UpdateSetupConfig(string dbPath)
        {
            try
            {
                var docSetup = new XmlDocument();
                docSetup.Load(_setupConfigPath);
                UpdateOrCreateNode(docSetup, "DefaultDBFilePath", dbPath);
                docSetup.Save(_setupConfigPath);
            }
            catch (Exception ex)
            {
                ShowMessage($"⚠️ Failed to update SetupUI config.\n{ex.Message}", false, true);
            }
        }

        private bool InitializeDatabase(string dbPath)
        {
            try
            {
                var sqliteOptions = new DbContextOptionsBuilder<SqliteDbContext>()
                    .UseSqlite($"Data Source={dbPath}")
                    .Options;

                using var context = new SqliteDbContext(sqliteOptions);
                context.Database.EnsureCreated();
                return true;
            }
            catch (Exception ex)
            {
                ShowMessage($"❌ Failed to initialize database: {ex.Message}", false, false);
                return false;
            }
        }

        // --- Update or create XML node ---
        private void UpdateOrCreateNode(XmlDocument doc, string key, string value)
        {
            var node = doc.SelectSingleNode($"//appSettings/add[@key='{key}']");
            if (node == null)
            {
                var appSettings = doc.SelectSingleNode("//appSettings") ?? doc.CreateElement("appSettings");
                if (appSettings.ParentNode == null)
                    doc.DocumentElement.AppendChild(appSettings);

                XmlElement newNode = doc.CreateElement("add");
                newNode.SetAttribute("key", key);
                newNode.SetAttribute("value", value);
                appSettings.AppendChild(newNode);
            }
            else
            {
                node.Attributes["value"].Value = value;
            }
        }

        // --- Save DB path to JSON ---
        private void SaveDbPathToJson(string jsonFilePath, string dbPath, string posId)
        {
            try
            {
                JObject root;

                // Read existing JSON if available
                if (File.Exists(jsonFilePath))
                {
                    string text = File.ReadAllText(jsonFilePath);
                    root = string.IsNullOrWhiteSpace(text) ? new JObject() : JObject.Parse(text);
                }
                else
                {
                    root = new JObject();
                }

                // Ensure AppSettings object exists
                if (root["AppSettings"] == null || root["AppSettings"].Type != JTokenType.Object)
                    root["AppSettings"] = new JObject();

                // ✅ Update DB file path and POS ID
                root["AppSettings"]["DefaultDBFilePath"] = dbPath;
                root["AppSettings"]["POS"] = posId;

                // Write updated JSON
                File.WriteAllText(jsonFilePath, root.ToString(Newtonsoft.Json.Formatting.Indented));
            }
            catch (Exception ex)
            {
                ShowMessage($"⚠️ Failed to update {Path.GetFileName(jsonFilePath)}: {ex.Message}", false, true);
            }
        }

        private void SaveEnvironmentToApiConfig(string environment)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(_jsonMainPath))
                {
                    ShowMessage("⚠️ API config path not found (_jsonMainPath is empty).", false, true);
                    return;
                }

                if (!File.Exists(_jsonMainPath))
                {
                    ShowMessage($"⚠️ API config file not found at:\n{_jsonMainPath}", false, true);
                    return;
                }

                string json = File.ReadAllText(_jsonMainPath);
                dynamic config = JsonConvert.DeserializeObject(json) ?? new JObject();

                if (config["AppSettings"] == null)
                    config["AppSettings"] = new JObject();

                // Update environment key
                config["AppSettings"]["Environment"] = environment;

                // Update API base URL (optional)
                string apiUrl = environment.Equals("Production", StringComparison.OrdinalIgnoreCase)
                    ? "https://api.yourdomain.com"
                    : "https://sandbox.api.yourdomain.com";
                config["AppSettings"]["ApiBaseUrl"] = apiUrl;

                // ✅ Update isProduction flag
                bool isProd = environment.Equals("Production", StringComparison.OrdinalIgnoreCase);
                config["AppSettings"]["isProduction"] = isProd;

                // ✅ Save back to file (indented, human-readable)
                File.WriteAllText(_jsonMainPath, JsonConvert.SerializeObject(config, Newtonsoft.Json.Formatting.Indented));
            }
            catch (Exception ex)
            {
                ShowMessage($"❌ Failed to update API config: {ex.Message}", false, false);
            }
        }

        // --- Save DB path and branch information to WinForms config ---
        private void SaveDbPathToWinFormsConfig(string dbPath, string branchName = "N/A", string branchAddress = "N/A", string businessName = "N/A")
        {
            try
            {
                var doc = new XmlDocument();
                doc.Load(_winformsConfigPath);

                // Update or create DefaultDBFilePath
                var node = doc.SelectSingleNode("//appSettings/add[@key='DefaultDBFilePath']");
                if (node == null)
                {
                    var appSettings = doc.SelectSingleNode("//appSettings") ?? doc.CreateElement("appSettings");
                    if (appSettings.ParentNode == null)
                        doc.DocumentElement.AppendChild(appSettings);

                    XmlElement newNode = doc.CreateElement("add");
                    newNode.SetAttribute("key", "DefaultDBFilePath");
                    newNode.SetAttribute("value", dbPath);
                    appSettings.AppendChild(newNode);
                }
                else
                {
                    node.Attributes["value"].Value = dbPath;
                }

                // Update or create branchName
                UpdateOrCreateNode(doc, "branchName", branchName);

                // Update or create branchAddress
                UpdateOrCreateNode(doc, "branchAddress", branchAddress);

                // Update or create businessName
                UpdateOrCreateNode(doc, "businessName", businessName);

                doc.Save(_winformsConfigPath);
            }
            catch (Exception ex)
            {
                ShowMessage($"⚠️ Failed to update WinForms config: {ex.Message}", false, true);
            }
        }

        // --- Get MAC address ---
        private string GetMacAddress()
        {
            try
            {
                var nic = NetworkInterface.GetAllNetworkInterfaces()
                    .FirstOrDefault(n => n.OperationalStatus == OperationalStatus.Up &&
                                         n.NetworkInterfaceType != NetworkInterfaceType.Loopback);
                return nic?.GetPhysicalAddress().ToString() ?? "UNKNOWN";
            }
            catch
            {
                return "UNKNOWN";
            }
        }

        private string GetDeviceMacAddress()
        {
            try
            {
                var nics = NetworkInterface.GetAllNetworkInterfaces()
                    .Where(n =>
                        n.OperationalStatus == OperationalStatus.Up &&
                        (n.NetworkInterfaceType == NetworkInterfaceType.Ethernet ||
                         n.NetworkInterfaceType == NetworkInterfaceType.Wireless80211) &&
                        !n.Description.ToLower().Contains("virtual") &&
                        !n.Description.ToLower().Contains("vpn"));

                var nic = nics.FirstOrDefault();

                if (nic != null)
                {
                    return nic.GetPhysicalAddress().ToString();
                }

                return "UNKNOWN";
            }
            catch
            {
                return "UNKNOWN";
            }
        }

        // --- Cancel button ---
        private void btnCancel_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show(
                "Are you sure you want to cancel?",
                "Cancel Setup",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );

            if (result == DialogResult.Yes)
            {
                try
                {
                    // release topmost before exit
                    this.TopMost = false;
                    SetWindowPos(this.Handle, HWND_NOTOPMOST, 0, 0, 0, 0,
                                 SWP_NOMOVE | SWP_NOSIZE | SWP_SHOWWINDOW);

                    // gracefully close WinForms UI
                    Application.ExitThread();
                    Application.Exit();

                    // notify MSI with cancel exit code
                    Environment.Exit(1602);
                }
                catch
                {
                    Environment.Exit(1602);
                }
            }
        }
    }
}