using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFCLogin
{
    /// <summary>
    /// Thrown when a NFC device is not found in NFCReader functions that interact with NFC devices.
    /// </summary>
    class CardNotFoundException : Exception
    {
    }
}
