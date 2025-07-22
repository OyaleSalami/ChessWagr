using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class chatbot : MonoBehaviour
{
    public TMP_InputField userInputField;
    public GameObject userMessagePrefab; // Prefab for user messages
    public GameObject botReplyPrefab; // Prefab for bot replies
    public Transform responseParent; // Parent transform where responses will be spawned
    public GameObject TimeOBj;
    public VerticalLayoutGroup Resize;

    private Dictionary<string, Func<string>> customResponses = new Dictionary<string, Func<string>>();

    public void StartKEY()
    {
        DisplayResponseBot("System Ibot: " + GetRandomResponse(new string[] { "Greetings!\n Thank you for reaching out to our support center, please use the options below to proceed.\n\n 1: how can i help you today.\n 2: Press 1 to reach out to any of our support team.\n" }));
        InitializeCustomResponses();
    }

    private void InitializeCustomResponses()
    {
        customResponses.Add("how are you" + userInputField.text, () => GetRandomResponse(new string[] { "I'm doing well, thank you!", "Not too bad, how about you?", "Pretty good!", "Can't complain!", "I'm a chatbot, I don't have feelings!" }));
        customResponses.Add("hello" + userInputField.text, () => GetRandomResponse(new string[] { "I'm doing well, thank you!", "Not too bad, how about you?", "Pretty good!", "Can't complain!", "I'm a chatbot, I don't have feelings!" }));
        customResponses.Add("hi" + userInputField.text, () => GetRandomResponse(new string[] { "Am good and you", "hey, am good and you", "hello" }));
        customResponses.Add("hey" + userInputField.text, () => "Hi, how are you doing");
        customResponses.Add("1" + userInputField.text, () => "Press 5: how to withdraw. \nPress 6: how to deposite");
        customResponses.Add("2" + userInputField.text, () => "A support team will be here shortly, This might take sometime.\n Press 0: to proceed");
        customResponses.Add("0", () => { offiBot(); return "connecting...\n\n 1: please do not leave this page.\n 2: kindly state you complain and wait for respons."; });
        customResponses.Add("good" + userInputField.text, () => "Alright, how can i help you");
        customResponses.Add("i have a problem" + userInputField.text, () => "connecting... you to a support team");
        customResponses.Add("your name" + userInputField.text, () => "My name is System Ibot, and what your name");
        customResponses.Add("my name is" + userInputField.text, () => "wow, your name is really nice");
        customResponses.Add("where are you from" + userInputField.text, () => "I am a digital entity residing in the world of Chess Wage.");
        customResponses.Add("what do you like to do" + userInputField.text, () => "i like to teach you how to play.");
        customResponses.Add("how was your day" + userInputField.text, () => "As a digital entity, I do not experience days in the same way humans do.");
        customResponses.Add("Your Creator" + userInputField.text, () => "he goes by the name Fragman/Flacon he is my creator .");
    }

    public void offiBot()
    {
        StartCoroutine(closingChat());
    }
    private IEnumerator closingChat()
    {
        yield return new WaitForSeconds(3f);
        Motherboard.instance.SystemSendButton.SetActive(true);
        StartCoroutine(Motherboard.instance.LoadChatMessages());
        this.enabled = false;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.KeypadEnter) && userInputField.text.Length != 0)
        {
            OnUserInputEndEdit();
        }
        else if (Input.GetKeyDown(KeyCode.KeypadEnter) && userInputField.text.Length == 0)
        {
            DisplayResponseBot("FragMan Ibot: " + GetRandomResponse(new string[] { "Try typing something!", "Ask me a question about the game!", "You can't send that", "I'm off for today!" }));
        }
    }

    public void OnUserInputEndEdit()
    {
        if (userInputField.text.Length == 0)
        {
            DisplayResponseBot("FragMan Ibot: " + GetRandomResponse(new string[] { "Try typing something!", "Ask me a question about the game!", "You can't send that", "I'm off for today!" }));
        }
        else
        {
            string userMessage = userInputField.text;
            userInputField.text = "";

            DisplayResponseUser("User: " + userMessage);

            StartCoroutine(DelayedBotResponse(userMessage));
            Resize.childControlHeight = false;
            StartCoroutine(RefreshRate1());
        }
    }
    private IEnumerator RefreshRate1()
    {
        yield return new WaitForSeconds(0.1f);
        Resize.childControlHeight = true;
        Resize.childControlHeight = false;
    }

    IEnumerator DelayedBotResponse(string userMessage)
    {
        // Wait for a short delay before showing the bot response
        yield return new WaitForSeconds(2.5f);

        string response = GenerateResponse(userMessage);
        DisplayResponseBot("System Ibot: " + response);
    }
    private void DisplayResponseUser(string message)
    {
        // Instantiate the user message prefab and set the text
        GameObject responseObj = Instantiate(userMessagePrefab, responseParent);
        TMP_Text responseText = responseObj.transform.Find("ChatFrame").GetComponentInChildren<TMP_Text>();
        TMP_Text responseText1 = responseObj.transform.Find("Text_Ago").GetComponent<TMP_Text>();
        string DateTimes = DateTime.Now.ToString("yyyy:MM:dd--HH:mm:ss");
        responseText1.text = DateTimes;
        responseText.text = message;
    }
    private void DisplayResponseBot(string message)
    {
        // Instantiate the bot reply prefab and set the text
        GameObject responseObj = Instantiate(botReplyPrefab, responseParent);
        TMP_Text responseText = responseObj.transform.Find("ChatFrame").GetComponentInChildren<TMP_Text>();
        TMP_Text responseText1 = responseObj.transform.Find("Text_Ago").GetComponent<TMP_Text>();
        string DateTimes = DateTime.Now.ToString("yyyy:MM:dd--HH:mm:ss");
        responseText1.text = DateTimes;
        responseText.text = message;
        StartCoroutine(RefreshRate());
    }
    private IEnumerator RefreshRate()
    {
        Resize.childControlHeight = false;
        yield return new WaitForSeconds(0.1f);
        Resize.childControlHeight = true;
    }
    private string GetRandomResponse(string[] options)
    {
        int randomIndex = UnityEngine.Random.Range(0, options.Length);
        return options[randomIndex];
    }
    private string GenerateResponse(string userMessage)
    {
        userMessage = userMessage.ToLower();

        foreach (var kvp in customResponses)
        {
            if (userMessage.Contains(kvp.Key))
            {
                return kvp.Value.Invoke();
            }
        }

        return "I'm sorry, I didn't understand that.";
    }

    public void ClearChat()
    {
        Motherboard.instance.GlobalChat.gameObject.SetActive(false);
        // Clear the chat display
        foreach (Transform child in responseParent)
        {
            if (child.gameObject.name != "Text (TMP)")
            {
                Destroy(child.gameObject);

            }
        }
    }
}