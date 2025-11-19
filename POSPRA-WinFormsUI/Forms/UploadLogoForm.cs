using System.Drawing.Drawing2D;
using System.Xml;

namespace POSPRA_WinFormsUI.Forms
{
    public partial class UploadLogoForm : Form
    {
        private Label lblTitle;
        private Label lblInstruction;
        private PictureBox picPreview;
        private Panel panelMain;
        private Panel panelButtons;

        public UploadLogoForm()
        {
            InitializeComponent();
            StyleForm();
        }

        private void StyleForm()
        {
            // Form settings
            this.BackColor = Color.FromArgb(248, 249, 250);
            this.FormBorderStyle = FormBorderStyle.None;
            this.Size = new Size(620, 500);
            this.StartPosition = FormStartPosition.CenterParent;

            // Create main panel with rounded corners
            panelMain = new Panel
            {
                BackColor = Color.White,
                Location = new Point(20, 20),
                Size = new Size(580, 460),
                Padding = new Padding(30)
            };
            panelMain.Paint += PanelMain_Paint;
            this.Controls.Add(panelMain);
            panelMain.BringToFront();

            // Title label
            lblTitle = new Label
            {
                Text = "UPLOAD COMPANY LOGO",
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                ForeColor = ColorTranslator.FromHtml("#48A787"),
                AutoSize = true,
                Location = new Point(30, 25)
            };
            panelMain.Controls.Add(lblTitle);

            // Instruction label
            lblInstruction = new Label
            {
                Text = "Select an image file (PNG, JPG) - Max 1MB",
                Font = new Font("Segoe UI", 10F, FontStyle.Regular),
                ForeColor = Color.FromArgb(108, 117, 125),
                AutoSize = true,
                Location = new Point(30, 65)
            };
            panelMain.Controls.Add(lblInstruction);

            // Preview picture box with checkered background
            picPreview = new PictureBox
            {
                Location = new Point(40, 100),
                Size = new Size(220, 220),
                BorderStyle = BorderStyle.None,
                SizeMode = PictureBoxSizeMode.CenterImage, // Changed from Zoom
                BackColor = Color.White
            };
            picPreview.Paint += PicPreview_Paint;
            panelMain.Controls.Add(picPreview);

            // File path label
            Label lblFilePath = new Label
            {
                Text = "Selected File:",
                Font = new Font("Segoe UI", 10F, FontStyle.Regular),
                ForeColor = Color.FromArgb(73, 80, 87),
                AutoSize = true,
                Location = new Point(280, 120)
            };
            panelMain.Controls.Add(lblFilePath);

            // Style existing controls
            txtFileName.Location = new Point(280, 145);
            txtFileName.Size = new Size(260, 40);
            txtFileName.Font = new Font("Segoe UI", 9.5F);
            txtFileName.BorderStyle = BorderStyle.None;
            txtFileName.ReadOnly = true;
            txtFileName.BackColor = Color.FromArgb(248, 249, 250);
            txtFileName.Multiline = true;
            txtFileName.Padding = new Padding(10, 10, 10, 10);
            txtFileName.Paint += (s, e) =>
            {
                var rect = new Rectangle(0, 0, txtFileName.Width - 1, txtFileName.Height - 1);
                using (var pen = new Pen(Color.FromArgb(206, 212, 218), 1))
                {
                    e.Graphics.DrawRectangle(pen, rect);
                }
            };
            panelMain.Controls.Add(txtFileName);

            // Style Browse button
            StyleButton(btnBrowse, ColorTranslator.FromHtml("#48A787"), Color.White, "BROWSE FILE", new Point(280, 200), new Size(260, 48));
            panelMain.Controls.Add(btnBrowse);

            // Style OK button - centered horizontally
            StyleButton(btnOK, ColorTranslator.FromHtml("#48A787"), Color.White, "UPLOAD LOGO", new Point(190, 350), new Size(200, 50));
            panelMain.Controls.Add(btnOK);

            // Style Cancel button - below OK button
            StyleButton(btnCancel, Color.FromArgb(108, 117, 125), Color.White, "CANCEL", new Point(190, 410), new Size(200, 40));
            panelMain.Controls.Add(btnCancel);

            // Remove original controls from form
            this.Controls.Remove(txtFileName);
            this.Controls.Remove(btnBrowse);
            this.Controls.Remove(btnOK);
            this.Controls.Remove(btnCancel);

            // Remove panelButtons as we're not using it anymore
            // panelButtons was created but we place buttons directly now

            // Add close button (X)
            Button btnClose = new Button
            {
                Text = "✕",
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                Size = new Size(40, 40),
                Location = new Point(panelMain.Width - 50, 10),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.Transparent,
                ForeColor = Color.FromArgb(108, 117, 125),
                Cursor = Cursors.Hand
            };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Click += (s, e) => this.Close();
            btnClose.MouseEnter += (s, e) => btnClose.ForeColor = Color.FromArgb(220, 53, 69);
            btnClose.MouseLeave += (s, e) => btnClose.ForeColor = Color.FromArgb(108, 117, 125);
            panelMain.Controls.Add(btnClose);
            btnClose.BringToFront();
        }

