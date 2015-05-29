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

            var extension = Path.GetExtension(binaryPath);
            var outputBuffer = new StringBuilder();
            var errorBuffer = new StringBuilder();
            bool result = false;

            // is it a VSIX package?
            if (string.Compare(extension, ".vsix", StringComparison.OrdinalIgnoreCase) == 0)
            {
                if (signContentInVsix)
                {
                    result = SignVsixContent(binaryPath, arguments, outputBuffer, errorBuffer);
                }

                if (!signContentInVsix || result)
                {
                    result = SignVsix(binaryPath, arguments, outputBuffer, errorBuffer);
                }
            }
            else
            {
                // or just a single binary to sign?
                result = SignBinary(binaryPath, arguments, outputBuffer, errorBuffer);
            }

            if (finishAction != null)
            {
                finishAction(new SignCompletionEventArgs(result, outputBuffer.ToString(), errorBuffer.ToString()));
            }
        }

        private static bool SignVsixContent(string binaryPath, SignData arguments, StringBuilder outputBuffer, StringBuilder errorBuffer)
        {
            bool success = true;

            // Step 1: rename vsix to zip
            string zipPackagePath = Path.ChangeExtension(binaryPath, ".zip");
            File.Move(binaryPath, zipPackagePath);

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
                success = SignBinary(file, arguments, outputBuffer, errorBuffer);
                if (!success)
                {
                    break;
                }
            }

            // Step 4: Zip the extracted files
            ZipFile.CreateFromDirectory(unZippedDir, zipPackagePath, CompressionLevel.Optimal, false);
            Directory.Delete(unZippedDir, true);

            // Step 5: Rename zip file to vsix
            string vsixPackagePath = Path.ChangeExtension(zipPackagePath, ".vsix");
            File.Move(zipPackagePath, vsixPackagePath);
            return success;
        }

        private static bool SignVsix(string vsixPackagePath, SignData arguments, StringBuilder outputBuffer, StringBuilder errorBuffer)
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
                    if (errorBuffer != null)
                    {
                        errorBuffer.AppendLine("Signing could not be completed: " + ex.Message);
                    }
                    return false;
                }

                if (ValidateSignatures(package))
                {
                    if (outputBuffer != null)
                    {
                        outputBuffer.AppendLine("VSIX signing completed successfully.");
                    }
                    return true;
                }

                if (outputBuffer != null)
                {
                    outputBuffer.AppendLine("The digital signature is invalid, there may have been a problem with the signing process.");
                }

                return false;
            }
        }

        private static bool SignBinary(string path, SignData arguments, StringBuilder outputBuffer, StringBuilder errorBuffer)
        {
            if (arguments == null)
                throw new ArgumentNullException("arguments");

            string output;
            string error;
            int exitCode = SignToolRunner.ExecuteCommand(path, arguments, out output, out error);

            if (!string.IsNullOrEmpty(output) && outputBuffer != null)
            {
                outputBuffer.AppendLine(output);
            }
            
            if (!string.IsNullOrEmpty(error) && errorBuffer != null)
            {
                errorBuffer.AppendLine(error);
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
    }
}
