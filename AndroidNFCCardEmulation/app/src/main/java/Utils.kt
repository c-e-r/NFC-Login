/**
 * Utils class holds the hex string to byte array conversion
 * (and byte array to hex string) static functions.
 */
class Utils {

    companion object {
        private val HEX_CHARS = "0123456789ABCDEF"

        /**
         * Converts a string that contains hex data to a byte array.
         * @param:
         *  data: the string with hex data to be converted
         * @return:
         *  result: byte array converted from the hex string
         */
        fun hexStringToByteArray(data: String) : ByteArray {

            val result = ByteArray(data.length / 2)

            for (i in 0 until data.length step 2) {
                val firstIndex = HEX_CHARS.indexOf(data[i]);
                val secondIndex = HEX_CHARS.indexOf(data[i + 1]);

                val octet = firstIndex.shl(4).or(secondIndex)
                result.set(i.shr(1), octet.toByte())
            }

            return result
        }

        private val HEX_CHARS_ARRAY = "0123456789ABCDEF".toCharArray()

        /**
         * Convert a byte array to a hex string.
         * @param:
         *  byteArray: a byte array to be converted to hex string
         * @return:
         *  result: the hex string converted from the byte array
         */
        fun toHex(byteArray: ByteArray) : String {
            val result = StringBuffer()

            byteArray.forEach {
                val octet = it.toInt()
                val firstIndex = (octet and 0xF0).ushr(4)
                val secondIndex = octet and 0x0F
                result.append(HEX_CHARS_ARRAY[firstIndex])
                result.append(HEX_CHARS_ARRAY[secondIndex])
            }

            return result.toString()
        }
    }
}