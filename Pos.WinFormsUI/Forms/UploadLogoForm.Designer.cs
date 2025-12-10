namespace Pos.WinFormsUI.Forms
{
    partial class UploadLogoForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            txtFileName = new TextBox();
            btnBrowse = new Button();
            btnOK = new Button();
            btnCancel = new Button();
            SuspendLayout();
            // 
            // txtFileName
            // 
            txtFileName.Location = new Point(128, 74);
            txtFileName.Name = "txtFileName";
            txtFileName.Size = new Size(179, 27);
            txtFileName.TabIndex = 0;
            // 
            // btnBrowse
            // 
            btnBrowse.Location = new Point(313, 74);
            btnBrowse.Name = "btnBrowse";
            btnBrowse.Size = new Size(94, 27);
            btnBrowse.TabIndex = 1;
            btnBrowse.Text = "Browse";
            btnBrowse.UseVisualStyleBackColor = true;
            btnBrowse.Click += btnBrowse_Click; 
            // 
            // btnOK
            // 
            btnOK.Location = new Point(213, 107);
            btnOK.Name = "btnOK";
            btnOK.Size = new Size(94, 29);
            btnOK.TabIndex = 2;
            btnOK.Text = "OK";
            btnOK.UseVisualStyleBackColor = true;
            btnOK.Click += btnUpload_Click;
            // 
            // btnCancel
            // 
            btnCancel.Location = new Point(313, 107);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(94, 29);
            btnCancel.TabIndex = 3;
            btnCancel.Text = "Cancel ";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += btnCancel_Click; 
            // 
            // UploadLogoForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(522, 249);
            Controls.Add(btnCancel);
            Controls.Add(btnOK);
            Controls.Add(btnBrowse);
            Controls.Add(txtFileName);
            Name = "UploadLogoForm";
            Text = "Upload Company Logo";
            StartPosition = FormStartPosition.CenterScreen;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox txtFileName;
        private Button btnBrowse;
        private Button btnOK;
        private Button btnCancel;
    }
}