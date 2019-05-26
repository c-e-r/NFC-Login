package ca.bcit.hostbasecardemulationtest

import java.io.Serializable

/**
 * Data class to hold the Doctor attributes.
 * Auto generates a list of Patients attached to the doctor.
 */
data class Doctor(var userName: String, var password: String) : Serializable {
    var patientList: ArrayList<Patient> = arrayListOf<Patient>()

    /**
     * Generates a static list of patients attached to this doctor.
     */
    fun initializePatient() {
        val patient1 = Patient("001", "Ben");
        val patient2 = Patient("002", "Simon");
        val patient3 = Patient("003", "Shoban");
        val patient4 = Patient("004", "Cameron");
        val patient5 = Patient("005", "Phat");
        val patient6 = Patient("006", "Jacky");
        val patient7 = Patient("007", "Ian");
        val patient8 = Patient("008", "Keishi");

        patientList.add(patient1);
        patientList.add(patient2);
        patientList.add(patient3);
        patientList.add(patient4);
        patientList.add(patient5);
        patientList.add(patient6);
        patientList.add(patient7);
        patientList.add(patient8);

    }
}