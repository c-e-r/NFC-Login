package ca.bcit.hostbasecardemulationtest

import android.content.Context
import android.view.LayoutInflater
import android.view.View
import android.view.ViewGroup
import android.widget.BaseAdapter
import android.widget.ImageView
import android.widget.TextView
import ca.bcit.hostbasecardemulationtest.R.drawable.wendy

/**
 * PatientAdapter class that overrides the default adapter class.
 * Takes in a list of patients to be populated in the ListView.
 * Constructs the view each patient entry in the ListView.
 */
class PatientAdapter(private val context: Context,
                    private val dataSource: ArrayList<Patient>) : BaseAdapter() {

    private val inflater: LayoutInflater
            = context.getSystemService(Context.LAYOUT_INFLATER_SERVICE) as LayoutInflater

    /**
     * Overridden function to get the patient array list size.
     * @return:
     *  the size of the patient array list
     */
    override fun getCount(): Int {
        return dataSource.size
    }

    /**
     * Overridden function to get the particular patient in the array list
     * @param:
     *  position: the position of the patient on the array list
     * @return:
     *  the patient at the particular position on the array list
     */
    override fun getItem(position: Int): Any {
        return dataSource[position]
    }

    /**
     * Overridden function to get the item position in the array list
     * @param:
     *  position: the position of the patient on the array list
     * @return:
     *  the position returned as long
     */
    override fun getItemId(position: Int): Long {
        return position.toLong()
    }

    /**
     * Gets the patient view and populate it with patient array list.
     * @param:
     *  position: is the selected patient in the array list.
     * @return:
     *  the view with the patient data populated
     */
    override fun getView(position: Int, convertView: View?, parent: ViewGroup): View {
        // Get view for row item
        val rowView = inflater.inflate(R.layout.patient_list_layout, parent, false)

        val patientNameView = rowView.findViewById(R.id.patient_name) as TextView

        val patientIDView = rowView.findViewById(R.id.patient_id) as TextView

        val patientImageView = rowView.findViewById(R.id.patient_image) as ImageView

        val patient = getItem(position) as Patient

        patientNameView.text = patient.patientName
        patientIDView.text = "ID: " + patient.patientId
        patientImageView.setImageResource(wendy)

        return rowView
    }

}

