using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFCLogin
{
    /// <summary>
    /// Thrown when an NFC ADPU command is given no response or an error response.
    /// </summary>
    class NFCCommandFailedException : Exception
    {
    }
}
