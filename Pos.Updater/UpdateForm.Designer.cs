namespace Pos.Updater
{
    partial class UpdateForm
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
            progressBar = new ProgressBar();
            lblStatus = new Label();
            SuspendLayout();
            // 
            // progressBar
            // 
            progressBar.Location = new Point(31, 75);
            progressBar.Name = "progressBar";
            progressBar.Size = new Size(440, 26);
            progressBar.Style = ProgressBarStyle.Marquee;
            progressBar.TabIndex = 2;
            // 
            // lblStatus
            // 
            lblStatus.AutoSize = true;
            lblStatus.Font = new Font("Segoe UI", 9F);
            lblStatus.ForeColor = Color.FromArgb(60, 60, 60);
            lblStatus.Location = new Point(31, 45);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(158, 20);
            lblStatus.TabIndex = 1;
            lblStatus.Text = "Checking for updates...";
            // 
            // UpdateForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(500, 150);
            Controls.Add(lblStatus);
            Controls.Add(progressBar);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "UpdateForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "POS-PRA Updater";
            TopMost = true;
            Load += UpdateForm_Load;
            ResumeLayout(false);
            PerformLayout();
        }
    }
}
