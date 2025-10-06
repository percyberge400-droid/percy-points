using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using POSPRA.SecurityEncryption;
using System;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace POSPRA.SetupUI
{
    public partial class ConfigForm : Form
    {
        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(
            IntPtr hWnd,
            IntPtr hWndInsertAfter,
            int X,
            int Y,
            int cx,
            int cy,
            uint uFlags);

        private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        private const uint SWP_NOMOVE = 0x0002;
        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_SHOWWINDOW = 0x0040;

        private readonly string _configPath;

        public ConfigForm(string configPath)
        {
            InitializeComponent();
            _configPath = configPath;

            txtUsername.KeyPress += txtUsername_KeyPress;
            txtPassword.KeyPress += txtPassword_KeyPress;
            txtUsername.TextChanged += ValidateForm;
            txtPassword.TextChanged += ValidateForm;

           // btnOk.Enabled = false;
            this.AcceptButton = btnOk;
        }

        protected override void OnShown(EventArgs e)
        {
            //base.OnShown(e);
            //this.CenterToScreen();
            //this.TopMost = true;

            //SetWindowPos(this.Handle, HWND_TOPMOST, 0, 0, 0, 0,
            //             SWP_NOMOVE | SWP_NOSIZE | SWP_SHOWWINDOW);
            //this.Activate();
            //this.BringToFront();
        }

        private void ConfigForm_Load(object sender, EventArgs e)
        {
            this.StartPosition = FormStartPosition.CenterScreen;
            SetWindowPos(this.Handle, HWND_TOPMOST, 0, 0, 0, 0,
                SWP_NOMOVE | SWP_NOSIZE | SWP_SHOWWINDOW);

            try
            {
                var doc = new XmlDocument();
                doc.Load(_configPath);

                var userNode = doc.SelectSingleNode("//appSettings/add[@key='Username']");
                var passNode = doc.SelectSingleNode("//appSettings/add[@key='Password']");
                var dbPathNode = doc.SelectSingleNode("//appSettings/add[@key='DefaultDBFilePath']");

                if (userNode != null)
                    txtUsername.Text = AesEncryptionHelper.Decrypt(userNode.Attributes["value"].Value);

                if (passNode != null)
                    txtPassword.Text = AesEncryptionHelper.Decrypt(passNode.Attributes["value"].Value);

                if (dbPathNode != null)
                    txtFilePath.Text = dbPathNode.Attributes["value"].Value; // not encrypted
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error reading config: " + ex.Message);
            }
        }

        // ✅ Browse button for selecting DB file path
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

        private void txtUsername_KeyPress(object sender, KeyPressEventArgs e)
        {
            //if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            //    e.Handled = true;

            //if (txtUsername.Text.Length >= 6 && !char.IsControl(e.KeyChar))
            //    e.Handled = true;
        }

        private void txtPassword_KeyPress(object sender, KeyPressEventArgs e)
        {
            //if (!char.IsControl(e.KeyChar) && !char.IsLetterOrDigit(e.KeyChar))
            //    e.Handled = true;

            //if (txtPassword.Text.Length >= 8 && !char.IsControl(e.KeyChar))
            //    e.Handled = true;
        }

        private void ValidateForm(object sender, EventArgs e)
        {
            //bool validUsername = System.Text.RegularExpressions.Regex.IsMatch(txtUsername.Text, @"^\d{6}$");
           // bool validPassword = System.Text.RegularExpressions.Regex.IsMatch(txtPassword.Text, @"^[a-zA-Z0-9]{8}$");
          //  btnOk.Enabled = validUsername;
        }

        private async void btnOk_Click(object sender, EventArgs e)
        {
            try
            {
                string username = txtUsername.Text.Trim();
                string password = txtPassword.Text.Trim();
                string dbPath = txtFilePath.Text.Trim();

                // ✅ Basic validation
                //if (!System.Text.RegularExpressions.Regex.IsMatch(username, @"^\d{6}$"))
                //{
                //    MessageBox.Show("Username must be exactly 6 numeric digits.");
                //    return;
                //}

                //if (!System.Text.RegularExpressions.Regex.IsMatch(password, @"^[a-zA-Z0-9]$"))
                //{
                //    MessageBox.Show("Password must be exactly 8 alphanumeric characters.");
                //    return;
                //}

                if (string.IsNullOrWhiteSpace(dbPath))
                {
                    MessageBox.Show("Please select a database file path.");
                    return;
                }

                // ✅ Auto-create folder if missing
                string folderPath = Path.GetDirectoryName(dbPath);
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                // ✅ Get MAC address
                string macAddress = GetMacAddress();

                // ✅ Prepare authentication payload
                var payload = new
                {
                    posId = int.Parse(username),
                    macAddress = "0293-33E7-F772-D133-52BE-B65E-8E17-A9FE",
                    token = password
                };

                string apiUrl = "http://10.16.67.30:8020/api/Live/authenticate-by-mac";

                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Add("accept", "text/plain");

                    var jsonContent = new StringContent(
                        JsonConvert.SerializeObject(payload),
                        Encoding.UTF8,
                        "application/json"
                    );

                    // ✅ Call API
                    var response = await client.PostAsync(apiUrl, jsonContent);
                    string responseBody = await response.Content.ReadAsStringAsync();

                    if (!response.IsSuccessStatusCode)
                    {
                        MessageBox.Show("Authentication failed: Invalid credentials or MAC address.",
                            "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    try
                    {
                        // ✅ Parse JSON response
                        var json = JObject.Parse(responseBody);
                        string statusCode = json["statusCode"]?.ToString();
                        bool data = json["data"]?.ToObject<bool>() ?? false;
                        string message = json["message"]?.ToString();

                        // ✅ Check success condition
                        if (statusCode == "200" && data)
                        {
                            MessageBox.Show("Authentication successful: " + message, "Success",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            MessageBox.Show("Authentication failed: " + message, "Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("Invalid server response format.", "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }

                // ✅ Save config after successful authentication
                var doc = new XmlDocument();
                doc.Load(_configPath);

                UpdateOrCreateNode(doc, "Username", AesEncryptionHelper.Encrypt(username));
                UpdateOrCreateNode(doc, "Password", AesEncryptionHelper.Encrypt(password));
                UpdateOrCreateNode(doc, "DefaultDBFilePath", dbPath);
                UpdateOrCreateNode(doc, "MacAddress", macAddress);

                doc.Save(_configPath);

                MessageBox.Show("Configuration saved successfully.", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                Environment.Exit(0);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(1);
            }
        }


        private void UpdateOrCreateNode(XmlDocument doc, string key, string value)
        {
            var node = doc.SelectSingleNode($"//appSettings/add[@key='{key}']");
            if (node == null)
            {
                var appSettings = doc.SelectSingleNode("//appSettings");
                if (appSettings == null)
                {
                    appSettings = doc.CreateElement("appSettings");
                    doc.DocumentElement.AppendChild(appSettings);
                }

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

        // ✅ Get MAC address of first active network adapter
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
                Environment.Exit(1602);
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnBrowse_Click_1(object sender, EventArgs e)
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
    }
}
