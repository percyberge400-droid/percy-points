using Microsoft.Extensions.DependencyInjection;
using POSPRA.SecurityEncryption;
using System.Configuration;
using System.Drawing.Drawing2D;
using System.ServiceProcess;

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
            // ✅ Make labels dynamic from App.config
            string inquiryNo = ConfigurationManager.AppSettings["generalinquiryNo"];
            string servicesNo = ConfigurationManager.AppSettings["eServicesNo"];
            string website = ConfigurationManager.AppSettings["website"];

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
                await RestartServiceAlwaysAsync(SERVICE_NAME);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Service restart failed: {ex.Message}",
                    "Service Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
        private void btnLogin_Click(object sender, EventArgs e)
        {
            try
            {
                // Get AES-encrypted values from config
                string encUsername = ConfigurationManager.AppSettings["Username"];
                string encPassword = ConfigurationManager.AppSettings["Password"];

                // 🔑 Decrypt values before using
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
    }
}