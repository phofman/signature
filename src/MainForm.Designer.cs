namespace CodeTitans.Signature
{
    partial class MainForm
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.txtLog = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.txtCertificateFilter = new System.Windows.Forms.TextBox();
            this.cmbTimestampServers = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.txtCertificatePassword = new System.Windows.Forms.TextBox();
            this.txtCertificatePath = new System.Windows.Forms.TextBox();
            this.radioPfx = new System.Windows.Forms.RadioButton();
            this.radioInstalled = new System.Windows.Forms.RadioButton();
            this.cmbCertificates = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.btnCertificateLocation = new System.Windows.Forms.Button();
            this.btnBinaryLocation = new System.Windows.Forms.Button();
            this.txtBinaryPath = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.btnClose = new System.Windows.Forms.Button();
            this.btnSign = new System.Windows.Forms.Button();
            this.homeLink = new System.Windows.Forms.LinkLabel();
            this.mainMenu = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openBinaryMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.findCertificateMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
            this.signToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.groupBox1.SuspendLayout();
            this.mainMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.txtLog);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.txtCertificateFilter);
            this.groupBox1.Controls.Add(this.cmbTimestampServers);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.txtCertificatePassword);
            this.groupBox1.Controls.Add(this.txtCertificatePath);
            this.groupBox1.Controls.Add(this.radioPfx);
            this.groupBox1.Controls.Add(this.radioInstalled);
            this.groupBox1.Controls.Add(this.cmbCertificates);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.btnCertificateLocation);
            this.groupBox1.Controls.Add(this.btnBinaryLocation);
            this.groupBox1.Controls.Add(this.txtBinaryPath);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Location = new System.Drawing.Point(12, 27);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(859, 438);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Properties";
            // 
            // txtLog
            // 
            this.txtLog.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtLog.Location = new System.Drawing.Point(20, 260);
            this.txtLog.Multiline = true;
            this.txtLog.Name = "txtLog";
            this.txtLog.ReadOnly = true;
            this.txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtLog.Size = new System.Drawing.Size(824, 159);
            this.txtLog.TabIndex = 16;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(169, 72);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(68, 13);
            this.label6.TabIndex = 6;
            this.label6.Text = "Subject filter:";
            // 
            // txtCertificateFilter
            // 
            this.txtCertificateFilter.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtCertificateFilter.Location = new System.Drawing.Point(243, 67);
            this.txtCertificateFilter.Name = "txtCertificateFilter";
            this.txtCertificateFilter.Size = new System.Drawing.Size(387, 20);
            this.txtCertificateFilter.TabIndex = 7;
            this.txtCertificateFilter.TextChanged += new System.EventHandler(this.OnCertificateFilterChanged);
            // 
            // cmbTimestampServers
            // 
            this.cmbTimestampServers.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cmbTimestampServers.FormattingEnabled = true;
            this.cmbTimestampServers.Location = new System.Drawing.Point(172, 218);
            this.cmbTimestampServers.Name = "cmbTimestampServers";
            this.cmbTimestampServers.Size = new System.Drawing.Size(622, 21);
            this.cmbTimestampServers.TabIndex = 15;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(17, 221);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(93, 13);
            this.label5.TabIndex = 14;
            this.label5.Text = "Timestamp server:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(169, 172);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(56, 13);
            this.label4.TabIndex = 12;
            this.label4.Text = "Password:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(169, 146);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(26, 13);
            this.label3.TabIndex = 9;
            this.label3.Text = "File:";
            // 
            // txtCertificatePassword
            // 
            this.txtCertificatePassword.Location = new System.Drawing.Point(243, 169);
            this.txtCertificatePassword.Name = "txtCertificatePassword";
            this.txtCertificatePassword.PasswordChar = '*';
            this.txtCertificatePassword.Size = new System.Drawing.Size(121, 20);
            this.txtCertificatePassword.TabIndex = 13;
            // 
            // txtCertificatePath
            // 
            this.txtCertificatePath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtCertificatePath.Location = new System.Drawing.Point(243, 143);
            this.txtCertificatePath.Name = "txtCertificatePath";
            this.txtCertificatePath.Size = new System.Drawing.Size(551, 20);
            this.txtCertificatePath.TabIndex = 10;
            // 
            // radioPfx
            // 
            this.radioPfx.AutoSize = true;
            this.radioPfx.Location = new System.Drawing.Point(81, 144);
            this.radioPfx.Name = "radioPfx";
            this.radioPfx.Size = new System.Drawing.Size(68, 17);
            this.radioPfx.TabIndex = 5;
            this.radioPfx.TabStop = true;
            this.radioPfx.Text = "from PFX";
            this.radioPfx.UseVisualStyleBackColor = true;
            this.radioPfx.CheckedChanged += new System.EventHandler(this.OnCertificateCheckedChanged);
            // 
            // radioInstalled
            // 
            this.radioInstalled.AutoSize = true;
            this.radioInstalled.Location = new System.Drawing.Point(81, 70);
            this.radioInstalled.Name = "radioInstalled";
            this.radioInstalled.Size = new System.Drawing.Size(63, 17);
            this.radioInstalled.TabIndex = 4;
            this.radioInstalled.TabStop = true;
            this.radioInstalled.Text = "installed";
            this.radioInstalled.UseVisualStyleBackColor = true;
            this.radioInstalled.CheckedChanged += new System.EventHandler(this.OnCertificateCheckedChanged);
            // 
            // cmbCertificates
            // 
            this.cmbCertificates.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cmbCertificates.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbCertificates.FormattingEnabled = true;
            this.cmbCertificates.Location = new System.Drawing.Point(243, 93);
            this.cmbCertificates.Name = "cmbCertificates";
            this.cmbCertificates.Size = new System.Drawing.Size(551, 21);
            this.cmbCertificates.TabIndex = 8;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(17, 72);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(57, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Certificate:";
            // 
            // btnCertificateLocation
            // 
            this.btnCertificateLocation.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCertificateLocation.Location = new System.Drawing.Point(800, 141);
            this.btnCertificateLocation.Name = "btnCertificateLocation";
            this.btnCertificateLocation.Size = new System.Drawing.Size(44, 23);
            this.btnCertificateLocation.TabIndex = 11;
            this.btnCertificateLocation.Text = "...";
            this.btnCertificateLocation.UseVisualStyleBackColor = true;
            this.btnCertificateLocation.Click += new System.EventHandler(this.btnCertificateLocation_Click);
            // 
            // btnBinaryLocation
            // 
            this.btnBinaryLocation.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnBinaryLocation.Location = new System.Drawing.Point(800, 27);
            this.btnBinaryLocation.Name = "btnBinaryLocation";
            this.btnBinaryLocation.Size = new System.Drawing.Size(44, 23);
            this.btnBinaryLocation.TabIndex = 2;
            this.btnBinaryLocation.Text = "...";
            this.btnBinaryLocation.UseVisualStyleBackColor = true;
            this.btnBinaryLocation.Click += new System.EventHandler(this.btnBinaryLocation_Click);
            // 
            // txtBinaryPath
            // 
            this.txtBinaryPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtBinaryPath.Location = new System.Drawing.Point(81, 29);
            this.txtBinaryPath.Name = "txtBinaryPath";
            this.txtBinaryPath.Size = new System.Drawing.Size(713, 20);
            this.txtBinaryPath.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(17, 32);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(39, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Binary:";
            // 
            // btnClose
            // 
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnClose.Location = new System.Drawing.Point(796, 471);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(75, 23);
            this.btnClose.TabIndex = 3;
            this.btnClose.Text = "&Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // btnSign
            // 
            this.btnSign.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSign.Location = new System.Drawing.Point(660, 471);
            this.btnSign.Name = "btnSign";
            this.btnSign.Size = new System.Drawing.Size(130, 23);
            this.btnSign.TabIndex = 2;
            this.btnSign.Text = "&Sign";
            this.btnSign.UseVisualStyleBackColor = true;
            this.btnSign.Click += new System.EventHandler(this.btnSign_Click);
            // 
            // homeLink
            // 
            this.homeLink.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.homeLink.AutoSize = true;
            this.homeLink.Location = new System.Drawing.Point(12, 476);
            this.homeLink.Name = "homeLink";
            this.homeLink.Size = new System.Drawing.Size(94, 13);
            this.homeLink.TabIndex = 1;
            this.homeLink.TabStop = true;
            this.homeLink.Text = "www.codetitans.pl";
            this.homeLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.homeLink_LinkClicked);
            // 
            // mainMenu
            // 
            this.mainMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.mainMenu.Location = new System.Drawing.Point(0, 0);
            this.mainMenu.Name = "mainMenu";
            this.mainMenu.Size = new System.Drawing.Size(883, 24);
            this.mainMenu.TabIndex = 4;
            this.mainMenu.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openBinaryMenuItem,
            this.findCertificateMenuItem,
            this.toolStripMenuItem1,
            this.signToolStripMenuItem,
            this.toolStripMenuItem2,
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "&File";
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.aboutMenuItem});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.helpToolStripMenuItem.Text = "&Help";
            // 
            // openBinaryMenuItem
            // 
            this.openBinaryMenuItem.Name = "openBinaryMenuItem";
            this.openBinaryMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
            this.openBinaryMenuItem.Size = new System.Drawing.Size(226, 22);
            this.openBinaryMenuItem.Text = "Open &binary...";
            this.openBinaryMenuItem.Click += new System.EventHandler(this.btnBinaryLocation_Click);
            // 
            // findCertificateMenuItem
            // 
            this.findCertificateMenuItem.Name = "findCertificateMenuItem";
            this.findCertificateMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.C)));
            this.findCertificateMenuItem.Size = new System.Drawing.Size(226, 22);
            this.findCertificateMenuItem.Text = "Find PFX certificate...";
            this.findCertificateMenuItem.Click += new System.EventHandler(this.btnCertificateLocation_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(223, 6);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.F4)));
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(226, 22);
            this.exitToolStripMenuItem.Text = "&Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(223, 6);
            // 
            // signToolStripMenuItem
            // 
            this.signToolStripMenuItem.Name = "signToolStripMenuItem";
            this.signToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
            this.signToolStripMenuItem.Size = new System.Drawing.Size(226, 22);
            this.signToolStripMenuItem.Text = "Sign";
            this.signToolStripMenuItem.Click += new System.EventHandler(this.btnSign_Click);
            // 
            // aboutMenuItem
            // 
            this.aboutMenuItem.Name = "aboutMenuItem";
            this.aboutMenuItem.Size = new System.Drawing.Size(152, 22);
            this.aboutMenuItem.Text = "&About...";
            this.aboutMenuItem.Click += new System.EventHandler(this.aboutMenuItem_Click);
            // 
            // MainForm
            // 
            this.AcceptButton = this.btnSign;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnClose;
            this.ClientSize = new System.Drawing.Size(883, 506);
            this.Controls.Add(this.homeLink);
            this.Controls.Add(this.btnSign);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.mainMenu);
            this.MainMenuStrip = this.mainMenu;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MainForm";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "CodeTitans Binary Signature";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.mainMenu.ResumeLayout(false);
            this.mainMenu.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.ComboBox cmbTimestampServers;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtCertificatePassword;
        private System.Windows.Forms.TextBox txtCertificatePath;
        private System.Windows.Forms.RadioButton radioPfx;
        private System.Windows.Forms.RadioButton radioInstalled;
        private System.Windows.Forms.ComboBox cmbCertificates;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnCertificateLocation;
        private System.Windows.Forms.Button btnBinaryLocation;
        private System.Windows.Forms.TextBox txtBinaryPath;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Button btnSign;
        private System.Windows.Forms.TextBox txtCertificateFilter;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox txtLog;
        private System.Windows.Forms.LinkLabel homeLink;
        private System.Windows.Forms.MenuStrip mainMenu;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openBinaryMenuItem;
        private System.Windows.Forms.ToolStripMenuItem findCertificateMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem signToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem aboutMenuItem;
    }
}

