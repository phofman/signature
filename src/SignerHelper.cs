using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.IO.Packaging;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Xml;

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

            var extension = Path.GetExtension(binaryPath);
            var outputBuffer = new StringBuilder();
            var errorBuffer = new StringBuilder();
            bool result = false;

            try
            {
                // is it a VSIX package?
                if (string.Compare(extension, ".vsix", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    result = SignVsix(binaryPath, arguments, outputBuffer, errorBuffer, signContentInVsix);
                }
                else
                {
                    // or just a single binary to sign?
                    result = SignBinary(binaryPath, arguments, outputBuffer, errorBuffer);
                }
            }
            catch (Exception ex)
            {
                result = false;
                outputBuffer.AppendLine("Signing internally failed: " + ex.Message);
            }

            if (finishAction != null)
            {
                finishAction(new SignCompletionEventArgs(result, outputBuffer.ToString(), errorBuffer.ToString()));
            }
        }

        private static bool SignVsix(string vsixPackagePath, 
                                     SignData arguments, 
                                     StringBuilder outputBuffer, 
                                     StringBuilder errorBuffer,
                                     bool signContentInVsix = false)
        {
            if (arguments == null)
                throw new ArgumentNullException("arguments");

            // try to load the certificate:
            try
            {
                arguments.VerifyCertificate();
            }
            catch (Exception ex)
            {
                if (errorBuffer != null)
                {
                    errorBuffer.AppendLine("Certificate error.");
                    errorBuffer.AppendLine(ex.Message);
                }
                return false;
            }

            // many thanks to Jeff Wilcox for the idea and code
            // check for details: http://www.jeff.wilcox.name/2010/03/vsixcodesigning/
            using (var package = Package.Open(vsixPackagePath))
            {
                var signatureManager = new PackageDigitalSignatureManager(package);
                signatureManager.CertificateOption = CertificateEmbeddingOption.InSignaturePart;

                // select respective hashing algorithm (http://www.w3.org/TR/2001/WD-xmlenc-core-20010626/):
                if (arguments.HashAlgorithm == null || string.IsNullOrEmpty(arguments.HashAlgorithm.Uri))
                {
                    // fail gracefully:
                    if (errorBuffer != null)
                    {
                        errorBuffer.AppendLine("Unable to sign VSIX with requested '" + (arguments.HashAlgorithm != null ? arguments.HashAlgorithm.Name : "<unknown>") + "' algorithm.");
                    }
                    return false;
                }

                signatureManager.HashAlgorithm = arguments.HashAlgorithm.Uri;

                var partsToSign = new List<Uri>();
                foreach (var packagePart in package.GetParts())
                {
                    if (signContentInVsix)
                    {
                        var fileName = Path.GetFileName(packagePart.Uri.OriginalString);
                        var name = Path.Combine(Path.GetTempPath(), fileName);
                        var extension = Path.GetExtension(name);
                        using (var stream = packagePart.GetStream(FileMode.Open, FileAccess.Read))
                        {
                            using (var fileStream = new FileStream(name, FileMode.Create))
                            {
                                stream.CopyTo(fileStream);
                            }
                        }
                        if ((extension.Equals(".dll") || extension.Equals(".exe")) && !VerifyBinaryDigitalSignature(name))
                        {
                            if (!SignBinary(name, arguments, outputBuffer, errorBuffer))
                            {
                                return false;
                            }

                            using (var stream = packagePart.GetStream(FileMode.Open, FileAccess.Write))
                            {
                                using (var fileStream = new FileStream(name, FileMode.Open))
                                {
                                    fileStream.CopyTo(stream);
                                }
                            }
                        }
                    }
                    
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
                finally
                {
                    signatureManager.HashAlgorithm = PackageDigitalSignatureManager.DefaultHashAlgorithm;
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
