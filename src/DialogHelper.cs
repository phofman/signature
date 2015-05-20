using System.Diagnostics;
using System.IO;
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
            openFile.Filter = "VSIX Package|*.vsix;*.cab;*.msi|Executable|*.dll;*.exe;*.ocx;*.cab;*.msi|Java Package|*xpi;*.jar;*.war;*.ear|All files|*.*";

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

        /// <summary>
        /// Opens Windows Explorer window with specified path.
        /// </summary>
        public static void StartExplorer(string path)
        {
            if (string.IsNullOrEmpty(path))
                return;

            // if specified directory doesn't exist, create it:
            try
            {
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
            }
            catch
            {
            }

            // open Explorer window with this folder:
            Process.Start("Explorer.exe", "/e,\"" + path + "\"");
        }

        /// <summary>
        /// Opens Windows Explorer window with specified file selected.
        /// </summary>
        public static void StartExplorerForFile(string path)
        {
            if (string.IsNullOrEmpty(path))
                return;

            // if file doesn't exits, try to open its parent folder:
            if (!File.Exists(path))
            {
                StartExplorer(Path.GetDirectoryName(path));
                return;
            }

            // open Explorer window with specified file selected:
            Process.Start("Explorer.exe", "/select,\"" + path + "\"");
        }

        /// <summary>
        /// Opens a default web-browser with specified URL.
        /// </summary>
        public static void StartURL(string url)
        {
            if (string.IsNullOrEmpty(url) || !(url.StartsWith("http://") || url.StartsWith("https://")))
                return;

            Process.Start(url);
        }
    }
}
