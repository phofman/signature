using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.IO.Packaging;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace CodeTitans.Signature
{
    /// <summary>
    /// Helper class performing some signing operations.
    /// </summary>
    static class SignerHelper
    {
        private const string VerifyDigitalSignatureCmd = "verify /pa \"{0}\"";
        private const string SignBinaryWithPfxCmd = "sign /fd \"{0}\" /f \"{1}\" /t \"{2}\" /p \"{3}\" \"{4}\"";
        private const string SignBinaryWithCertCmd = "sign /fd \"{0}\" /sha1 \"{1}\" /a /t \"{2}\" \"{3}\"";
        private static string _signtool = EnsureSignTool();
        private static StringBuilder _output = new StringBuilder();
        private static StringBuilder _error = new StringBuilder();

        /// <summary>
        /// Signs the binary.
        /// </summary>
        public static void Sign(string binaryPath,
                                X509Certificate2 certificate,
                                string certificatePath,
                                string certificatePassword,
                                string timestampServer,
                                string hashAlgorithm,
                                bool signContentInVsix,
                                Action<SignEventArgs> finishAction)
        {
            if (string.IsNullOrEmpty(binaryPath))
                throw new ArgumentNullException("binaryPath");
            if (certificate == null && string.IsNullOrEmpty(certificatePath))
                throw new ArgumentException("certificate");

            // try to load the certificate:
            if (certificate == null)
            {
                try
                {
                    certificate = new X509Certificate2(certificatePath, certificatePassword);
                }
                catch (Exception ex)
                {
                    if (finishAction != null)
                    {
                        finishAction(new SignEventArgs(false, "Certificate error.", ex.Message));
                    }
                    return;
                }
            }

            // is it a VSIX package?
            var extension = Path.GetExtension(binaryPath);
            string thumbPrint = certificate.Thumbprint;

            if (string.Compare(extension, ".vsix", StringComparison.OrdinalIgnoreCase) == 0)
            {
                if (signContentInVsix)
                {
                    var result2 = SignVsixContent(binaryPath, thumbPrint, certificatePath, certificatePassword, timestampServer, hashAlgorithm);
                    if (!result2)
                    {
                        if (finishAction != null)
                        {
                            finishAction(new SignEventArgs(false, "Signing of binary contained in VSIX failed.", _error.ToString()));
                        }
                        return;
                    }
                }

                SignVsix(binaryPath, certificate, hashAlgorithm, finishAction);
                return;
            }

            // or just a single binary to sign?
            var result = SignBinary(binaryPath, thumbPrint, certificatePath, certificatePassword, timestampServer, hashAlgorithm);
            if (result)
            {
                if (finishAction != null)
                {
                    finishAction(new SignEventArgs(true, _output.ToString(), null));
                }
            }
            else
            {
                if (finishAction != null)
                {
                    finishAction(new SignEventArgs(false, "Signing of binary failed.", _error.ToString()));
                }
            }
        }

        private static bool SignVsixContent(string binaryPath,
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
                success = SignBinary(file, thumbPrint, certificatePath, certificatePassword, timestampServer, hashAlgorithm);
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

        private static void SignVsix(string vsixPackagePath, X509Certificate2 certificate, string hashAlgorithm, Action<SignEventArgs> finishAction)
        {
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
                    signatureManager.Sign(partsToSign, certificate);
                }
                catch (CryptographicException ex)
                {
                    if (finishAction != null)
                        finishAction(new SignEventArgs(false, null, "Signing could not be completed: " + ex.Message));
                    return;
                }

                if (ValidateSignatures(package))
                {
                    _output.AppendLine("VSIX signing completed successfully.");
                    if (finishAction != null)
                        finishAction(new SignEventArgs(true, _output.ToString(), null));
                }
                else
                {
                    if (finishAction != null)
                        finishAction(new SignEventArgs(false, "The digital signature is invalid, there may have been a problem with the signing process.", _error.ToString()));
                }
            }
        }

        private static bool SignBinary(string path,
                                       string thumbPrint,
                                       string certPath,
                                       string certPassword,
                                       string timestampServer,
                                       string hashAlgorithm)
        {
            string command;
            if (thumbPrint != null)
            {
                // "sign /fd {0} /sha1 {1} /a /t {2} {3}"
                command = String.Format(SignBinaryWithCertCmd,
                                        hashAlgorithm,
                                        thumbPrint,
                                        timestampServer,
                                        path);
            }
            else
            {
                // "sign /fd {0} /f {1} /t {2} /p {3} {4}"
                command = String.Format(SignBinaryWithPfxCmd,
                                        hashAlgorithm,
                                        certPath,
                                        timestampServer,
                                        certPassword,
                                        path);
            }
            string output;
            string error;
            int exitCode = ExecuteCommand(command, out output, out error);
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
            string command = String.Format(VerifyDigitalSignatureCmd, path);

            string output;
            string error;
            int exitCode = ExecuteCommand(command, out output, out error);
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

        private static int ExecuteCommand(string arguments, out string output, out string error)
        {
            if (Signtool == null)
            {
                throw new ArgumentException("Cannot find signtool.exe");
            }

            ProcessStartInfo procStartInfo = new ProcessStartInfo(Signtool, arguments);
            procStartInfo.CreateNoWindow = true;
            procStartInfo.RedirectStandardOutput = true;
            procStartInfo.RedirectStandardError = true;
            procStartInfo.UseShellExecute = false;

            Process proc = new Process();
            proc.StartInfo = procStartInfo;
            int exitCode = -1;
            try
            {
                proc.Start();
                output = proc.StandardOutput.ReadToEnd();
                error = proc.StandardError.ReadToEnd();
                proc.WaitForExit();
                exitCode = proc.ExitCode;
            }
            finally
            {
                proc.Close();
            }

            return exitCode;
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
            var programFilesX86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);

            if (string.IsNullOrEmpty(programFilesX86))
            {
                programFilesX86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
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
    }
}
