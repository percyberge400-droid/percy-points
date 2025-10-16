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
                string username = txtUsername.Text.Trim();
                string password = txtPassword.Text.Trim();
                string dbPath = txtFilePath.Text.Trim();
                //MessageBox.Show(_jsonWorkerPath);
                //MessageBox.Show(_jsonMainPath);

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

                // ✅ Auto-create DB folder if missing
                Directory.CreateDirectory(Path.GetDirectoryName(dbPath));

                string mac = GetMacAddress();

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
                //Call Api
                using (var client = new HttpClient { Timeout = TimeSpan.FromSeconds(10) })
                {
                    client.DefaultRequestHeaders.Clear();
                    client.DefaultRequestHeaders.Add("Accept", "application/json");

                    var jsonContent = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");
                    var response = await client.PostAsync(apiUrl, jsonContent);

                    var responseBody = await response.Content.ReadAsStringAsync();

                    if (!response.IsSuccessStatusCode)
                    {
                        MessageBox.Show($"Authentication failed. Server returned {(int)response.StatusCode}: {response.ReasonPhrase}",
                            "API Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    //MessageBox.Show(responseBody.data)


                    var json = JObject.Parse(responseBody);

                    // If the API wraps JSON inside a string, detect and re-parse it
                    if (json.Type == JTokenType.String)
                    {
                        json = JObject.Parse(json.ToString());
                    }
                    else if (json["response"] != null && json["response"].Type == JTokenType.String)
                    {
                        json = JObject.Parse(json["response"].ToString());
                    }
                    string branchName = json["data"]?["branchName"]?.ToString() ?? "N/A";
                    string branchAddress = json["data"]?["branchAddress"]?.ToString() ?? "N/A";
                    string businessName = json["data"]?["businessName"]?.ToString() ?? "N/A";

                    // ✅ Save credentials + MAC to XML
                    var doc = new XmlDocument();
                    doc.Load(_xmlConfigPath);
                    UpdateOrCreateNode(doc, "Username", AesEncryptionHelper.Encrypt(username));
                    UpdateOrCreateNode(doc, "Password", AesEncryptionHelper.Encrypt(password));
                    UpdateOrCreateNode(doc, "MacAddress", mac);
                    doc.Save(_xmlConfigPath);

                    // ✅ Update DB path in other configs
                    SaveDbPathToJson(_jsonWorkerPath, dbPath, username);
                    SaveDbPathToJson(_jsonMainPath, dbPath, username);
                    SaveDbPathToWinFormsConfig(dbPath, branchName, branchAddress, businessName);

                    try
                    {
                        string? statusCode = json["statusCode"]?.ToString();
                        string? message = json["message"]?.ToString()?.ToLower();

                        bool success = statusCode == "200" &&
                                       (message?.Contains("record found") == true || message?.Contains("success") == true);

                        if (!success)
                        {
                            string error = json["message"]?.ToString() ?? "Invalid credentials or MAC address.";
                            MessageBox.Show("Authentication failed: " + error,
                                "Auth Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }

                        MessageBox.Show("✅ Authentication successful! Proceeding with configuration...",
                            "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error while parsing authentication response: {ex.Message}",
                            "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }

                }




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

                // ✅ Initialize SQLite DB
                var sqliteOptions = new DbContextOptionsBuilder<SqliteDbContext>()
                    .UseSqlite($"Data Source={dbPath}")
                    .Options;

                using (var context = new SqliteDbContext(sqliteOptions))
                {
                    context.Database.EnsureCreated();
                }

                MessageBox.Show("Configuration saved and authentication successful.", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                Environment.Exit(0);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unexpected error: " + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
