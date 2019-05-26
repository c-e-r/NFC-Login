package ca.bcit.hostbasecardemulationtest

import android.content.Context
import android.content.Intent
import android.graphics.Color
import android.graphics.drawable.ColorDrawable
import android.os.Bundle
import android.support.v7.app.AppCompatActivity
import android.view.View
import android.widget.*
import kotlinx.android.synthetic.main.loggedinpage.*

/**
 * The LoggedInPage class holds the activity after logging in successfully.
 * Sends the doctor information and patient ID to HostCardEmulatorService
 * Allows user to select a patient via a list.
 */
class LoggedInPage: AppCompatActivity() {

    private lateinit var doctor : Doctor

    private lateinit var patients : ArrayList<String>

    private lateinit var listView :  ListView

    /**
     * Overridden onCreate function.
     * Prints a welcome message in the header with the username.
     * Calls the sendDefaultLogin function to send the doctor data without the patient ID.
     * Populates a ListView with the patient array attached to the doctor.
     *  -Set a onSelect listener to monitor which patient is selected
     *  -Sends the doctor with the patient ID selected to HostCardEmulatorService via intent.
     */
    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContentView(R.layout.loggedinpage)

        val userName = intent.getStringExtra("userName")
        val password = intent.getStringExtra("password")

        val welcomeMessage = resources.getString(R.string.login_message, userName)
        welcome_message.text = welcomeMessage

        //create the doctor
        doctor = Doctor(userName, password)
        doctor.initializePatient()
        
        sendDefaultLogin()

        patients = ArrayList()

        for (tempPatient in doctor.patientList) {
            patients.add(tempPatient.patientName)
        }

        listView = findViewById<ListView>(R.id.patient_list)

        val adapter = PatientAdapter(this, doctor.patientList)
        listView.adapter = adapter

        listView.onItemClickListener = object : AdapterView.OnItemClickListener {
            override fun onItemClick(p0: AdapterView<*>?, p1: View?, p2: Int, p3: Long) {
                //Send the intent
                val intent = HostCardEmulatorService.newIntent(this@LoggedInPage)
                intent.putExtra("doctor", doctor)
                intent.putExtra("patientPositionInArray", p2)
                startService(intent)
            }
        }
    }

    /**
     * Create intent function used by other activities
     */
    companion object {
        fun newIntent(context: Context): Intent {
            return Intent(context, LoggedInPage::class.java)
        }
    }

    /**
     * Logs out and go back to the Main Page.
     * Also clears the doctor variable in HostCardEmulatorService via intent.
     */
    fun logOut(v: View) {
        val serviceIntent = HostCardEmulatorService.newIntent(this@LoggedInPage)
        val emptyDoctor = Doctor("", "")
        serviceIntent.putExtra("doctor", emptyDoctor)
        startService(serviceIntent)

        finish()
    }

    /**
     * Default login data to be sent to the HostCardEmulatorService service.
     * This contains the doctor without a patient position (starts as no patients selected)
     */
    private fun sendDefaultLogin() {
        val serviceIntent = HostCardEmulatorService.newIntent(this@LoggedInPage)
        serviceIntent.putExtra("doctor", doctor)
        startService(serviceIntent)
    }
}