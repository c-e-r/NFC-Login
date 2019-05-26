using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Timers;
using System.ComponentModel;

namespace NFCLogin
{
    /// <summary>
    /// A class for the implementation of the NFCReader class that uses a "Fongwah S9-BU-13-00" NFC reader.
    /// </summary>
    class FongwahReader : NFCReader
    {
        #region DLL Imports
        /// <summary>
        /// Initializes the communication port for the Fongwah NFC reader.
        /// </summary>
        /// <param name="port">The COM port to connect to. USB is COM port 100.</param>
        /// <param name="baud">The baud rate to use for the communication. Use 0 in the case of a USB reader.</param>
        /// <returns>Less than 0 if the function failed. Otherwise returns a handle for the Fongwah NFC reader</returns>
        [DllImport("umf.DLL", EntryPoint = "fw_init")]
        public static extern Int32 fw_init(Int16 port, Int32 baud);

        /// <summary>
        /// Close the communication port for the Fongwah NFC reader.
        /// </summary>
        /// <param name="icdev">A Fongwah NFC reader device handle.</param>
        /// <returns>0 if successful; otherise nonzero.</returns>
        [DllImport("umf.DLL", EntryPoint = "fw_exit")]
        public static extern Int32 fw_exit(Int32 icdev);

        /// <summary>
        /// Find a NFC device. Equivalent to calling fw_request, fw_anticoll, and fw_select (These functions not imported)
        /// </summary>
        /// <param name="icdev">A Fongwah NFC reader device handle.</param>
        /// <param name="_Mode">The mode to operate in. 0 = IDLE mode. 1 = ALL mode.</param>
        /// <param name="_Snr">The returned card serial number.</param>
        /// <returns>0 if successful; otherise nonzero.</returns>
        [DllImport("umf.DLL", EntryPoint = "fw_card")]
        public static extern Int32 fw_card(Int32 icdev, Byte _Mode, ulong[] _Snr);

        /// <summary>
        /// Function for APDU data exchange.
        /// </summary>
        /// <param name="ICDev">A Fongwah NFC reader device handle.</param>
        /// <param name="slen">The length of the information that is being sent.</param>
        /// <param name="sbuff">The first byte in an array containing the ADPU command to send.</param>
        /// <param name="rlen">A byte that will contain the length of the received ADPU response.</param>
        /// <param name="rbuff">The first byte in an array that will store the ADPU response.</param>
        /// <param name="tt">The timout period for the NFC command. In 10ms intervals.</param>
        /// <param name="FG">The split length. Recommended to be less than 64 by the Fongwah SDK reference manual.</param>
        /// <returns>0 if successful; otherise nonzero.</returns>
        [DllImport("umf.DLL", EntryPoint = "fw_pro_commandlink")]
        public static extern Int16 fw_pro_commandlink(Int32 ICDev, Byte slen, ref Byte sbuff, ref Byte rlen, ref Byte rbuff, Byte tt, Byte FG);

        /// <summary>
        /// Card reset function. For more information see https://en.wikipedia.org/wiki/Answer_to_reset
        /// </summary>
        /// <param name="ICDev">A Fongwah NFC reader device handle.</param>
        /// <param name="rlen">A byte that will contain the length of the returned reset information.</param>
        /// <param name="rbuff">The first byte in an array that will contain the returned reset information.</param>
        /// <returns>0 if successful; otherise nonzero.</returns>
        [DllImport("umf.DLL", EntryPoint = "fw_pro_reset")]
        public static extern Int16 fw_pro_reset(Int32 ICDev, ref Byte rlen, ref Byte rbuff);

        /// <summary>
        /// Function to get Fongwah reader version.
        /// </summary>
        /// <param name="icdev">A Fongwah NFC reader device handle.</param>
        /// <param name="rlen">A byte that will contain the length of the returned version information.</param>
        /// <param name="version">The first byte in an array that will contain the version information </param>
        /// <returns>0 if successful; otherise nonzero.</returns>
        [DllImport("umf.DLL", EntryPoint = "fw_getver")]
        public static extern Int32 fw_getver(Int32 icdev, ref Byte version);

        /// <summary>
        /// Make the Fongwah device beep
        /// </summary>
        /// <param name="icdev">A Fongwah NFC reader device handle.</param>
        /// <param name="_Msec">The length of time the device should be in 10ms increments.</param>
        /// <returns>0 if successful; otherise nonzero.</returns>
        [DllImport("umf.DLL", EntryPoint = "fw_beep")]
        public static extern Int32 fw_beep(Int32 icdev, uint _Msec);
        #endregion

