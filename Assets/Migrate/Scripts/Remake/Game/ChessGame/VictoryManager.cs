using UnityEngine;
using TMPro;
using UnityEngine.UI; // Ensure you're using TMPro for TextMesh Pro

public class VictoryManager : MonoBehaviour
{
    public static VictoryManager instance;

    public GameObject victoryPanel; // Assign the victory panel in the Inspector
    public GameObject Victory,DefeatPanel; // Assign the victory panel in the Inspector
    public TMP_Text winnerNameText; // Assign the TextMeshPro text component for the winner's name
    public TMP_Text winnerDescriptionText, WageCost;
    public TMP_Text CountDown;
    public Button MenuButton;
    float timmer = 3;
    public bool active;
    public int playing;
    public float Win;
    public float rating;
    public float lostmatch;
    public float AdminProfitperGame;

    private void Start()
    {
        instance = this;
    }

    public void Update()
    {
        if(active == true)
        {
            timmer -= 1 * Time.deltaTime;

            if (DefeatPanel.activeInHierarchy == true)
            {
                MenuButton.interactable = true;
                TMP_Text menubutton = MenuButton.GetComponentInChildren<TMP_Text>();
                menubutton.text = "Menu in " + "<#636363>" + timmer.ToString("0");
            }
            else if (Victory.activeInHierarchy == true)
            {
                MenuButton.interactable = false;
                TMP_Text menubutton = MenuButton.GetComponentInChildren<TMP_Text>();
                menubutton.text = "Menu in " + "<#636363>" + timmer.ToString("0");
            }

            if(timmer <= 0)
            {
                active = false;
                timmer = 3.0f;
                MenuButton.interactable = true;
                if (DefeatPanel.activeInHierarchy == true)
                {
                    NetworkTurnManager.Instance.OnClickExitButton();
                }
            }
        }
    }

    // Call this method when the game ends, passing the winner's name as a parameter
    public void ShowVictoryPanel(string winnerName)
    {
        if (victoryPanel != null && winnerNameText != null)
        {
            winnerNameText.text = winnerName;
            winnerDescriptionText.text = "";
            WageCost.text = "";
            WageCost.gameObject.SetActive(true);
            victoryPanel.SetActive(true);
            DefeatPanel.SetActive(false);
            Victory.SetActive(true);
            Win = 1;
            rating = 100;

            if(Motherboard.instance.INTOURNAMENT == true)
            {
                Motherboard.instance.UpdateTournamentWin();
                Motherboard.instance.won = true;
            }

            //Play Victory sound
            StartUP.instance.VictorySound();
        }
    }
    public void ShowDefeatPanel(string winnerName)
    {
        if (victoryPanel != null && winnerNameText != null)
        {
            winnerNameText.text = winnerName;
            winnerDescriptionText.text = "";
            WageCost.text = "";
            WageCost.gameObject.SetActive(true);
            victoryPanel.SetActive(true);
            DefeatPanel.SetActive(true);
            Victory.SetActive(false);

            float WageWon = 0;
            WageWon = Motherboard.instance.MyWage;
            WageCost.text = "- " + WageWon;
            lostmatch = 1;

            if (Motherboard.instance.INTOURNAMENT == true)
            {
                Motherboard.instance.UpdateTournamentWin();
                Motherboard.instance.won = false;
            }

            LobbyPanelController.instance.YesButton.interactable = true;
            LobbyPanelController.instance.Refund.interactable = false;
            Motherboard.instance.MyWage = 0;

            Win = 0;

            StartUP.instance.DefeatSound();
        }
    }

    // Call this method to hide the victory panel, if needed
    public void HideVictoryPanel()
    {
        victoryPanel.SetActive(false); // Disable the victory panel
    }
}
