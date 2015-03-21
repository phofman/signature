using System.Windows.Forms;

namespace CodeTitans.Signature
{
    /// <summary>
    /// Helper class to allow accessing files.
    /// </summary>
    static class DialogHelper
    {
        /// <summary>
        /// Creates dialog to point to binary file.
        /// </summary>
        public static OpenFileDialog OpenBinaryFile(string title)
        {
            var openFile = new OpenFileDialog();
            openFile.Title = title;
            openFile.DefaultExt = ".exe";
            openFile.Filter = "Executable|*.dll;*.exe|VSIX Package|*.vsix|All files|*.*";

            openFile.FilterIndex = 0;
            openFile.CheckFileExists = true;
            openFile.CheckPathExists = true;

            return openFile;
        }
        
        /// <summary>
        /// Creates dialog to point to certificate file.
        /// </summary>
        public static OpenFileDialog OpenCertificateFile(string title)
        {
            var openFile = new OpenFileDialog();
            openFile.Title = title;
            openFile.DefaultExt = ".pfx";
            openFile.Filter = "Certificate File|*.pfx|All files|*.*";

            openFile.FilterIndex = 0;
            openFile.CheckFileExists = true;
            openFile.CheckPathExists = true;

            return openFile;
        }
    }
}
