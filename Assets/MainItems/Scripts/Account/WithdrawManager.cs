using UnityEngine;
using TMPro;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class BankWithdrawManager : MonoBehaviour
{
    [System.Serializable]
    public class Bank
    {
        public string name;
        public string code;

        public Bank(string bankName, string bankCode)
        {
            name = bankName;
            code = bankCode;
        }
    }

    public TMP_Dropdown bankDropdown;
    public TMP_InputField accountNumberInput;
    public TMP_InputField amountInput;
    public Button checkAccountButton;
    public Button withdrawButton;
    public TMP_Text notificationText;

    private float userBalance = 10000;  // Example balance
    private string phpApiUrl = "https://connectboy.isellibuys.com/flutterwave_api.php";
    public string selectedBankCode = "";
    private List<Bank> banks = new List<Bank>();

    void Start()
    {
        InitializeBankList();
        PopulateBankDropdown();

        withdrawButton.interactable = false;
        checkAccountButton.onClick.AddListener(() => StartCoroutine(ValidateAccount()));
        withdrawButton.onClick.AddListener(() => StartCoroutine(WithdrawFunds()));

        bankDropdown.onValueChanged.AddListener(delegate { OnBankSelected(); });
    }

    void InitializeBankList()
    {
        banks = new List<Bank>
        {
            new Bank("Select Bank", ""),
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
            new Bank("Jaiz Bank", "102"),
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
            new Bank("FCMB", "809")
        };
    }

    void PopulateBankDropdown()
    {
        bankDropdown.options.Clear();
        foreach (Bank bank in banks)
        {
            bankDropdown.options.Add(new TMP_Dropdown.OptionData(bank.name));
        }
        bankDropdown.RefreshShownValue();
    }

    void OnBankSelected()
    {
        selectedBankCode = banks[bankDropdown.value].code;
        Debug.Log("Selected bank code: " + selectedBankCode);
    }

    IEnumerator ValidateAccount()
    {
        string bankCode = selectedBankCode;  // This will come from your UI (dropdown)
        string accountNumber = accountNumberInput.text;  // From the account number input field

        if (string.IsNullOrEmpty(bankCode) || string.IsNullOrEmpty(accountNumber))
        {
            notificationText.text = "Please select a bank and enter an account number";
            yield break;
        }

        // Create the request payload as JSON
        var requestData = new
        {
            action = "validateAccount",
            account_number = accountNumber,
            bank_code = bankCode
        };
        string jsonPayload = JsonUtility.ToJson(requestData);

        // Create the UnityWebRequest with JSON body
        using (UnityWebRequest request = new UnityWebRequest(phpApiUrl, "POST"))
        {
            byte[] bodyRaw = new System.Text.UTF8Encoding().GetBytes(jsonPayload);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Error: " + request.error);
                notificationText.text = "Error validating account: " + request.error;
            }
            else
            {
                var responseText = request.downloadHandler.text;
                Debug.Log("Response: " + responseText);

                var response = JsonUtility.FromJson<AccountValidationResponse>(responseText);

                if (response.success)
                {
                    notificationText.text = $"Account Name: {response.account_name}";
                    withdrawButton.interactable = true;  // Enable the withdraw button
                }
                else
                {
                    notificationText.text = "Failed to validate account: " + response.message;
                }
            }
        }
    }

    IEnumerator WithdrawFunds()
    {
        string accountNumber = accountNumberInput.text;
        string withdrawAmountText = amountInput.text;

        if (string.IsNullOrEmpty(accountNumber) || string.IsNullOrEmpty(selectedBankCode) || string.IsNullOrEmpty(withdrawAmountText))
        {
            notificationText.text = "Please fill all fields.";
            yield break;
        }

        float withdrawAmount = float.Parse(withdrawAmountText);

        if (withdrawAmount > userBalance)
        {
            notificationText.text = "Insufficient balance.";
            yield break;
        }

        // Create the request payload as JSON
        var requestData = new
        {
            action = "withdrawAmount",
            account_number = accountNumber,
            bank_code = selectedBankCode,
            withdraw_amount = withdrawAmountText
        };
        string jsonPayload = JsonUtility.ToJson(requestData);

        // Create the UnityWebRequest with JSON body
        using (UnityWebRequest request = new UnityWebRequest(phpApiUrl, "POST"))
        {
            byte[] bodyRaw = new System.Text.UTF8Encoding().GetBytes(jsonPayload);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                notificationText.text = "Error: " + request.error;
            }
            else
            {
                string responseText = request.downloadHandler.text;
                Debug.Log("Response: " + responseText);

                var response = JsonUtility.FromJson<WithdrawResponse>(responseText);

                if (response.success)
                {
                    notificationText.text = response.message;
                    userBalance -= withdrawAmount;
                }
                else
                {
                    notificationText.text = "Withdrawal Failed: " + response.message;
                }
            }
        }
    }


    [System.Serializable]
    public class AccountValidationResponse
    {
        public bool success;
        public string account_name;
        public string message;
    }

    [System.Serializable]
    public class WithdrawResponse
    {
        public bool success;
        public string message;
    }
}
