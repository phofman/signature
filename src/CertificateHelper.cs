using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Forms;

namespace CodeTitans.Signature
{
    /// <summary>
    /// Helper class providing info about certificates.
    /// </summary>
    static class CertificateHelper
    {
        /// <summary>
        /// Loads all certificates that belong to current user.
        /// </summary>
        public static IEnumerable<X509Certificate2> LoadUserCertificates(NamedStore store, string subjectName)
        {
            var certStore = store == null ? new X509Store(StoreName.My, StoreLocation.CurrentUser) : new X509Store(store.Name, store.Location);
            try
            {
                certStore.Open(OpenFlags.ReadOnly);

                var result = new List<X509Certificate2>();
                var certificates = string.IsNullOrEmpty(subjectName) ? certStore.Certificates : certStore.Certificates.Find(X509FindType.FindBySubjectName, subjectName, true);
                foreach (var cert in certificates)
                {
                    result.Add(cert);
                }
                return result;
            }
            finally
            {
                certStore.Close();
            }
        }

        /// <summary>
        /// Gets the list of timestamp servers.
        /// </summary>
        public static IEnumerable<string> LoadTimestampServers()
        {
            return new[]
            {
                "http://time.certum.pl",
                "http://timestamp.verisign.com/scripts/timstamp.dll",
                "http://timestamp.comodoca.com/authenticode"
            };
        }

        /// <summary>
        /// Gets the list of names for available hash algorithms.
        /// </summary>
        public static IEnumerable<NamedHashAlgorithm> LoadHashAlgorithms()
        {
            return new[]
            {
                new NamedHashAlgorithm("SHA1", "http://www.w3.org/2000/09/xmldsig#sha1"),
                new NamedHashAlgorithm("SHA256", "http://www.w3.org/2001/04/xmlenc#sha256"),
                new NamedHashAlgorithm("SHA384", "http://www.w3.org/2001/04/xmldsig-more#sha384"),
                new NamedHashAlgorithm("SHA512", "http://www.w3.org/2001/04/xmlenc#sha512")
            };
        }

        /// <summary>
        /// Gets the list of stores, where to search for installed certificate.
        /// </summary>
        public static IEnumerable<NamedStore> LoadNamedCertificateScores()
        {
            return new[]
            {
                new NamedStore(StoreName.My),
                new NamedStore(StoreName.Root),
                new NamedStore(StoreName.AuthRoot),
                new NamedStore(StoreName.AddressBook),
                new NamedStore(StoreName.CertificateAuthority),
                new NamedStore(StoreName.TrustedPeople),
                new NamedStore(StoreName.TrustedPublisher),

                new NamedStore(StoreName.My, StoreLocation.LocalMachine),
                new NamedStore(StoreName.Root, StoreLocation.LocalMachine),
                new NamedStore(StoreName.AuthRoot, StoreLocation.LocalMachine),
                new NamedStore(StoreName.AddressBook, StoreLocation.LocalMachine),
                new NamedStore(StoreName.CertificateAuthority, StoreLocation.LocalMachine),
                new NamedStore(StoreName.TrustedPeople, StoreLocation.LocalMachine),
                new NamedStore(StoreName.TrustedPublisher, StoreLocation.LocalMachine),
            };
        }
    }
}
