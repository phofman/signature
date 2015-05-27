using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.IO.Packaging;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Forms;

namespace CodeTitans.Signature
{
    /// <summary>
    /// Helper class performing some signing operations.
    /// </summary>
    static class SignerHelper
    {
        private const string SigningVerifiedString = "Successfully verified";
        private const string VerifyDigitalSignatureCmd = " verify /pa \"{0}\"";
        private const string SignBinaryWithPfxCmd = " sign /fd {0} /f {1} /t {2} /p {3} {4}";
        private const string SignBinaryWithCertCmd = " sign /fd {0} /sha1 {1} /n \"{2}\" /t {3} {4}";
        private const string SubjectPrefix = "CN=";
        private static string _signtool = EnsureSignTool();

        /// <summary>
        /// Signs the binary.
        /// </summary>
        public static void Sign(string binaryPath,
                                X509Certificate2 certificate,
                                string certificatePath,
                                string certificatePassword,
                                string timestampServer,
                                string hashAlgorithm,
                                bool signContentInVsix)
        {
            if (string.IsNullOrEmpty(binaryPath))
                throw new ArgumentNullException("binaryPath");
            if (certificate == null && string.IsNullOrEmpty(certificatePath))
                throw new ArgumentException("certificate");

            bool success = false;
            // is it a VSIX package?
            var extension = Path.GetExtension(binaryPath);

            if (certificate == null)
            {
                try
                {
                    certificate = new X509Certificate2(certificatePath, certificatePassword);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Certificate error");
                    return;
                }
            }
            string subject = GetSubjectName(certificate == null ? null : certificate.Subject);
            string thumbPrint = certificate == null ? null : certificate.Thumbprint;

            if (string.Compare(extension, ".vsix", StringComparison.OrdinalIgnoreCase) == 0)
            {
               

                if (signContentInVsix)
                {
                    success = SignVsixContent(binaryPath, subject, thumbPrint, certificatePath, certificatePassword, timestampServer, hashAlgorithm);
                    if (!success)
                    {
                        MessageBox.Show("Signing of binary contained in VSIX failed.", "VSIX signing failed");
                        return;
                    }
                }

                SignVsix(binaryPath, certificate, hashAlgorithm);
                return;
            }

            success = SignBinary(binaryPath, subject, thumbPrint, certificatePath, certificatePassword, timestampServer, hashAlgorithm);
            if (success)
            {
                MessageBox.Show("The signing completed successfully.", "Extension Signing Complete");
            }
            else
            {
                MessageBox.Show("The digital signature is invalid, there may have been a problem with the signing process.", "Invalid Signature");
            }
        }

        private static bool SignVsixContent(string binaryPath,
                                            string subject,
                                            string thumbPrint,
                                            string certificatePath,
                                            string certificatePassword,
                                            string timestampServer,
                                            string hashAlgorithm)
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
                success = SignBinary(file, subject, thumbPrint, certificatePath, certificatePassword, timestampServer, hashAlgorithm);
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

        private static void SignVsix(string vsixPackagePath, X509Certificate2 certificate, string hashAlgorithm)
        {
            // many thanks to Jeff Wilcox for the idea and code
            // check for details: http://www.jeff.wilcox.name/2010/03/vsixcodesigning/
            using (var package = Package.Open(vsixPackagePath))
            {
                var signatureManager = new PackageDigitalSignatureManager(package);
                signatureManager.CertificateOption = CertificateEmbeddingOption.InSignaturePart;

                //if (hashAlgorithm.Equals("SHA256", StringComparison.OrdinalIgnoreCase))
                //{
                //    signatureManager.HashAlgorithm = "http://www.w3.org/2001/04/xmldsig#rsa-sha256";
                //}
                //else if (hashAlgorithm.Equals("SHA512", StringComparison.OrdinalIgnoreCase))
                //{
                //    signatureManager.HashAlgorithm = "http://www.w3.org/2001/04/xmldsig-more#rsa-sha512";
                //}

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
                    MessageBox.Show(ex.ToString(), "Extension Signing Complete");
                    return;
                }

                if (ValidateSignatures(package))
                {
                    MessageBox.Show("The signing completed successfully.", "Extension Signing Complete");
                }
                else
                {
                    MessageBox.Show("The digital signature is invalid, there may have been a problem with the signing process.", "Invalid Signature");
                }
            }
        }

