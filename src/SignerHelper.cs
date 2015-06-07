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

        private static bool SignVsixContent(string packagePath, SignData arguments, StringBuilder outputBuffer, StringBuilder errorBuffer)
        {
            bool success = true;

            // Step 1: rename vsix to zip
            string zipPackagePath = Path.ChangeExtension(packagePath, ".zip");
            if (File.Exists(zipPackagePath))
            {
                File.Delete(zipPackagePath);
            }
            File.Move(packagePath, zipPackagePath);

            // Step 2: extract files and delete the zip file
            string fileDir = Path.GetDirectoryName(packagePath);
            string fileName = Path.GetFileNameWithoutExtension(packagePath);
            string unZippedDir = Path.Combine(fileDir, fileName);
            if (Directory.Exists(unZippedDir))
            {
                Directory.Delete(unZippedDir, true);
            }
            ZipFile.ExtractToDirectory(zipPackagePath, unZippedDir);
            File.Delete(zipPackagePath);

            // Step 3: sign all extracted files
            var filesToSign = Directory.GetFiles(unZippedDir, "*.dll", SearchOption.AllDirectories).
                                Concat(Directory.GetFiles(unZippedDir, "*.exe", SearchOption.AllDirectories)).
                                Where(f => !VerifyBinaryDigitalSignature(f)).ToArray();
            foreach (var file in filesToSign)
            {
                success = SignBinary(file, arguments, outputBuffer, errorBuffer);
                if (!success)
                {
                    break;
                }
            }

            // Step 4: Read info about MIME types and delete the metadata
            var contentTypesPath = Path.Combine(unZippedDir, "[Content_Types].xml");
            var registeredExtensions = ReadContentTypesXml(contentTypesPath);
            File.Delete(contentTypesPath);

            // Step 5: Create the VSIX package again
            Package finalPackage = null;
            try
            {
                finalPackage = Package.Open(zipPackagePath, FileMode.Create);
                var allFiles = Directory.GetFiles(unZippedDir, "*", SearchOption.AllDirectories);

                foreach (var file in allFiles)
                {
                    var uri = PackUriHelper.CreatePartUri(new Uri(file.Substring(unZippedDir.Length + 1), UriKind.Relative));
                    var part = finalPackage.CreatePart(uri, GetMimeType(registeredExtensions, uri.OriginalString), CompressionOption.Maximum);

                    if (part != null)
                    {
                        using (var sourceStream = File.OpenRead(file))
                        {
                            using (var outputStream = part.GetStream(FileMode.Create))
                            {
                                //sourceStream.CopyTo(outputStream);
                                CopyStream(sourceStream, outputStream);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (errorBuffer != null)
                    errorBuffer.AppendLine(ex.Message).AppendLine(ex.StackTrace);

                success = false;
            }
            finally
            {
                if (finalPackage != null)
                    finalPackage.Close();
            }

            // Step 6: Rename zip file to vsix
            string vsixPackagePath = Path.ChangeExtension(zipPackagePath, ".vsix");
            File.Move(zipPackagePath, vsixPackagePath);
            return success;
        }

        private static void CopyStream(Stream source, Stream output)
        {
            byte[] buffer = new byte[16 * 1024];
            int bytesRead;

            while ((bytesRead = source.Read(buffer, 0, buffer.Length)) > 0)
            {
                output.Write(buffer, 0, bytesRead);
            }
        }

        private static string GetMimeType(Dictionary<string, string> registeredExtensions, string path)
        {
            // INFO: all extensions or parts belong to this dictionary as we rewrite exactly the same package as original one
            // we could be in trouble if adding new type of files into it...
            var ext = Path.GetExtension(path);
            return string.IsNullOrEmpty(ext) ? registeredExtensions[path] : registeredExtensions[ext];
        }

        private static Dictionary<string, string> ReadContentTypesXml(string path)
        {
            var result = new Dictionary<string, string>();

            if (File.Exists(path))
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(path);

                // get extensions descriptions:
                foreach (XmlElement node in doc.GetElementsByTagName("Default"))
                {
                    var ext = "." + node.GetAttribute("Extension");
                    var type = node.GetAttribute("ContentType");
                    result[ext] = type;
                }

                // get unrecognized paths descriptions:
                foreach (XmlElement node in doc.GetElementsByTagName("Override"))
                {
                    var part = node.GetAttribute("PartName");
                    var type = node.GetAttribute("ContentType");
                    result[part] = type;
                }
            }

            return result;
        }

        private static bool SignVsix(string vsixPackagePath, SignData arguments, StringBuilder outputBuffer, StringBuilder errorBuffer)
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
