using UnityEngine;
using TMPro;

public class ScriptLoader : MonoBehaviour
{
    public TMP_Text CountDown;
    public float timer = 5f;
    public GameObject ChessGameLoader;
    private bool isOn = true;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(isOn == true)
        {
            if (timer != 0)
            {
                timer -= 1 * Time.deltaTime;
            }
            if (timer <= 0)
            {
                isOn = false;
                timer = 0f;
            }
        }
        UpdateText();
    }

    public void UpdateText()
    {
        if(timer <= 5)
        {
            CountDown.color = Color.green;
            CountDown.text = timer.ToString("0");
        }
        if (timer <= 3)
        {
            CountDown.color = Color.yellow;
            CountDown.text = timer.ToString("0");
        }
        if (timer <= 2)
        {
            CountDown.color = Color.red;
            CountDown.text = timer.ToString("0");
        }
        if (timer <= 1)
        {
            CountDown.color = Color.yellow;
            CountDown.text = "Ready!";
            LoadObject();
        }
        if (timer <= 0)
        {
            CountDown.text = "";
        }
        
    }

    public void LoadObject()
    {
        ChessGameLoader.SetActive(true);
    }
}
