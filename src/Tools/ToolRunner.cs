using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Text;

namespace CodeTitans.Signature.Tools
{
    /// <summary>
    /// Class that runs specified executable, captures its output and error messages.
    /// 
    /// Note: The outputs are assumed to be small. They are all accumulated inside internal buffers and at process exit just flushed.
    /// If this behavior doesn't suit specific tool needs, override the ProcessOutputLine() and ProcessErrorLine() methods to process them as they arrive.
    /// </summary>
    public class ToolRunner : IDisposable
    {
        private Process _process;
        private StringBuilder _output;
        private StringBuilder _error;
        private bool _isProcessing;

        public event EventHandler<ToolRunnerEventArgs> Finished;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public ToolRunner()
        {
            _process = new Process();

            _process.StartInfo.UseShellExecute = false;
            _process.StartInfo.CreateNoWindow = true;
            _process.StartInfo.RedirectStandardOutput = true;
            _process.StartInfo.RedirectStandardError = true;
            _process.StartInfo.RedirectStandardInput = true;

            _process.OutputDataReceived += OutputDataReceived;
            _process.ErrorDataReceived += ErrorDataReceived;
        }

        ~ToolRunner()
        {
            Dispose(false);
        }

        /// <summary>
        /// Init constructor. Setups the executable and its working directory.
        /// </summary>
        /// <param name="fileName">Name of the binary to execute</param>
        /// <param name="workingDirectory">Executable working directory</param>
        public ToolRunner(string fileName, string workingDirectory)
            : this()
        {
            FileName = fileName;
            WorkingDirectory = workingDirectory;
        }

        #region Properties

        public string FileName
        {
            get { return _process.StartInfo.FileName; }
            set { _process.StartInfo.FileName = value; }
        }

        public string WorkingDirectory
        {
            get { return _process.StartInfo.WorkingDirectory; }
            set { _process.StartInfo.WorkingDirectory = value; }
        }

        public StringDictionary Environment
        {
            get { return _process.StartInfo.EnvironmentVariables; }
        }

        public string Arguments
        {
            get { return _process.StartInfo.Arguments; }
            set { _process.StartInfo.Arguments = value; }
        }

        public bool IsProcessing
        {
            get { return _isProcessing; } // using custom variable instead of _process.HasExited, as 'data processing' after tool termination should also indicate that state
        }

        public int ExitCode
        {
            get;
            private set;
        }

        public string Output
        {
            get;
            protected set;
        }

        public string Error
        {
            get;
            protected set;
        }

        #endregion

        #region Internal Properties

        protected int PID
        {
            get;
            set;
        }

        #endregion

        protected virtual void ProcessOutputLine(string text)
        {
            if (_output != null && text != null)
                _output.AppendLine(text);
        }

        protected virtual void ProcessErrorLine(string text)
        {
            if (_error != null && text != null)
                _error.AppendLine(text);
        }

        private void OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null)
            {
                ProcessOutputLine(e.Data);
            }
        }

        private void ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null)
            {
                ProcessErrorLine(e.Data);
            }
        }

        private void InternalPrepareStarted()
        {
            if (_process.StartInfo.RedirectStandardError)
                _process.BeginErrorReadLine();
            if (_process.StartInfo.RedirectStandardOutput)
                _process.BeginOutputReadLine();
            if (_process.StartInfo.RedirectStandardInput)
                _process.StandardInput.AutoFlush = true;

            PID = _process.Id;
            PrepareStarted(PID);
        }

        /// <summary>
        /// Starts the tool and waits until it completes. It will block the current thread until that time.
        /// The executable file name and all arguments are required to be setup earlier.
        /// </summary>
        /// <returns>Returns 'true', if tool returned exit code '0', otherwise 'false'.</returns>
        public bool Execute()
        {
            if (_isProcessing)
                throw new InvalidOperationException("The process is already running");
            if (_process == null)
                throw new ObjectDisposedException("ToolRunner");
            if (string.IsNullOrEmpty(FileName))
                throw new InvalidOperationException("No executable to start");

            PrepareExecution();

            try
            {
                _process.EnableRaisingEvents = false;
                _process.Start();
                InternalPrepareStarted();

                _process.WaitForExit();
                ExitCode = _process.ExitCode;

                return ExitCode == 0;
            }
            catch (Exception ex)
            {
                ProcessErrorLine(ex.Message);
                return false;
            }
            finally
            {
                // release process resources:
                _process.Close();

                CompleteExecution();
            }
        }

        /// <summary>
        /// Starts the tool asynchronously. It will not block the current thread.
        /// Subscribe to Finished event before, to know, when tool's process completed execution.
        /// </summary>
        public void ExecuteAsync()
        {
            if (_isProcessing)
                throw new InvalidOperationException("The process is already running");
            if (_process == null)
                throw new ObjectDisposedException("ToolRunner");

            PrepareExecution();

            try
            {
                _process.Exited -= AsyncProcessExited;
                _process.Exited += AsyncProcessExited;
                _process.EnableRaisingEvents = true;

                _process.Start();
                InternalPrepareStarted();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(_process.StartInfo.Arguments);

                _process.Close();
                _isProcessing = false;

                ProcessErrorLine(ex.Message);
                NotifyFinished(-1, null, null);
            }
        }

        private void AsyncProcessExited(object sender, EventArgs e)
        {
            if (_process != null)
            {
                ExitCode = _process.ExitCode;
                _process.Exited -= AsyncProcessExited;

                // release process resources:
                _process.Close();

                CompleteExecution();
            }
        }

        private void NotifyFinished(int exitCode, string output, string error)
        {
            var handler = Finished;

            if (handler != null)
            {
                handler(this, new ToolRunnerEventArgs(exitCode, output, error));
            }
        }

        private void PrepareExecution()
        {
            ExitCode = int.MinValue;
            _isProcessing = true;
            Output = null;
            Error = null;
            _output = new StringBuilder();
            _error = new StringBuilder();

            // allow setup customization by sub-classes:
            PrepareStartup();
        }

        private void CompleteExecution()
        {
            var outputText = _output != null && _output.Length > 0 ? _output.ToString() : null;
            var errorText = _error != null && _error.Length > 0 ? _error.ToString() : null;

            _output = null;
            _error = null;
            Output = outputText;
            Error = errorText;

            // consume received data:
            ConsumeResults(outputText, errorText);

            // notify other listeners, in case they want to get something extra:
            NotifyFinished(ExitCode, outputText, errorText);

            _isProcessing = false;
            Cleanup();
        }

        /// <summary>
        /// Method executed before starting the tool, to setup the state of the current runner.
        /// </summary>
        protected virtual void PrepareStartup()
        {
        }

        /// <summary>
        /// Method executed just after staring the tool, to setup extra behavior of the runner.
        /// </summary>
        protected virtual void PrepareStarted(int pid)
        {
            // do nothing, subclasses should handle it, if needed
        }

        protected virtual void ConsumeResults(string output, string error)
        {
            // do nothing, subclasses should handle parsing output
        }

        protected virtual void Cleanup()
        {
            // do nothing, subclasses should handle it, when needed
        }

        #region IDisposable Implementation

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_process != null)
            {
                _process.Exited -= AsyncProcessExited;
                _process.OutputDataReceived -= OutputDataReceived;
                _process.ErrorDataReceived -= ErrorDataReceived;
                _process.Dispose();
                _process = null;
            }

            Finished = null;
        }

        #endregion
    }
}
