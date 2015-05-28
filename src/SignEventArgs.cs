using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeTitans.Signature
{
    internal sealed class SignEventArgs : EventArgs
    {
        public SignEventArgs(string output, string error)
        {
            this.Output = output;
            this.Error = error;
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