        private static bool SignBinary(string path, 
                                       string subject, 
                                       string thumbPrint, 
                                       string certPath, 
                                       string certPassword, 
                                       string timestampServer, 
                                       string hashAlgorithm)
        {
            string command;
            if (subject != null && thumbPrint != null)
            {
                // " sign /fd {0} /sha1 {1} /n "{2}" /t {3} {4}"
                command = String.Format(SignBinaryWithCertCmd,
                                        hashAlgorithm,
                                        thumbPrint,
                                        subject,
                                        timestampServer,
                                        path);
            }
            else
            {
                // " sign /fd {0} /f {1} /t {2} /p {3} {4}"
                command = String.Format(SignBinaryWithPfxCmd,
                                        hashAlgorithm,
                                        certPath,
                                        timestampServer,
                                        certPassword,
                                        path);
            }
            
            string msg = ExecuteCommand(command);
            return msg.Contains("Successfully signed:");
        }

        private static bool VerifyBinaryDigitalSignature(string path)
        {
            // Verify digital signature
            string command = String.Format(VerifyDigitalSignatureCmd, path);
            string message = ExecuteCommand(command);
            return message.Contains(SigningVerifiedString);
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

        private static string ExecuteCommand(string command)
        {
            if (Signtool == null)
            {
                throw new ArgumentNullException("Cannot find signtool.exe");
            }

            ProcessStartInfo procStartInfo = new ProcessStartInfo(Signtool, command);
            procStartInfo.CreateNoWindow = true;
            procStartInfo.RedirectStandardOutput = true;
            procStartInfo.UseShellExecute = false;

            Process proc = new Process();
            proc.StartInfo = procStartInfo;
            string output = String.Empty;

            proc.Start();
            output = proc.StandardOutput.ReadToEnd();
            if (!proc.HasExited)
            {
                proc.Close();
            }

            return output;
        }

        private static string Signtool
        {
            get
            {
                if (_signtool == null)
                {
                    _signtool = EnsureSignTool();
                }
                return _signtool;
            }
        }

        private static string EnsureSignTool()
        {
            var programFilesX86 = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ProgramFilesX86);

            if (string.IsNullOrEmpty(programFilesX86))
            {
                programFilesX86 = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ProgramFiles);
            }

            string signtool = Path.Combine(programFilesX86, "Windows Kits", "8.1", "bin", "x64", "signtool.exe");
            if (File.Exists(signtool))
            {
                return signtool;
            }

            signtool = Path.Combine(programFilesX86, "Windows Kits", "8.1", "bin", "x86", "signtool.exe");
            if (File.Exists(signtool))
            {
                return signtool;
            }

            signtool = Path.Combine(programFilesX86, "Windows Kits", "8.0", "bin", "x64", "signtool.exe");
            if (File.Exists(signtool))
            {
                return signtool;
            }

            signtool = Path.Combine(programFilesX86, "Windows Kits", "8.0", "bin", "x86", "signtool.exe");
            if (File.Exists(signtool))
            {
                return signtool;
            }

            signtool = Path.Combine(programFilesX86, "Microsoft SDKs", "Windows", "v7.1A", "Bin", "signtool.exe");
            if (File.Exists(signtool))
            {
                return signtool;
            }

            return null;
        }

        private static string GetSubjectName(string subject)
        {
            if (subject == null)
                return null;

            return subject.Substring(subject.IndexOf(SubjectPrefix) + SubjectPrefix.Length);
        }
    }
}
