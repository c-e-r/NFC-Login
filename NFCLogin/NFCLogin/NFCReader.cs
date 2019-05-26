using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFCLogin
{

    /// <summary>
    /// An event handler for NFCReader events.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="args"></param>
    public delegate void ReaderEventHandler(object source, ReaderEventArgs args);

    /// <summary>
    /// An abstract class to represent an NFC reader.
    /// </summary>
    public abstract class NFCReader
    {
        public static String AID_LOGIN = "F0000EFABCDEF0";
        public static String AID_HEARTBEAT = "F0000EFABCDEF1";
        public static String AID_TEST = "F0000EFABCDEF2";

        public static String SELECT_COMMAND = "00A4040007";


        /// <summary>
        /// Connect to the NFC reader
        /// </summary>
        /// <exception cref="ReaderNotFoundException">The reader could not be found</exception>
        /// <exception cref="ReaderAlreadyConnectedException">The reader is already connected</exception>
        public abstract void Connect();

        /// <summary>
        /// Closes the connection to the NFC reader.
        /// </summary>
        public abstract void Close();
        
        /// <summary>
        /// Attempts to select the AID from whatever card is present on the reader
        /// </summary>
        /// <param name="aid"></param>
        /// <returns></returns>
        /// <exception cref="NFCCommandFailedException">The NFC command failed</exception>"
        public abstract string SelectAID(String aid);

        /// <summary>
        /// The event is triggered when an NFC device is detected near the reader
        /// </summary>
        public abstract event ReaderEventHandler CardDetected;

        /// <summary>
        /// The event is triggered when an NFC device is removed from the reader
        /// </summary>
        public abstract event ReaderEventHandler CardRemoved;

        /// <summary>
        /// The event is triggered when the heartbeat signal from the NFC changes
        /// </summary>
        public abstract event ReaderEventHandler HeartbeatChanged;
    }
}
