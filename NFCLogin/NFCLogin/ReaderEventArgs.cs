using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFCLogin
{
    /// <summary>
    /// A class to store reader event arguments.
    /// </summary>
    public class ReaderEventArgs : EventArgs
    {

        public String Message {get;}

        public ReaderEventArgs(String message = "")
        {
            Message = message;
        }
    }
}
