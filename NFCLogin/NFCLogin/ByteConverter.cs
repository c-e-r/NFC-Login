using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFCLogin
{
    /// <summary>
    /// A class for methods to convert to and from byte arrays.
    /// </summary>
    class ByteConverter
    {

        /// <summary>
        /// Convert the given hexedecimal string to an array of bytes.
        /// </summary>
        /// <param name="hexString">The hexedecimal string to convert<./param>
        /// <returns>A byte array representation of the hexedecimal string.</returns>
        public static Byte[] HexStringToByteArray(String hexString)
        {
            if  (hexString.Length % 2 == 1)
            {
                hexString = "0" + hexString;
            }


            Byte[] arr = new Byte[hexString.Length/2];
            for (int i = 0; i < hexString.Length/2; i++)
            {
                String substr = hexString.Substring(2 * i, 2);
                arr[i] = Byte.Parse(substr, NumberStyles.HexNumber);
            }
            return arr;
        }
        
        /// <summary>
        /// Covert the given byte array to a hexedecial string.
        /// </summary>
        /// <param name="arr">The byte array to convert.</param>
        /// <returns>A hexedecimal string representation of the byte array.</returns>
        public static string ByteArrayToHexString(Byte[] arr)
        {
            string hexString = "";
            foreach (Byte b in arr)
            {
                hexString += b.ToString("X2");
            }
            return hexString;
        }

        /// <summary>
        /// Covert the given byte array to a UTF8 string.
        /// </summary>
        /// <param name="arr">The byte array to convert.</param>
        /// <returns>A UTF8 representation of the byte array.</returns>
        public static string ByteArrayToUTF8(Byte[] arr)
        {
            Debug.WriteLine("Recieved: " + ByteArrayToHexString(arr));
            return Encoding.UTF8.GetString(arr);
        }
        
    }
}
