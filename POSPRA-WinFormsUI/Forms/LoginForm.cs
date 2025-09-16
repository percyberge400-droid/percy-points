using Microsoft.Extensions.DependencyInjection;
using System.Configuration;
using System.Drawing.Drawing2D;

namespace POSPRA_WinFormsUI.Forms
{
    public partial class LoginForm : Form
    {
        private readonly IServiceProvider _provider;
        public LoginForm(IServiceProvider provider)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
            //MessageBox.Show($"Current Font: {this.Font.Name}");
            InitializeComponent();
            this.AcceptButton = btnLogin; // Pressing Enter triggers login
            // Apply rounded edges to loginBox
            MakeRoundedControl(loginBox, 25); // radius = 20px
                                              // MakeRoundedControl(txtUsername, 30); // radius = 20px
                                              //MakeRoundedControl(txtPassword, 30); // radius = 20px
            MakeRoundedControl(btnLogin, 25); // radius = 20px
            MakeRoundedControl(btnClose, 10); // radius = 20px
        }
        private void Login_Load(object sender, EventArgs e)
        {

        }

        private void leftPanel_Paint(object sender, PaintEventArgs e)
        {

        }
        private void chkShowPassword_CheckedChanged(object sender, EventArgs e)
        {
            txtPassword.UseSystemPasswordChar = !chkShowPassword.Checked;
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            try
            {

                string configUsername = ConfigurationManager.AppSettings["Username"];
                string configPassword = ConfigurationManager.AppSettings["Password"];

                string enteredUsername = txtUsername.Text.Trim();
                string enteredPassword = txtPassword.Text.Trim();

                if (enteredUsername == configUsername && enteredPassword == configPassword)
                {
                    // Open DashboardForm
                    var dashboard = _provider.GetRequiredService<Main>();
                    dashboard.Show();

                    // Hide the current login form
                    this.Hide();
                }
                else
                {
                    MessageBox.Show("Invalid credentials!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception)
            {

                throw;
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
            if (control == null) return;

            var rect = control.ClientRectangle;
            if (rect.Width == 0 || rect.Height == 0) return; // avoid errors at design time

            var path = new GraphicsPath();
            path.AddArc(rect.X, rect.Y, radius, radius, 180, 90);                         // Top-left
            path.AddArc(rect.Right - radius, rect.Y, radius, radius, 270, 90);            // Top-right
            path.AddArc(rect.Right - radius, rect.Bottom - radius, radius, radius, 0, 90); // Bottom-right
            path.AddArc(rect.X, rect.Bottom - radius, radius, radius, 90, 90);            // Bottom-left
            path.CloseFigure();

            control.Region = new Region(path);
        }
        private void loginBox_Resize(object sender, EventArgs e)
        {
            MakeRoundedControl(loginBox, 20);
        }
    }
}
