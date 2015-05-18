using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Forms;
using CodeTitans.Signature.Tools;

namespace CodeTitans.Signature
{
    public partial class MainForm : Form
    {
        private const string AppTitle = "CodeTitans Signature";

        private OpenFileDialog _openBinaryDialog;
        private OpenFileDialog _openCertDialog;
        private bool _signContentInVsix;

        public MainForm()
        {
            InitializeComponent();

            radioInstalled.Checked = true;
            txtCertificateFilter.Text = "Open Source Developer";

            FillTimestampServers();

            // allow file drops on that application:
            AllowDrop = true;
            DragEnter += OnFileDropEnter;
            DragDrop += OnFileDropDone;
        }

        private void OnFileDropDone(object sender, DragEventArgs e)
        {
            string[] filePaths = e.Data.GetData(DataFormats.FileDrop) as string[];

            if (filePaths != null)
            {
                foreach (var name in filePaths)
                {
                    if (string.IsNullOrEmpty(name))
                        continue;

                    if (name.EndsWith(".pfx", StringComparison.OrdinalIgnoreCase))
                        SetCertificateFromFilePath(name);
                    else
                        SetBinaryFromFilePath(name);
                }
            }
        }

        private void SetCertificateFromFilePath(string name)
        {
            radioPfx.Checked = true;
            txtCertificatePath.Text = name;
            ActiveControl = txtCertificatePassword;
        }

        private void SetBinaryFromFilePath(string name)
        {
            txtBinaryPath.Text = name;
        }

        private void OnFileDropEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
        }

        private bool ShowOpenResult
        {
            get { return bttOpenResult.Visible; }
            set
            {
                bttOpenResult.Visible = value;
                openResultFolderMenuItem.Visible = value;
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void FillCertificates(string subjectName)
        {
            cmbCertificates.Items.Clear();

            IEnumerable<X509Certificate2> certificates = null;
            try
            {
                certificates = CertificateHelper.LoadUserCertificates(subjectName);
            }
            catch (Exception ex)
            {
                cmbCertificates.Items.Add(new ComboBoxItem(null, ex.Message));
            }

            if (certificates != null)
            {
                foreach (var cert in certificates)
                    cmbCertificates.Items.Add(new ComboBoxItem(cert, cert.SubjectName.Name));

                if (cmbCertificates.Items.Count > 0)
                {
                    cmbCertificates.SelectedIndex = 0;
                }
            }
        }

        private void FillTimestampServers()
        {
            cmbTimestampServers.Items.Clear();
            foreach (var server in CertificateHelper.LoadTimestampServers())
            {
                cmbTimestampServers.Items.Add(new ComboBoxItem(server, server));
            }
            cmbTimestampServers.SelectedIndex = 0;
        }

        private void OnCertificateFilterChanged(object sender, EventArgs e)
        {
            FillCertificates(txtCertificateFilter.Text);
        }

        private void OnCertificateCheckedChanged(object sender, EventArgs e)
        {
            bool enabled = radioInstalled.Checked;

            txtCertificateFilter.Enabled = enabled;
            cmbCertificates.Enabled = enabled;
            txtCertificatePath.Enabled = !enabled;
            txtCertificatePassword.Enabled = !enabled;
            btnCertificateLocation.Enabled = !enabled;
            findCertificateMenuItem.Enabled = !enabled;

            if (ActiveControl == radioInstalled || ActiveControl == radioPfx)
            {
                if (enabled)
                {
                    ActiveControl = txtCertificateFilter;
                }
                else
                {
                    ActiveControl = txtCertificatePath;
                }
            }
        }

        private void btnBinaryLocation_Click(object sender, EventArgs e)
        {
            if (_openBinaryDialog == null)
                _openBinaryDialog = DialogHelper.OpenBinaryFile("Binary Files");

            if (_openBinaryDialog.ShowDialog() == DialogResult.OK)
            {
                txtBinaryPath.Text = _openBinaryDialog.FileName;
                ShowOpenResult = false;
            }
        }

        private void btnCertificateLocation_Click(object sender, EventArgs e)
        {
            if (_openCertDialog == null)
                _openCertDialog = DialogHelper.OpenCertificateFile("Certificate Files");

            if (_openCertDialog.ShowDialog() == DialogResult.OK)
            {
                txtCertificatePath.Text = _openCertDialog.FileName;
            }
        }

        private void btnSign_Click(object sender, EventArgs e)
        {
            ShowOpenResult = false;

            if (string.IsNullOrEmpty(txtBinaryPath.Text) || !File.Exists(txtBinaryPath.Text))
            {
                MessageBox.Show("You must specify valid binary to sign", AppTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
                ActiveControl = txtBinaryPath;
                return;
            }

            if (txtCertificatePath.Enabled && (string.IsNullOrEmpty(txtCertificatePath.Text) || !File.Exists(txtCertificatePath.Text)))
            {
                MessageBox.Show("You must specify valid certificate PFX file", AppTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
                ActiveControl = txtCertificatePath;
                return;
            }

            var certificate = cmbCertificates.SelectedItem != null ? ((ComboBoxItem) cmbCertificates.SelectedItem).Data as X509Certificate2 : null;
            if (cmbCertificates.Enabled && certificate == null)
            {
                MessageBox.Show("You must select a valid certificate", AppTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
                ActiveControl = cmbCertificates;
                return;
            }

            var timestampServer = cmbTimestampServers.SelectedItem != null ? ((ComboBoxItem) cmbTimestampServers.SelectedItem).Data as string : null;
            if (cmbCertificates.Enabled)
            {
                SignerHelper.Sign(txtBinaryPath.Text, certificate, null, null, timestampServer, OnFinished);
            }
            else
            {
                SignerHelper.Sign(txtBinaryPath.Text, null, txtCertificatePath.Text, txtCertificatePassword.Text, timestampServer, OnFinished);
            }

            ShowOpenResult = true;
        }

        private void OnFinished(ToolRunnerEventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<ToolRunnerEventArgs>(OnFinished), e);
                return;
            }

            txtLog.Text = string.Concat(e.Output, string.IsNullOrEmpty(e.Output) ? string.Empty : Environment.NewLine, e.Error);
        }

        private void homeLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            DialogHelper.StartURL("http://www.codetitans.pl");
        }

        private void aboutMenuItem_Click(object sender, EventArgs e)
        {
            var aboutForm = new AboutForm();
            aboutForm.ShowDialog();
        }

        private void bttOpenResult_Click(object sender, EventArgs e)
        {
            DialogHelper.StartExplorerForFile(txtBinaryPath.Text);
        }

        private void signContentInVsix_CheckedChanged(object sender, EventArgs e)
        {
            _signContentInVsix = this.signContentInVsix.Checked;
        }

        private void txtBinaryPath_TextChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(this.txtBinaryPath.Text) && this.txtBinaryPath.Text.TrimEnd().EndsWith(".vsix", StringComparison.OrdinalIgnoreCase))
            {
                this.signContentInVsix.Enabled = true;
            }
            else
            {
                this.signContentInVsix.Enabled = false;
            }
        }
    }
}