        private void PanelMain_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            // Draw simple rectangle with shadow
            Rectangle rect = new Rectangle(0, 0, panelMain.Width - 1, panelMain.Height - 1);

            // Shadow
            using (var shadowBrush = new SolidBrush(Color.FromArgb(30, 0, 0, 0)))
            {
                var shadowRect = new Rectangle(3, 3, rect.Width, rect.Height);
                e.Graphics.FillRectangle(shadowBrush, shadowRect);
            }

            // Main panel background
            using (var brush = new SolidBrush(Color.White))
            {
                e.Graphics.FillRectangle(brush, rect);
            }

            // Border
            using (var pen = new Pen(Color.FromArgb(222, 226, 230), 2))
            {
                e.Graphics.DrawRectangle(pen, rect);
            }
        }

        private void PicPreview_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;

            // Only draw checkered background if there's an image
            if (picPreview.Image != null)
            {
                // Draw checkered background pattern (like Photoshop transparency grid)
                int squareSize = 10;
                for (int y = 0; y < picPreview.Height; y += squareSize)
                {
                    for (int x = 0; x < picPreview.Width; x += squareSize)
                    {
                        bool isEvenSquare = ((x / squareSize) + (y / squareSize)) % 2 == 0;
                        using (var brush = new SolidBrush(isEvenSquare ? Color.FromArgb(240, 240, 240) : Color.White))
                        {
                            e.Graphics.FillRectangle(brush, x, y, squareSize, squareSize);
                        }
                    }
                }

                // Draw the image on top of checkered background
                if (picPreview.Image != null)
                {
                    // Calculate scaling to fit image in preview box while maintaining aspect ratio
                    float ratioX = (float)picPreview.Width / picPreview.Image.Width;
                    float ratioY = (float)picPreview.Height / picPreview.Image.Height;
                    float ratio = Math.Min(ratioX, ratioY);

                    int newWidth = (int)(picPreview.Image.Width * ratio);
                    int newHeight = (int)(picPreview.Image.Height * ratio);

                    int posX = (picPreview.Width - newWidth) / 2;
                    int posY = (picPreview.Height - newHeight) / 2;

                    e.Graphics.DrawImage(picPreview.Image, posX, posY, newWidth, newHeight);
                }
            }

            // Draw dashed border for preview area
            using (var pen = new Pen(ColorTranslator.FromHtml("#48A787"), 2))
            {
                pen.DashStyle = DashStyle.Dash;
                var rect = new Rectangle(1, 1, picPreview.Width - 3, picPreview.Height - 3);
                e.Graphics.DrawRectangle(pen, rect);
            }

