package ca.bcit.hostbasecardemulationtest

import android.content.Context
import android.content.Intent
import android.nfc.cardemulation.HostApduService
import android.os.Bundle
import io.jsonwebtoken.Jwts
import io.jsonwebtoken.security.Keys
import javax.crypto.SecretKey

/**
 * HostCardEmulatorService is the service that handles all the NFC data
 * transfer between the Android application and the NFC reader.
 */
class HostCardEmulatorService: HostApduService() {
    companion object {
        val TAG = "Host Card Emulator"
        val STATUS_SUCCESS = "9000"
        val STATUS_FAILED = "6F00"
        val CLA_NOT_SUPPORTED = "6E00"
        val INS_NOT_SUPPORTED = "6D00"
        val AID_LOGIN = "F0000EFABCDEF0"
        val AID_HEARTBEAT = "F0000EFABCDEF1"
        val AID_TEST = "F0000EFABCDEF2"
        val SELECT_INS = "A4"
        val DEFAULT_CLA = "00"
        val MIN_APDU_LENGTH = 12

        /**
         * Allows other activities to reference this service as destination via intent.
         */
        fun newIntent(context: Context): Intent {
            val intent = Intent(context, HostCardEmulatorService::class.java)
            return intent
        }
    }

    private val charset = Charsets.UTF_8

    private var doctor = Doctor("","")

    private var position : Int = 0

    /**
     * Overriden onStartCommand function.
     * Initiates the doctor variable via intent
     * Updates the position (ID) of the patient selected that is attached to the doctor object.
     *
     * @param:
     *  intent: the intent passed from MainActivity or LoggedInPage that holds the current doctor
     *          and the position (ID) of the patient selected (if any)
     */
    override fun onStartCommand(intent: Intent?, flags: Int, startId: Int): Int {
        doctor = intent?.getSerializableExtra("doctor") as Doctor
        position = intent.getIntExtra("patientPositionInArray", 0)

        return super.onStartCommand(intent, flags, startId)
    }

    override fun onDeactivated(reason: Int) {
        //do nothing
    }

    /**
     * Overriden processCommandApdu function.
     * Decide on what data to send via NFC to the reader depending on the APDU command received.
     * There are three possible success messages:
     *  1) AID_LOGIN
     *      -Sends the doctor and password entered from the MainActivity to the reader via NFC as
     *      a JSON Web Token
     *  2) AID_HEARBEAT
     *      -Sends the patient position (ID) selected from LoggedInPage
     *  3) AID_TEST
     *      -Sends the maximum amount of data a typical Android NFC service can send. This function
     *      is NOT used in normal operation. It is left in the code in case additional
     *      testing/validation is desired.
     *
     * Error:
     *  1) If user is not logged in
     *      -Send STATUS_FAIL
     *  2) A unknown APDU message
     *      -Send Error string converted to byte array
     *
     * @param:
     *  commandApdu: the byte array containing the APDU command
     *
     * @return:
     *  the message in byte array that will be sent via NFC to the reader
     */
    override fun processCommandApdu(commandApdu: ByteArray?, extras: Bundle?): ByteArray {
        if (commandApdu == null) {
            //do nothing
        }

        val hexCommandApdu = Utils.toHex(commandApdu!!)

        if (!checkIsLogin()) {
            return Utils.hexStringToByteArray(STATUS_FAILED)
        }

        if (hexCommandApdu.substring(10, 24) == AID_LOGIN) {
            val sendString: String
            val keyString = "CDSuDNSIaERroYI1Q3fqcR8mcFqaienU"
            val keyByte: ByteArray = keyString.toByteArray(charset)
            val keySecret: SecretKey = Keys.hmacShaKeyFor(keyByte)
            sendString = Jwts.builder()
                    .claim("username", doctor.userName).signWith(keySecret)
                    .claim("password", doctor.password).signWith(keySecret)
                    .compact()

            return sendString.toByteArray(charset)
        } else if (hexCommandApdu.substring(10, 24) == AID_HEARTBEAT) {
            val patientIdString = doctor.patientList[position].patientId
            return patientIdString.toByteArray(charset)
        } else if (hexCommandApdu.substring(10, 24) == AID_TEST) {
            var testString = ""

            var i = 0
            while (i < 15) {
                //var value = i % 8
                testString += "0123456789ABCDEF"
                ++i
            }
            testString += "0123456789ABCDE"

            return testString.toByteArray(charset)
        } else {
            val errorSendString = "Error"
            return errorSendString.toByteArray(charset)
        }
    }

    /**
     * Check if user is logged in.
     * If the password field is empty, the user is not logged in.
     */
    private fun checkIsLogin() : Boolean {
        return doctor.password != ""
    }
}