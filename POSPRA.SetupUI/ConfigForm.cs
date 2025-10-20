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


        public ConfigForm(string xmlConfigPath, string jsonWorkerPath, string jsonMainPath, string setupConfigPath, string winformsConfigPath)
        {
            InitializeComponent();
            // Always stay above all other windows
            this.TopMost = true;

            // Make sure it stays focused
            this.BringToFront();
            this.Activate();

            //this.StartPosition = FormStartPosition.CenterScreen;

            //this.Load += ConfigForm_Load;

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
            string value = ConfigurationManager.AppSettings["DefaultDBFilePath"];
            ClearAllFields();

            // Load default DB path from App.config
            string defaultPath = ConfigurationManager.AppSettings["DefaultDBFilePath"];
            if (!string.IsNullOrWhiteSpace(defaultPath))
            {
                txtFilePath.Text = defaultPath;
            }

        }

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
                // ======== 1️⃣ VALIDATION ========
                string username = txtUsername.Text.Trim();
                string password = txtPassword.Text.Trim();
                string dbPath = txtFilePath.Text.Trim();

                if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                {
                    MessageBox.Show("Please enter both POS ID and Access Code.", "Validation Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(dbPath))
                {
                    MessageBox.Show("Please select a database file path.", "Validation Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                Directory.CreateDirectory(Path.GetDirectoryName(dbPath));

                // ======== 2️⃣ MAC ADDRESSES ========
                string mac = string.Empty;
                string deviceMac = string.Empty;

                try
                {
                    mac = GetMacAddress();
                    deviceMac = GetDeviceMacAddress();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to read MAC address: {ex.Message}",
                        "Network Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

                // ======== 3️⃣ API CALL ========
                JObject json = null;
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
                        MessageBox.Show("API URL is missing in App.config.", "Configuration Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    using (var client = new HttpClient { Timeout = TimeSpan.FromSeconds(10) })
                    {
                        client.DefaultRequestHeaders.Clear();
                        client.DefaultRequestHeaders.Add("Accept", "application/json");

                        var jsonContent = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");
                        var response = await client.PostAsync(apiUrl, jsonContent);
                        string responseBody = await response.Content.ReadAsStringAsync();

                        if (!response.IsSuccessStatusCode)
                        {
                            MessageBox.Show($"Authentication failed. Server returned {(int)response.StatusCode}: {response.ReasonPhrase}",
                                "API Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }

                        try
                        {
                            json = JObject.Parse(responseBody);

                            // If the API wraps JSON inside a string
                            if (json.Type == JTokenType.String)
                                json = JObject.Parse(json.ToString());
                            else if (json["response"] != null && json["response"].Type == JTokenType.String)
                                json = JObject.Parse(json["response"].ToString());
                        }
                        catch (Exception jex)
                        {
                            MessageBox.Show($"Error parsing API response: {jex.Message}",
                                "JSON Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                    }
                }
                catch (HttpRequestException ex)
                {
                    MessageBox.Show($"Network error while connecting to API:\n{ex.Message}",
                        "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                catch (TaskCanceledException)
                {
                    MessageBox.Show("The API request timed out. Please check your internet or server availability.",
                        "Timeout", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Unexpected API error: {ex.Message}",
                        "API Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // ======== 4️⃣ SAFE JSON FIELD EXTRACTION ========
                string branchName = "N/A";
                string branchAddress = "N/A";
                string businessName = "N/A";

                try
                {
                    var data = json["data"];
                    if (data != null && data.Type == JTokenType.Object)
                    {
                        branchName = data["branchName"]?.ToString() ?? "N/A";
                        branchAddress = data["branchAddress"]?.ToString() ?? "N/A";
                        businessName = data["businessName"]?.ToString() ?? "N/A";
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error extracting branch details: {ex.Message}",
                        "Data Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

                // ======== 5️⃣ SAVE CONFIG FILES ========
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
                    MessageBox.Show($"Failed to update XML config: {ex.Message}",
                        "Config Save Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

                try
                {
                    SaveDbPathToJson(_jsonWorkerPath, dbPath, username);
                    SaveDbPathToJson(_jsonMainPath, dbPath, username);
                    SaveDbPathToWinFormsConfig(dbPath, branchName, branchAddress, businessName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to update DB path configs: {ex.Message}",
                        "Config Save Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

                // ======== 6️⃣ VERIFY AUTH RESPONSE ========
                try
                {
                    if (json == null)
                    {
                        MessageBox.Show("No response received from the server.",
                            "Response Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    string statusCode = json["statusCode"]?.ToString();
                    string message = json["message"]?.ToString()?.ToLower();
                    string serverMsg = json["message"]?.ToString() ?? "";

                    // Handle missing fields explicitly
                    if (statusCode == null)
                    {
                        MessageBox.Show("Response did not contain a status code. The API may have returned an unexpected format.",
                            "Response Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    // Handle known status codes precisely
                    switch (statusCode)
                    {
                        case "200":
                            bool success = message?.Contains("record found") == true || message?.Contains("success") == true;
                            if (!success)
                            {
                                MessageBox.Show($"Authentication failed: {serverMsg}",
                                    "Authentication Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                return;
                            }

                            MessageBox.Show("✅ Authentication successful! Proceeding with configuration...",
                                "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            break;

                        case "401":
                            MessageBox.Show("Invalid POS ID or access code. Please check your credentials.",
                                "Invalid Credentials", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;

                        case "403":
                            MessageBox.Show("Access denied. This device is not authorized to use the system.",
                                "Unauthorized Access", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;

                        case "404":
                            // ✅ Based on your API behavior, 404 = MAC not found / not registered
                            MessageBox.Show("MAC address verification failed. This device is not registered or recognized.",
                                "MAC Address Mismatch", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;

                        case "500":
                            MessageBox.Show("The server encountered an internal error. Please try again later.",
                                "Server Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;

                        default:
                            MessageBox.Show($"Unexpected response from server (Status Code: {statusCode}).\nMessage: {serverMsg}",
                                "Unexpected Response", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                    }
                }
                catch (JsonReaderException jex)
                {
                    MessageBox.Show($"Invalid JSON format received from API: {jex.Message}",
                        "Response Parsing Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch (NullReferenceException nex)
                {
                    MessageBox.Show($"Expected response fields are missing (statusCode/message). Details: {nex.Message}",
                        "Missing Data", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch (HttpRequestException hex)
                {
                    MessageBox.Show($"Network or connectivity issue while verifying authentication.\nDetails: {hex.Message}",
                        "Network Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Unexpected error while verifying authentication: {ex.Message}",
                        "Response Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }


                // ======== 7️⃣ UPDATE ENVIRONMENT ========
                try
                {
                    if (rdoProduction.Checked)
                        SaveEnvironmentToApiConfig("Production");
                    else if (rdoSandbox.Checked)
                        SaveEnvironmentToApiConfig("Sandbox");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to save environment settings: {ex.Message}",
                        "Environment Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

                // ======== 8️⃣ UPDATE SETUP CONFIG ========
                try
                {
                    var docSetup = new XmlDocument();
                    docSetup.Load(_setupConfigPath);
                    UpdateOrCreateNode(docSetup, "DefaultDBFilePath", dbPath);
                    docSetup.Save(_setupConfigPath);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Warning: failed to update SetupUI config.\n{ex.Message}",
                        "Config Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

                // ======== 9️⃣ INITIALIZE DATABASE ========
                try
                {
                    var sqliteOptions = new DbContextOptionsBuilder<SqliteDbContext>()
                        .UseSqlite($"Data Source={dbPath}")
                        .Options;

                    using (var context = new SqliteDbContext(sqliteOptions))
                    {
                        context.Database.EnsureCreated();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to initialize database: {ex.Message}",
                        "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // ======== 🔟 SUCCESS EXIT ========
                MessageBox.Show("Configuration saved and authentication successful.", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                Environment.Exit(0);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unexpected fatal error: {ex.Message}",
                    "Critical Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

                // ✅ Show confirmation
                string savedPath = root["AppSettings"]["DefaultDBFilePath"]?.ToString() ?? "(no path found)";
                string savedPos = root["AppSettings"]["POS"]?.ToString() ?? "(no POS found)";
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Failed to update {Path.GetFileName(jsonFilePath)}: {ex.Message}",
                    "Warning",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
            }
        }

        private void SaveEnvironmentToApiConfig(string environment)
        {
            try
            {
                // 🔹 jsonMainPath should be a class-level or passed variable from Program.cs
                if (string.IsNullOrWhiteSpace(_jsonMainPath))
                {
                    MessageBox.Show("API config path not found (_jsonMainPath is empty).", "Warning",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (!File.Exists(_jsonMainPath))
                {
                    MessageBox.Show($"API config file not found at:\n{_jsonMainPath}", "Config Missing",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string json = File.ReadAllText(_jsonMainPath);
                dynamic config = JsonConvert.DeserializeObject(json) ?? new JObject();

                if (config["AppSettings"] == null)
                    config["AppSettings"] = new JObject();

                // ✅ Update environment key
                config["AppSettings"]["Environment"] = environment;

                // ✅ Update API base URL (optional)
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
                MessageBox.Show($"Failed to update API config: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                MessageBox.Show($"Failed to update WinForms config: {ex.Message}", "Warning",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                    return nic.GetPhysicalAddress().ToString(); // returns like 001A2B3C4D5E
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
                    Application.ExitThread();  // close the UI thread
                    Application.Exit();        // exit application loop

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
