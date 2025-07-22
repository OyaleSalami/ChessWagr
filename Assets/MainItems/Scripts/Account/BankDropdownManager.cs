using UnityEngine;
using TMPro;  // For TextMeshPro Dropdown
using System.Collections.Generic;
using System.Diagnostics;

public class BankDropdownManager : MonoBehaviour
{
    public TMP_Dropdown bankDropdown;

    // Create a list of bank names and their corresponding codes
    private List<Bank> banks = new List<Bank>()
    {
        new Bank("Access Bank", "044"),
        new Bank("First Bank", "011"),
        new Bank("GTBank", "058"),
        new Bank("UBA", "070"),
        new Bank("Zenith Bank", "035"),
        new Bank("Sterling Bank", "076"),
        new Bank("Ecobank", "232"),
        new Bank("Unity Bank", "501"),
        new Bank("Skye Bank", "222"),
        new Bank("Fidelity Bank", "073"),
        new Bank("Diamond Bank", "030"),
        new Bank("Union Bank", "082"),
        new Bank("Wema Bank", "007"),
        new Bank("Stanbic IBTC", "057"),
        new Bank("Citibank", "301"),
        new Bank("Keystone Bank", "049"),
        new Bank("GTBank", "058"),  // Duplicate GTBank, same code as above
        new Bank("Jaiz Bank", "102"),
        new Bank("Taj Bank", "404"),
        new Bank("VFD", "505"),
        new Bank("E-Transact", "505"),
        new Bank("Fairmoney", "603"),
        new Bank("Carbon", "611"),
        new Bank("Eyowo", "803"),
        new Bank("Alat by Wema", "056"),
        new Bank("Paycom", "402"),
        new Bank("Bank of Agriculture", "003"),
        new Bank("Stanbic IBTC Bank", "415"),
        new Bank("SunTrust Bank", "085"),
        new Bank("FCMB", "809"),
        new Bank("Lagos Building Society", "605"),
        new Bank("Stanbic Bank", "100"),
        new Bank("NPF Microfinance Bank", "114"),
        new Bank("Hedonmark Bank", "423")
    };

    // Start is called before the first frame update
    void Start()
    {
        PopulateBankDropdown();
    }

    // Populate the TMP_Dropdown with the bank list
    private void PopulateBankDropdown()
    {
        bankDropdown.ClearOptions();  // Clear any existing options

        List<TMP_Dropdown.OptionData> dropdownOptions = new List<TMP_Dropdown.OptionData>();

        foreach (var bank in banks)
        {
            // Create and add an option for each bank
            TMP_Dropdown.OptionData option = new TMP_Dropdown.OptionData(bank.name);
            dropdownOptions.Add(option);
        }

        bankDropdown.AddOptions(dropdownOptions);

        // Optionally, add a listener to get the selected bank code when a bank is selected
        bankDropdown.onValueChanged.AddListener(delegate {
            OnBankSelected(bankDropdown.value);
        });
    }

    // Called when a bank is selected from the dropdown
    private void OnBankSelected(int index)
    {
        string selectedBankName = banks[index].name;
        string selectedBankCode = banks[index].code;

        print("Selected Bank: " + selectedBankName + " - Code: " + selectedBankCode);

        // You can now use the selectedBankCode for further processing, such as validating the account
    }

    public string GetSelectedBankCode()
    {
        if (bankDropdown.options.Count == 0 || bankDropdown.value < 0)
        {
            return "";  // Return empty string if no bank is selected
        }

        return banks[bankDropdown.value].code;  // Return the bank code of the selected bank
    }

}

// Define a Bank class to hold the bank name and code
public class Bank
{
    public string name;
    public string code;

    public Bank(string name, string code)
    {
        this.name = name;
        this.code = code;
    }
}