            // If no image, show upload icon placeholder
            if (picPreview.Image == null)
            {
                string placeholder = "📁\n\nImage Preview";
                using (var brush = new SolidBrush(Color.FromArgb(108, 117, 125)))
                using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                {
                    e.Graphics.DrawString(placeholder, new Font("Segoe UI", 12F), brush, picPreview.ClientRectangle, sf);
                }
            }
        }

        private void StyleButton(Button btn, Color bgColor, Color textColor, string text, Point location, Size size)
        {
            btn.Text = text;
            btn.Font = new Font("Segoe UI", 10.5F, FontStyle.Bold);
            btn.BackColor = bgColor;
            btn.ForeColor = textColor;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.Size = size;
            btn.Location = location;
            btn.Cursor = Cursors.Hand;

            Color originalColor = bgColor;

            // Hover effects
            btn.MouseEnter += (s, e) =>
            {
                btn.BackColor = Color.FromArgb(
                    Math.Min(255, bgColor.R + 20),
                    Math.Min(255, bgColor.G + 20),
                    Math.Min(255, bgColor.B + 20)
                );
            };
            btn.MouseLeave += (s, e) => btn.BackColor = originalColor;
        }

        private GraphicsPath CreateRoundRectPath(Rectangle rect, int radius)
        {
            int d = radius * 2;
            GraphicsPath path = new GraphicsPath();
            path.AddArc(rect.X, rect.Y, d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        // YOUR ORIGINAL LOGIC BELOW
        private void btnBrowse_Click(object sender, EventArgs e)
        {
            using var ofd = new OpenFileDialog
            {
                Filter = "Image Files|*.png;*.jpg",
                Title = "Select Company Logo"
            };
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                txtFileName.Text = ofd.FileName;

                // Show preview
                try
                {
                    // Dispose old image if exists
                    if (picPreview.Image != null)
                    {
                        picPreview.Image.Dispose();
                        picPreview.Image = null;
                    }

                    using (var img = Image.FromFile(ofd.FileName))
                    {
                        picPreview.Image = new Bitmap(img);
                    }
                    picPreview.Invalidate(); // Force repaint
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading image: {ex.Message}", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnUpload_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtFileName.Text))
                {
                    MessageBox.Show("Please select a file first.", "No File Selected",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                var filePath = txtFileName.Text;
                var fileInfo = new FileInfo(filePath);
                if (!fileInfo.Exists)
                {
                    MessageBox.Show("File not found", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                if (fileInfo.Length > 1024 * 1024) // 1MB limit
                {
                    MessageBox.Show("Logo size too large. Please select an image under 1 MB", "File Too Large",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                // Convert to Base64
                string base64;
                using (var img = Image.FromFile(filePath))
                using (var ms = new MemoryStream())
                {
                    img.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                    base64 = Convert.ToBase64String(ms.ToArray());
                }
                // Shared config path
                string dir = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                    "POSPRA"
                );
                Directory.CreateDirectory(dir);
                string configFile = Path.Combine(dir, "AppSettings.config");
                // Load or create XML
                var xml = new XmlDocument();
                if (File.Exists(configFile))
                    xml.Load(configFile);
                else
                {
                    xml.AppendChild(xml.CreateXmlDeclaration("1.0", "utf-8", null));
                    xml.AppendChild(xml.CreateElement("appSettings"));
                }
                var appSettings = xml.SelectSingleNode("//appSettings");
                if (appSettings == null)
                {
                    appSettings = xml.CreateElement("appSettings");
                    xml.AppendChild(appSettings);
                }
                // Update or create logo entry
                var node = appSettings.SelectSingleNode("add[@key='CompLogobase64']") as XmlElement;
                if (node == null)
                {
                    node = xml.CreateElement("add");
                    node.SetAttribute("key", "CompLogobase64");
                    appSettings.AppendChild(node);
                }
                node.SetAttribute("value", base64);
                xml.Save(configFile);
                MessageBox.Show("Logo saved successfully!", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to update logo: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            // Draw form shadow
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using (var shadowBrush = new SolidBrush(Color.FromArgb(50, 0, 0, 0)))
            {
                e.Graphics.FillRectangle(shadowBrush, 0, 0, this.Width, this.Height);
            }
        }
    }
}