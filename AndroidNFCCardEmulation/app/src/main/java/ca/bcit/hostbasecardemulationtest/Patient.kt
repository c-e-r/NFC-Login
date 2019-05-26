package ca.bcit.hostbasecardemulationtest

import java.io.Serializable

/**
 * Data class to hold the Patient attributes.
 */
data class Patient(val patientId: String, val patientName: String) : Serializable