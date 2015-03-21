using System;

namespace CodeTitans.Signature.Tools
{
    /// <summary>
    /// Arguments passes along with ToolRunner events.
    /// </summary>
    public sealed class ToolRunnerEventArgs : EventArgs
    {
        #region Properties

        public ToolRunnerEventArgs(int exitCode, string output, string error)
        {
            ExitCode = exitCode;
            Output = output;
            Error = error;
        }

        public int ExitCode
        {
            get;
            private set;
        }

        public string Output
        {
            get;
            private set;
        }

        public string Error
        {
            get;
            private set;
        }

        public bool IsSuccessful
        {
            get { return string.IsNullOrEmpty(Error); }
        }

        #endregion
    }
}
