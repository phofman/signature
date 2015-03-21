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
        public MainForm()
        {
            InitializeComponent();

            radioInstalled.Checked = true;
            txtCertificateFilter.Text = "Open Source Developer";

            FillTimestampServers();
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
            var dialog = DialogHelper.OpenBinaryFile("Binary Files");
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                txtBinaryPath.Text = dialog.FileName;
            }
        }

        private void btnCertificateLocation_Click(object sender, EventArgs e)
        {
            var dialog = DialogHelper.OpenCertificateFile("Certificate Files");
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                txtCertificatePath.Text = dialog.FileName;
            }
        }

        private void btnSign_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtBinaryPath.Text) || !File.Exists(txtBinaryPath.Text))
            {
                MessageBox.Show("You must specify valid binary to sign");
                ActiveControl = txtBinaryPath;
                return;
            }

            if (txtCertificatePath.Enabled && (string.IsNullOrEmpty(txtCertificatePath.Text) || !File.Exists(txtCertificatePath.Text)))
            {
                MessageBox.Show("You must specify valid certificate PFX file");
                ActiveControl = txtCertificatePath;
                return;
            }

            var certificate = cmbCertificates.SelectedItem != null ? ((ComboBoxItem) cmbCertificates.SelectedItem).Data as X509Certificate2 : null;
            if (cmbCertificates.Enabled && certificate == null)
            {
                MessageBox.Show("You must select a valid certificate");
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
    }
}
