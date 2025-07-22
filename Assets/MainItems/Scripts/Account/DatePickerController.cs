using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DatePickerController : MonoBehaviour
{
    public string Output;

    // Public lists for YYYY, MM, and DD
    public List<string> years;
    public List<string> months;
    public List<string> days;

    // Reference to TMP_Text components for displaying date in the picker
    public TMP_Text yearText;
    public TMP_Text monthText;
    public TMP_Text dayText;

    // Reference to the TMP_InputField for output
    public TMP_Text selectedInputField;  // The currently selected input field

    // Internal index for the currently selected year, month, and day
    private int currentYearIndex = 0;
    private int currentMonthIndex = 0;
    private int currentDayIndex = 0;

    void Start()
    {
        // Initialize the date picker by setting the default values (first in the list)
        UpdateDatePickerDisplay();
    }

    // Method to handle Done button press and update the selected InputField
    public void Done()
    {
        // Construct the date string from the selected values
        Output = $"{dayText.text}/{monthText.text}/{yearText.text}";

        // Update the selected TMP_InputField with the selected date
        if (selectedInputField != null)
        {
            selectedInputField.text = Output;
        }
    }

    // Method to update the TMP_Text fields with current selections
    private void UpdateDatePickerDisplay()
    {
        yearText.text = years[currentYearIndex];
        monthText.text = months[currentMonthIndex];
        dayText.text = days[currentDayIndex];
    }

    // Method to cycle through the year list (you can bind this to a button)
    public void NextYear()
    {
        currentYearIndex = (currentYearIndex + 1) % years.Count;
        UpdateDatePickerDisplay();
    }

    public void PreviousYear()
    {
        currentYearIndex = (currentYearIndex - 1 + years.Count) % years.Count;
        UpdateDatePickerDisplay();
    }

    // Method to cycle through the month list (you can bind this to a button)
    public void NextMonth()
    {
        currentMonthIndex = (currentMonthIndex + 1) % months.Count;
        UpdateDatePickerDisplay();
    }

    public void PreviousMonth()
    {
        currentMonthIndex = (currentMonthIndex - 1 + months.Count) % months.Count;
        UpdateDatePickerDisplay();
    }

    // Method to cycle through the day list (you can bind this to a button)
    public void NextDay()
    {
        currentDayIndex = (currentDayIndex + 1) % days.Count;
        UpdateDatePickerDisplay();
    }

    public void PreviousDay()
    {
        currentDayIndex = (currentDayIndex - 1 + days.Count) % days.Count;
        UpdateDatePickerDisplay();
    }

    // Method to set the active input field when clicked (call this when an input field is selected)
    public void SelectInputField(TMP_Text inputField)
    {
        selectedInputField = inputField;
    }
}
