using System.Reflection;
using System.Windows.Forms;

namespace CodeTitans.Signature
{
    public partial class AboutForm : Form
    {
        public AboutForm()
        {
            InitializeComponent();

            lblVersion.Text = "v" + GetVersion();
        }

        private string GetVersion()
        {
            var assembly = Assembly.GetExecutingAssembly();
            return assembly.GetName().Version.ToString();
        }

        private void homeLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            DialogHelper.StartURL("http://www.codetitans.pl");
        }

        private void twitterLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            DialogHelper.StartURL("http://twitter.com/CodeTitans");
        }

        private void projectLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            DialogHelper.StartURL("https://github.com/phofman/signature");
        }

        private void issuesLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            DialogHelper.StartURL("https://github.com/phofman/signature/issues");
        }
    }
}
