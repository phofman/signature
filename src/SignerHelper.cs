using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Packaging;
using System.Security.Cryptography.X509Certificates;
using CodeTitans.Signature.Tools;
using System.IO.Compression;

namespace CodeTitans.Signature
{
    /// <summary>
    /// Helper class performing some signing operations.
    /// </summary>
    static class SignerHelper
    {
        /// <summary>
        /// Signs the binary.
        /// </summary>
        public static void Sign(string binaryPath, X509Certificate2 certificate, string certificatePath, string certificatePassword, string timestampServer, Action<ToolRunnerEventArgs> finishAction)
        {
            if (string.IsNullOrEmpty(binaryPath))
                throw new ArgumentNullException("binaryPath");
            if (certificate == null && string.IsNullOrEmpty(certificatePath))
                throw new ArgumentException("certificate");

            // is it a VSIX package?
            var extension = Path.GetExtension(binaryPath);
            if (string.Compare(extension, ".vsix", StringComparison.OrdinalIgnoreCase) == 0)
            {
                if (certificate == null)
                {
                    try
                    {
                        certificate = new X509Certificate2(certificatePath, certificatePassword);
                    }
                    catch (Exception ex)
                    {
                        if (finishAction != null)
                            finishAction(new ToolRunnerEventArgs(-1, null, ex.Message));
                        return;
                    }
                }

                SignVsixContent(binaryPath, certificate);
                SignVsix(binaryPath, certificate, finishAction);
                return;
            }

            SignBinary(binaryPath, certificate != null ? certificate.Thumbprint : null, certificatePath, certificatePassword, timestampServer, finishAction);
        }

        private static void SignVsixContent(string binaryPath, X509Certificate2 certificate)
        {
            // Step 1: rename vsix to zip
            string zipPackagePath = RenameExtention(binaryPath, ".zip");

            // Step 2: extract files and delete the zip file
            string fileDir = Path.GetDirectoryName(binaryPath);
            string fileName = Path.GetFileNameWithoutExtension(binaryPath);
            string unZippedDir = Path.Combine(fileDir, fileName);
            ZipFile.ExtractToDirectory(zipPackagePath, unZippedDir);
            File.Delete(zipPackagePath);

            // Step 3: sign all extracted files

            // Step 4: Zip the extracted files
            ZipFile.CreateFromDirectory(unZippedDir, zipPackagePath, CompressionLevel.NoCompression, false);

            // Step 5: Rename zip file to vsix
            string vsixPackagePath = RenameExtention(zipPackagePath, ".vsix");
        }

        private static void SignVsix(string vsixPackagePath, X509Certificate2 certificate, Action<ToolRunnerEventArgs> finishAction)
        {
            // many thanks to Jeff Wilcox for the idea and code
            // check for details: http://www.jeff.wilcox.name/2010/03/vsixcodesigning/
            using (var package = Package.Open(vsixPackagePath))
            {
                var signatureManager = new PackageDigitalSignatureManager(package);
                signatureManager.CertificateOption = CertificateEmbeddingOption.InSignaturePart;

                var partsToSign = new List<Uri>();
                foreach (var packagePart in package.GetParts())
                {
                    partsToSign.Add(packagePart.Uri);
                }

                partsToSign.Add(PackUriHelper.GetRelationshipPartUri(signatureManager.SignatureOrigin));
                partsToSign.Add(signatureManager.SignatureOrigin);
                partsToSign.Add(PackUriHelper.GetRelationshipPartUri(new Uri("/", UriKind.RelativeOrAbsolute)));

                try
                {
                    signatureManager.Sign(partsToSign, certificate);
                }
                catch (System.Security.Cryptography.CryptographicException ex)
                {
                    if (finishAction != null)
                        finishAction(new ToolRunnerEventArgs(-2, null, "Signing could not be completed: " + ex.Message));
                    return;
                }

                if (ValidateSignatures(package))
                {
                    System.Windows.Forms.MessageBox.Show("The signing completed successfully.", "Extension Signing Complete");
                }
                else
                {
                    System.Windows.Forms.MessageBox.Show("The digital signature is invalid, there may have been a problem with the signing process.", "Invalid Signature");
                }

                if (finishAction != null)
                    finishAction(new ToolRunnerEventArgs(0, "Signing completed", null));
            }
        }

        private static void SignBinary(string binaryPath, string certificateThumbprint, string certificatePath, string certificatePassword, string timestampServer, Action<ToolRunnerEventArgs> finishAction)
        {
            var runner = new SignToolRunner(binaryPath, certificateThumbprint, certificatePath, certificatePassword, timestampServer);

            runner.Finished += (sender, e) =>
            {
                if (finishAction != null)
                {
                    finishAction(e);
                }
            };

            runner.ExecuteAsync();
        }

        private static bool ValidateSignatures(Package package)
        {
            var signatureManager = new PackageDigitalSignatureManager(package);
            bool isSigned = signatureManager.IsSigned;
            var verifyResult = signatureManager.VerifySignatures(true);
            return isSigned && verifyResult == VerifyResult.Success;
        }

        private static string RenameExtention(string filePath, string toExtension)
        {
            string fileDir = Path.GetDirectoryName(filePath);
            string fileName = Path.GetFileNameWithoutExtension(filePath);

            string newFilePath = Path.Combine(fileDir, String.Format("{0}{1}", fileName, toExtension));
            
            File.Move(filePath, newFilePath);
            return newFilePath;
        }

        private static void UnZipFile(string filePath, string dstFolder)
        {
            ZipFile.ExtractToDirectory(filePath, dstFolder);
        }
    }
}
