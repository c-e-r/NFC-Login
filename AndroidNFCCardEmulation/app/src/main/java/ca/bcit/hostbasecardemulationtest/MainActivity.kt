package ca.bcit.hostbasecardemulationtest

import android.content.Context
import android.content.Intent
import android.support.v7.app.AppCompatActivity
import android.os.Bundle
import android.view.View
import android.widget.Toast
import kotlinx.android.synthetic.main.activity_main.*

/**
 * The MainActivity class that holds the activity on app startup.
 * Holds the login form.
 */
class MainActivity : AppCompatActivity() {

    /**
     * Overridden onCreate function.
     * Calls function to bind the login button listener
     */
    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContentView(R.layout.activity_main)
        setLoginButtonListener()
    }

    /**
     * Overridden onResume function.
     * Calls the service intents and resets the user login information
     */
    override fun onResume() {
        super.onResume()
        val serviceIntent = HostCardEmulatorService.newIntent(this@MainActivity)
        val emptyDoctor = Doctor("", "")
        serviceIntent.putExtra("doctor", emptyDoctor)
        startService(serviceIntent)
    }

    /**
     * Function to bind the login in button to a listener to the function to log in.
     */
    private fun setLoginButtonListener() {
        buttonSendData.setOnClickListener {
            logIn()
        }
    }

    /**
     * Logs into a new page only if the user has entered a username and password.
     * Makes a toast message informing the user if not all fields are filled.
     */
    fun logIn() {
        val intent = LoggedInPage.newIntent(this)
        val userName = usernameEditText.text.toString()
        val password = passwordEditText.text.toString()

        if (!(userName == "" && password == "")) {
            intent.putExtra("userName", userName)
            intent.putExtra("password", password)
            startActivity(intent)
        } else {
            Toast.makeText(this, "Please enter a username and password", Toast.LENGTH_LONG).show()
        }
    }

    /**
     * Create intent function used by other activities
     */
    companion object {
        fun newIntent(context: Context): Intent {
            val intent = Intent(context, MainActivity::class.java)
            return intent
        }
    }
}
