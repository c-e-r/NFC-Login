using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFCLogin
{
    /// <summary>
    /// Thrown when trying to connect to reader while it is already connected.
    /// </summary>
    class ReaderAlreadyConnectedException : Exception
    {
    }
}
