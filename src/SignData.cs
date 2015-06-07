using System;
using System.Security.Cryptography.X509Certificates;

namespace CodeTitans.Signature
{
    /// <summary>
    /// Arguments about the certificate used to sign data.
    /// </summary>
    sealed class SignData
    {
        private X509Certificate2 _certificate;
        private readonly NamedStore _certificateStore;
        private readonly string _certificatePath;
        private readonly string _certificatePassword;
        private readonly string _timestampServer;
        private readonly NamedHashAlgorithm _hashAlgorithm;

        /// <summary>
        /// Init constructor.
        /// </summary>
        public SignData(X509Certificate2 certificate, NamedStore certificateStore, string certificatePath, string certificatePassword, string timestampServer, NamedHashAlgorithm hashAlgorithm)
        {
            if (certificate == null && string.IsNullOrEmpty(certificatePath))
                throw new ArgumentException("certificate");
            if (certificate != null && certificateStore == null)
                throw new ArgumentNullException("certificateStore");

            _certificate = certificate;
            _certificateStore = certificateStore;
            _certificatePath = certificatePath;
            _certificatePassword = certificatePassword;
            _timestampServer = timestampServer;
            _hashAlgorithm = hashAlgorithm;
        }

        /// <summary>
        /// Ensures certificate is loaded.
        /// </summary>
        public void VerifyCertificate()
        {
            // try to load the certificate:
            if (_certificate == null)
            {
                _certificate = new X509Certificate2(_certificatePath, _certificatePassword);
            }
        }

        #region Properties

        /// <summary>
        /// Gets the certificate's thumbprint.
        /// </summary>
        public string Thumbprint
        {
            get { return _certificate != null ? _certificate.Thumbprint : null; }
        }

        /// <summary>
        /// Gets the info about certificate.
        /// </summary>
        public X509Certificate2 Certificate
        {
            get { return _certificate; }
        }

        public string CertificatePath
        {
            get { return _certificatePath; }
        }

        public string CertificatePassword
        {
            get { return _certificatePassword; }
        }

        public string TimestampServer
        {
            get { return _timestampServer; }
        }

        public NamedHashAlgorithm HashAlgorithm
        {
            get { return _hashAlgorithm; }
        }

        public NamedStore CertificateStore
        {
            get { return _certificateStore; }
        }

        #endregion
    }
}