        #region Constants
        private const int RECONNECT_TIMER_INTERVAL = 5000;
        private const int CARD_POLL_TIMER_INTERVAL = 1000;

        private const int SEH_EXCEPTION_SLEEP = 2000;

        private const int DEVICE_PORT_NUMBER = 100;
        private const int DEVICE_BAUD_RATE = 0;

        private const int RESPONSE_DELAY_10MS = 9;
        private const int RESPONSE_SPLIT_LENGTH_BYTES = 60;

        private const int CARD_ID_SIZE = 3;
        private const int LARGE_BYTE_SIZE = 256;
        private const int RECONNECT_BEEP_DURATION = 5;
        #endregion

        /// <summary>
        /// The event is triggered when an NFC device is detected near the Fongwah reader.
        /// </summary>
        public override event ReaderEventHandler CardDetected;

        /// <summary>
        /// The event is triggered when an NFC device is removed from the Fongwah reader.
        /// </summary>
        public override event ReaderEventHandler CardRemoved;

        /// <summary>
        /// The event is triggered when the heartbeat signal from the NFC changes.
        /// </summary>
        public override event ReaderEventHandler HeartbeatChanged;

        private bool _cardPresent;
        private string _lastHeartbeat = null;

        private Timer _monitorTimer;
        private Timer _reconnectTimer;

        private Int32 _deviceHandle;

        #region Public Overridden Functions

        /// <summary>
        /// Closes the connection to the Fongwah reader.
        /// </summary>
        public override void Close()
        {
            _reconnectTimer.Stop();
            _monitorTimer.Stop();

            _reconnectTimer.Dispose();
            _monitorTimer.Dispose();

            if(_deviceHandle > 0)
            {
                fw_exit(_deviceHandle);
            }
            _deviceHandle = 0;
        }
        
        /// <summary>
        /// Connect to the Fongwah reader
        /// </summary>
        /// <exception cref="ReaderNotFoundException">The reader could not be found.</exception>
        /// <exception cref="ReaderAlreadyConnectedException">The reader is already connected.</exception>
        public override void Connect()
        {        
            if (_deviceHandle != 0)
            {
                throw new ReaderAlreadyConnectedException();
            }
            else
            {
                SetupReaderReconnecter();
                SetCardPollTimer();

                if (!ConnectToReader())
                {
                    _reconnectTimer.Start();
                }
                else
                {
                    _monitorTimer.Start();
                }
            }
        }

        /// <summary>
        /// Attempts to select the AID from whatever card is present on the Fongwah reader.
        /// </summary>
        /// <param name="aid"></param>
        /// <returns></returns>
        /// <exception cref="NFCCommandFailedException">The NFC command failed.</exception>"
        public override string SelectAID(string aid)
        {
            try
            {
                var response = GetCommandLinkResponse(SELECT_COMMAND + aid);
                var responseString = ByteConverter.ByteArrayToUTF8(response.Data);

                return responseString;
            }
            catch (NFCCommandFailedException e)
            {
                Debug.WriteLine(e.StackTrace);
                throw e;
            }
        }

        #endregion

        #region Private Functions

        /// <summary>
        /// Timer function. Will keep track of whether a card is present and trigger events when they are detected/removed.
        /// </summary>
        /// <param name="source">unused</param>
        /// <param name="e">unused</param>
        private void CheckForCard(Object source, ElapsedEventArgs e)
        {
            if (!_cardPresent)
            { 
                if (!ReaderIsConnected())
                {
                    Debug.WriteLine("\nNot connected!\n");
                    StopPollingAndStartReconnecting();
                    return;
                }                

                if (PrepareCard() && ResetCard())
                {
                    Debug.WriteLine("card found");
                    _cardPresent = true;
                    CardDetected?.Invoke(this, new ReaderEventArgs());
                }
            }
            else
            {
                Debug.WriteLine("card is expected to be there");

                try
                {
                    CommandLinkResponse response = GetCommandLinkResponse(SELECT_COMMAND + AID_HEARTBEAT);

                    String heartbeatMessage = ByteConverter.ByteArrayToUTF8(response.Data);

                    if (_lastHeartbeat != heartbeatMessage)
                    {
                        HeartbeatChanged?.Invoke(this, new ReaderEventArgs(heartbeatMessage));
                    }
                    _lastHeartbeat = heartbeatMessage;


                    Debug.WriteLine("card is there");
                }
                catch (NFCCommandFailedException)
                {
                    Debug.WriteLine("card not there");
                    _cardPresent = false;
                    CardRemoved?.Invoke(this, new ReaderEventArgs());
                    return;
                }
               
            }                
        }
        
