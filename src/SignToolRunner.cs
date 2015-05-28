using System;
using System.Diagnostics;
using System.IO;

namespace CodeTitans.Signature
{
    /// <summary>
    /// Helper class to invoke signtool.exe
    /// </summary>
    static class SignToolRunner
    {
        /// <summary>
        /// Invokes signtool.exe and returns exit-code along with standard-output and standard-error.
        /// </summary>
        public static int ExecuteCommand(string arguments, out string output, out string error)
        {
            var signtoolPath = EnsureSignTool();
            if (string.IsNullOrEmpty(signtoolPath))
            {
                throw new ArgumentException("Cannot find signtool.exe");
            }

            var procStartInfo = new ProcessStartInfo(signtoolPath, arguments);
            procStartInfo.CreateNoWindow = true;
            procStartInfo.RedirectStandardOutput = true;
            procStartInfo.RedirectStandardError = true;
            procStartInfo.UseShellExecute = false;

            var proc = new Process();
            proc.StartInfo = procStartInfo;
            try
            {
                proc.Start();
                output = proc.StandardOutput.ReadToEnd();
                error = proc.StandardError.ReadToEnd();
                proc.WaitForExit();

                return proc.ExitCode;
            }
            finally
            {
                proc.Close();
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
