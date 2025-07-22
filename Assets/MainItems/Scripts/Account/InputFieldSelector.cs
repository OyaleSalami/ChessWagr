using TMPro;
using System.Collections.Generic;
using UnityEngine;

public class InputFieldSelector : MonoBehaviour
{
    public DatePickerController datePickerController; // Reference to the DatePickerController

    // Call this when the input field is clicked
    public void OnInputFieldSelected(TMP_Text inputField)
    {
        datePickerController.gameObject.transform.parent.gameObject.SetActive(true);
        // Set the currently selected input field in the DatePickerController
        datePickerController.SelectInputField(inputField);
    }
}
