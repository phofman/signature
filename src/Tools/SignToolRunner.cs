using System.IO;
using System.Text;

namespace CodeTitans.Signature.Tools
{
    /// <summary>
    /// Runner that handles execution of the SignTool.exe
    /// </summary>
    sealed class SignToolRunner : ToolRunner
    {
        private string _binaryPath;
        private string _certThumbprint;
        private string _certPath;
        private string _certPassword;
        private string _timestampServer;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public SignToolRunner()
            : base("cmd.exe", null)
        {
            UpdateArguments();
        }

        /// <summary>
        ///  Init constructor.
        /// </summary>
        public SignToolRunner(string binaryPath, string certificateThumbprint, string certificatePath, string certificatePassword, string timestampServer)
            : base("cmd.exe", null)
        {
            _binaryPath = binaryPath;
            _certThumbprint = certificateThumbprint;
            _certPath = certificatePath;
            _certPassword = certificatePassword;
            _timestampServer = timestampServer;

            UpdateArguments();
        }

        #region Properties

        /// <summary>
        /// Gets or sets the path to the binary to sign.
        /// </summary>
        public string BinaryPath
        {
            get { return _binaryPath; }
            set
            {
                _binaryPath = value;
                UpdateArguments();
            }
        }

        /// <summary>
        /// Gets or sets the certificate's SHA1 thumbprint for the installed certificate to use.
        /// Optional and overrides usage of CertificatePath and CertificatePassword.
        /// </summary>
        public string CertificateThumbprint
        {
            get { return _certThumbprint; }
            set
            {
                _certThumbprint = value;
                UpdateArguments();
            }
        }

        /// <summary>
        /// Gets or sets the path to the PFX file containing the certificate to sign the binary.
        /// </summary>
        public string CertificatePath
        {
            get { return _certPath; }
            set
            {
                _certPath = value;
                UpdateArguments();
            }
        }

        /// <summary>
        /// Gets or sets the certificate's password. Used only in conjuntion with PFX path.
        /// </summary>
        public string CertificatePassword
        {
            get { return _certPassword; }
            set
            {
                _certPassword = value;
                UpdateArguments();
            }
        }

        /// <summary>
        /// Gets or sets the URI to the timestamp server.
        /// </summary>
        public string TimestampServer
        {
            get { return _timestampServer; }
            set
            {
                _timestampServer = value;
                UpdateArguments();
            }
        }

        #endregion

        private void UpdateArguments()
        {
            var argsBuilder = new StringBuilder("/C signtool.exe ");

            // do we only specify timestamp?
            if (string.IsNullOrEmpty(CertificateThumbprint) && string.IsNullOrEmpty(CertificatePath) && string.IsNullOrEmpty(CertificatePassword))
            {
                argsBuilder.Append("timestamp ");
            }
            else
            {
                argsBuilder.Append("sign ");
                if (!string.IsNullOrEmpty(CertificateThumbprint))
                {
                    argsBuilder.Append("/sha1 ");
                    argsBuilder.Append('"').Append(CertificateThumbprint).Append('"').Append(' ');
                }
                else
                {
                    if (!string.IsNullOrEmpty(CertificatePath))
                    {
                        argsBuilder.Append("/f ");
                        argsBuilder.Append('"').Append(CertificatePath).Append('"').Append(' ');
                    }
                    if (!string.IsNullOrEmpty(CertificatePassword))
                    {
                        argsBuilder.Append("/p ");
                        argsBuilder.Append('"').Append(CertificatePassword).Append('"').Append(' ');
                    }
                }
            }

            if (!string.IsNullOrEmpty(TimestampServer))
            {
                argsBuilder.Append("/t ");
                argsBuilder.Append('"').Append(TimestampServer).Append('"').Append(' ');
            }

            if (!string.IsNullOrEmpty(BinaryPath))
            {
                argsBuilder.Append('"').Append(BinaryPath).Append('"').Append(' ');
            }

            Arguments = argsBuilder.ToString();
        }

        protected override void PrepareStartup()
        {
            base.PrepareStartup();

            var programFilesX86 = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ProgramFilesX86);

            if (string.IsNullOrEmpty(programFilesX86))
            {
                programFilesX86 = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ProgramFiles);
            }

            // make sure signtool.exe can be found:
            Environment["PATH"] = string.Concat(
                                         Path.Combine(programFilesX86, "Windows Kits", "8.0", "bin", "x64"),
                                    ";", Path.Combine(programFilesX86, "Windows Kits", "8.0", "bin", "x86"),
                                    ";", Path.Combine(programFilesX86, "Microsoft SDKs", "Windows", "v7.0A", "Bin"),
                                    ";", Environment["PATH"]);

            ProcessOutputLine(string.Concat(FileName, " ", Arguments, System.Environment.NewLine));
        }
    }
}
