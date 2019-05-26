using System;
using System.Diagnostics;

namespace NFCLogin
{
    /**
     * <summary>
     * A class that stores relevant data from fw_pro_commandlink() calls.
     * </summary>
     */
    class CommandLinkResponse
    {
        public static String ERROR_RESPONSE = "6F00";
        public static String APPLET_NOT_FOUND_RESPONSE = "6999";

        public Byte[] Data { get; }

        /**
         * <summary>
         * Constructs a CommandLinkResponse using data from a fw_pro_commandlink() call.
         * </summary>
         * <param name="state">
         * The integer returned by fw_pro_commandlink()
         * </param>
         * <param name="responseLength">
         * The length of the response, use the rlen parameter from a fw_pro_commandlink() call.
         * </param>
         * <param name="response">
         * The response itself, use the rbuff parameter from a fw_pro_commandlink() call.
         * </param>
         * <exception cref="NFCCommandFailedException">
         * Thrown when state is not 0 or if the response contains 0x6F00 or 0x6999.
         * </exception>
         */
        public CommandLinkResponse(int state, Byte responseLength, Byte[] response)
        {
            if (state != 0)
            {
                Debug.WriteLine("State:" + state);
                throw new NFCCommandFailedException();
            }

            Data = new Byte[responseLength];

            Array.Copy(response, Data, responseLength);

            String hexString = ByteConverter.ByteArrayToHexString(Data);

            if (hexString == ERROR_RESPONSE || hexString == APPLET_NOT_FOUND_RESPONSE)
            {
                Debug.WriteLine("Error:" + hexString);
                throw new NFCCommandFailedException();
            }
        }
    }
}
