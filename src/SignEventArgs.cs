using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeTitans.Signature
{
    internal sealed class SignEventArgs : EventArgs
    {
        public SignEventArgs(bool success, string output, string error)
        {
            this.Success = success;
            this.Output = output;
            this.Error = error;
        }

        public bool Success
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
    }
}
