using UnityEngine;
using UnityEngine.UI;
using System;
using System.Text;
using TMPro;

public class ToggleTextVisibility : MonoBehaviour
{
    public static ToggleTextVisibility instance;
    public TMP_InputField inputField;
    public Button toggleButton;

    public string encryptedText,reciver;
    public bool isHidden = false;

    void Start()
    {
        instance = this;
        if (toggleButton != null)
        {
            toggleButton.onClick.AddListener(ToggleVisibility);
        }
    }

    void Update()
    {
        reciver = Motherboard.instance.signPassword.text;
    }

    void ToggleVisibility()
    {
        if (isHidden)
        {
            string decryptedText = Decrypt(encryptedText);
            inputField.text = decryptedText;
            inputField.contentType = TMP_InputField.ContentType.Standard;
        }
        else
        {
            encryptedText = Encrypt(inputField.text);
            inputField.text = new string('*', inputField.text.Length);
            inputField.contentType = TMP_InputField.ContentType.Password;

            string decryptedText = Decrypt(encryptedText);
            reciver = decryptedText;
            
        }

        // Update the display
        inputField.ForceLabelUpdate();
        isHidden = !isHidden;
    }

    string Encrypt(string plainText)
    {
        byte[] plainTextBytes = Encoding.UTF8.GetBytes(plainText);
        return Convert.ToBase64String(plainTextBytes);
    }

    string Decrypt(string encryptedText)
    {
        byte[] encryptedTextBytes = Convert.FromBase64String(encryptedText);
        return Encoding.UTF8.GetString(encryptedTextBytes);
    }
}
