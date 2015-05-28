using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.IO.Packaging;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace CodeTitans.Signature
{
    /// <summary>
    /// Helper class performing some signing operations.
    /// </summary>
    static class SignerHelper
    {
        private static StringBuilder _output = new StringBuilder();
        private static StringBuilder _error = new StringBuilder();

        /// <summary>
        /// Signs the binary.
        /// </summary>
        public static void Sign(string binaryPath, SignData arguments, bool signContentInVsix, Action<SignCompletionEventArgs> finishAction)
        {
            if (string.IsNullOrEmpty(binaryPath))
                throw new ArgumentNullException("binaryPath");
            if (arguments == null)
                throw new ArgumentNullException("arguments");

            // try to load the certificate:
            try
            {
                arguments.VerifyCertificate();
            }
            catch (Exception ex)
            {
                if (finishAction != null)
                {
                    finishAction(new SignCompletionEventArgs(false, "Certificate error.", ex.Message));
                }
                return;
            }

            // is it a VSIX package?
            var extension = Path.GetExtension(binaryPath);

            if (string.Compare(extension, ".vsix", StringComparison.OrdinalIgnoreCase) == 0)
            {
                if (signContentInVsix)
                {
                    var result2 = SignVsixContent(binaryPath, arguments);
                    if (!result2)
                    {
                        if (finishAction != null)
                        {
                            finishAction(new SignCompletionEventArgs(false, "Signing of binary contained in VSIX failed.", _error.ToString()));
                        }
                        return;
                    }
                }

                SignVsix(binaryPath, arguments, finishAction);
                return;
            }

            // or just a single binary to sign?
            var result = SignBinary(binaryPath, arguments);
            if (result)
            {
                if (finishAction != null)
                {
                    finishAction(new SignCompletionEventArgs(true, _output.ToString(), null));
                }
            }
            else
            {
                if (finishAction != null)
                {
                    finishAction(new SignCompletionEventArgs(false, "Signing of binary failed.", _error.ToString()));
                }
            }
        }

        private static bool SignVsixContent(string binaryPath, SignData arguments)
        {
            bool success = true;

            // Step 1: rename vsix to zip
            string zipPackagePath = RenameExtention(binaryPath, ".zip");

            // Step 2: extract files and delete the zip file
            string fileDir = Path.GetDirectoryName(binaryPath);
            string fileName = Path.GetFileNameWithoutExtension(binaryPath);
            string unZippedDir = Path.Combine(fileDir, fileName);
            ZipFile.ExtractToDirectory(zipPackagePath, unZippedDir);
            File.Delete(zipPackagePath);

            // Step 3: sign all extracted files
            var filesToSign = Directory.GetFiles(unZippedDir, "*.dll").
                                Concat(Directory.GetFiles(unZippedDir, "*.exe")).
                                Where(f => !VerifyBinaryDigitalSignature(f)).ToArray();
            foreach (var file in filesToSign)
            {
                success = SignBinary(file, arguments);
                if (!success)
                {
                    break;
                }
            }

            // Step 4: Zip the extracted files
            ZipFile.CreateFromDirectory(unZippedDir, zipPackagePath, CompressionLevel.NoCompression, false);
            Directory.Delete(unZippedDir, true);

            // Step 5: Rename zip file to vsix
            string vsixPackagePath = RenameExtention(zipPackagePath, ".vsix");
            return success;
        }

        private static void SignVsix(string vsixPackagePath, SignData arguments, Action<SignCompletionEventArgs> finishAction)
        {
            if (arguments == null)
                throw new ArgumentNullException("arguments");

            // many thanks to Jeff Wilcox for the idea and code
            // check for details: http://www.jeff.wilcox.name/2010/03/vsixcodesigning/
            using (var package = Package.Open(vsixPackagePath))
            {
                var signatureManager = new PackageDigitalSignatureManager(package);
                signatureManager.CertificateOption = CertificateEmbeddingOption.InSignaturePart;

                // TODO: support signing VSIX with digest algorithm set to SHA256 

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
                    signatureManager.Sign(partsToSign, arguments.Certificate);
                }
                catch (CryptographicException ex)
                {
                    if (finishAction != null)
                        finishAction(new SignCompletionEventArgs(false, null, "Signing could not be completed: " + ex.Message));
                    return;
                }

                if (ValidateSignatures(package))
                {
                    _output.AppendLine("VSIX signing completed successfully.");
                    if (finishAction != null)
                        finishAction(new SignCompletionEventArgs(true, _output.ToString(), null));
                }
                else
                {
                    if (finishAction != null)
                        finishAction(new SignCompletionEventArgs(false, "The digital signature is invalid, there may have been a problem with the signing process.", _error.ToString()));
                }
            }
        }

        private static bool SignBinary(string path, SignData arguments)
        {
            if (arguments == null)
                throw new ArgumentNullException("arguments");

            string output;
            string error;
            int exitCode = SignToolRunner.ExecuteCommand(path, arguments, out output, out error);
            if (!String.IsNullOrEmpty(output))
            {
                _output.AppendLine(output);
            }
            
            if (!String.IsNullOrEmpty(error))
            {
                _error.AppendLine(error);
            }            
            return exitCode == 0;
        }

        private static bool VerifyBinaryDigitalSignature(string path)
        {
            // Verify digital signature
            string output;
            string error;
            int exitCode = SignToolRunner.ExecuteSignatureVerification(path, out output, out error);
            return exitCode == 0;
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
    }
}