        /// <summary>
        /// Sets up the timer for reconnecting.
        /// </summary>
        private void SetupReaderReconnecter()
        {
            _reconnectTimer = new Timer(RECONNECT_TIMER_INTERVAL);
            _reconnectTimer.Elapsed += ReconnectToReader;
            _reconnectTimer.AutoReset = true;
        }

        /// <summary>
        /// Timer function. Will attempt to reconnect to the Fongwah reader.
        /// </summary>
        /// <param name="source">unused</param>
        /// <param name="e">unused</param>
        private void ReconnectToReader(Object source = null, ElapsedEventArgs e = null)
        {
            Debug.WriteLine("attempting reconnect");
            try
            {
                if (ConnectToReader())
                {
                    Debug.WriteLine("\nReconnected!\n");
                    StopReconnectingAndStartPolling();
                    fw_beep(_deviceHandle, RECONNECT_BEEP_DURATION);
                    System.Threading.Thread.Sleep(250);
                    fw_beep(_deviceHandle, RECONNECT_BEEP_DURATION);

                }
            }
            catch (SEHException)
            {
                System.Threading.Thread.Sleep(SEH_EXCEPTION_SLEEP);
            }
        }

        /// <summary>
        /// Sets up the timer for card polling.
        /// </summary>
        private void SetCardPollTimer()
        {
            _monitorTimer = new Timer(CARD_POLL_TIMER_INTERVAL);
            _monitorTimer.Elapsed += CheckForCard;
            _monitorTimer.AutoReset = true;
        }

        /// <summary>
        /// Calls the Fongwah funciton to reconnect to the reader and returns a bool indication success.
        /// </summary>
        /// <returns>True if the device connected and false if the connection failed.</returns>
        private bool ConnectToReader()
        {
            _deviceHandle = fw_init(DEVICE_PORT_NUMBER, DEVICE_BAUD_RATE);
            return _deviceHandle > 0;
        }

        /// <summary>
        /// Calls the Fongwah version function to determine if the reader is still connected.
        /// </summary>
        /// <returns></returns>
        private bool ReaderIsConnected()
        {
            Byte[] versionBuffer = new Byte[3];
            return fw_getver(_deviceHandle, ref versionBuffer[0]) == 0;
        }

        /// <summary>
        /// Calls the Fongwah card function and returns a bool indicating success.
        /// </summary>
        /// <returns>True if the command succeeded and false if it failed.</returns>
        private bool PrepareCard()
        {
            Debug.WriteLine("card isnt expected");
            ulong[] cardID = new ulong[CARD_ID_SIZE];

            int status = fw_card(_deviceHandle, 1, cardID);
            return status == 0;
        }

        /// <summary>
        /// Calls the Fongwah reset card function and returns a bool indicating success.
        /// </summary>
        /// <returns>True if the command succeeded and false if it failed.</returns>
        private bool ResetCard()
        {
            Debug.WriteLine("resetting card");

            Byte resetLength = 0;
            Byte[] resetBuffer = new Byte[LARGE_BYTE_SIZE];

            return fw_pro_reset(_deviceHandle, ref resetLength, ref resetBuffer[0]) == 0;
        }

        /// <summary>
        /// Sends the given command string to a NFCdevice and returns the device's ADPU response.
        /// </summary>
        /// <param name="commandString">The command string to be sent to the NFC device.</param>
        /// <returns>The ADPU response.</returns>
        /// <exception cref="NFCCommandFailedException">Thrown when the NFC device cannot be found, the NFC readers handle is invalid, or the NFC device gives and error code as its ADPU response.</exception>"
        private CommandLinkResponse GetCommandLinkResponse(string commandString)
        {
            Byte[] command = ByteConverter.HexStringToByteArray(commandString);
            Byte[] response = new Byte[LARGE_BYTE_SIZE];

            Byte commandLength = 12;
            Byte responseLength = 0;

            int state = fw_pro_commandlink(_deviceHandle, commandLength, ref command[0], ref responseLength, ref response[0], RESPONSE_DELAY_10MS, RESPONSE_SPLIT_LENGTH_BYTES);

            return new CommandLinkResponse(state, responseLength, response);
        }

        /// <summary>
        /// Stop trying to reconnect to the NFC reader and start polling for NFC devices.
        /// </summary>
        private void StopReconnectingAndStartPolling()
        {
            _reconnectTimer.Stop();
            _monitorTimer.Start();
        }

        /// <summary>
        /// Stops polling for NFC devices and start trying to reconnect to the NFC reader.
        /// </summary>
        private void StopPollingAndStartReconnecting()
        {
            ReconnectToReader();
            _monitorTimer.Stop();
            _reconnectTimer.Start();
        }

        #endregion
    }
}
