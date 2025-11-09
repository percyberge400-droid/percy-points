namespace POSPRA.Launcher
{
    partial class LauncherForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.Label lblStatus;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            progressBar = new System.Windows.Forms.ProgressBar();
            lblStatus = new System.Windows.Forms.Label();
            SuspendLayout();
            // 
            // progressBar
            // 
            progressBar.Location = new System.Drawing.Point(31, 75);
            progressBar.Name = "progressBar";
            progressBar.Size = new System.Drawing.Size(440, 26);
            progressBar.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
            progressBar.TabIndex = 2;
            // 
            // lblStatus
            // 
            lblStatus.AutoSize = true;
            lblStatus.Font = new System.Drawing.Font("Segoe UI", 9F);
            lblStatus.ForeColor = System.Drawing.Color.FromArgb(60, 60, 60);
            lblStatus.Location = new System.Drawing.Point(31, 45);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new System.Drawing.Size(158, 20);
            lblStatus.TabIndex = 1;
            lblStatus.Text = "Checking for updates...";
            // 
            // LauncherForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            BackColor = System.Drawing.Color.White;
            ClientSize = new System.Drawing.Size(500, 150);
            Controls.Add(lblStatus);
            Controls.Add(progressBar);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "LauncherForm";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "POS-PRA Launcher";
            TopMost = true;
            Load += LauncherForm_Load;
            ResumeLayout(false);
            PerformLayout();
        }
    }
}
