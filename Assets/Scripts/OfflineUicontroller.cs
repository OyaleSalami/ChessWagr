using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OfflineUicontroller : MonoBehaviour
{

    public static OfflineUicontroller instance;

    public Button PlayerUi, BotUi;
    public float PlayerTime, BotTime;
    private float SetTime;
    public float Timmer = 90;
    public TMP_Text PlayerDisplayTime,BotDisplayTime;
    public TMP_Text PlayerTurnCount, BotTurnCount;
    public TMP_Text playername, BotName;
    public string PlayerName = "Player1", botname = "Bot";
    public int playercount, botcount;
    public GameObject Cam1, Cam2;

    [SerializeField]
    private bool playerTurn,botTurn;

    [SerializeField]
    private GameObject WinScreen;
    public Text Team;
    public TMP_Text Team1;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        instance = this;
        SetTime = Timmer;

        playername.text = PlayerName;
        BotName.text = botname;
    }

    private void Update()
    {
        //player
        if(playerTurn == true)
        {
            if(Cam1 != null || Cam2 != null)
            {
                Cam1.SetActive(true);
                Cam2.SetActive(false);
            }
            else
            {

            }

            PlayerTime -= 1 * Time.deltaTime;
            PlayerDisplayTime.text = "Time: " + PlayerTime.ToString("F1") + " Second";
            if(PlayerTime <= 0)
            {
                if(WinScreen != null)
                {
                    WinScreen.SetActive(true);
                    Team.text = "Black Team Wins!!";
                    Team1.text = "Black Team Wins!!";
                    //Display the Opponent Wins
                    //set timer to zero
                    PlayerTime = 0;
                    playerTurn = false;
                }
                else
                {
                    Debug.LogWarning("Empty fields on Winscerrn");
                }
            }
        }
        //bot
        if (botTurn == true)
        {
            if (Cam1 != null || Cam2 != null)
            {
                Cam1.SetActive(false);
                Cam2.SetActive(true);
            }
            else
            {

            }
            
            BotTime -= 1 * Time.deltaTime;
            BotDisplayTime.text = "Time: " + BotTime.ToString("F1") + " Second";
            if (BotTime <= 0)
            {
                if (WinScreen != null)
                {
                    WinScreen.SetActive(true);
                    Team.text = "White Team Wins!!";
                    Team1.text = "White Team Wins!!";
                    //Display the Opponent Wins
                    //set timer to zero
                    BotTime = 0;
                    botTurn = false;
                }
                else
                {
                    Debug.LogWarning("Empty fields on Winscerrn");
                }
            }
        }
    }

    public void PlayerTurn()
    {
        PlayerUi.interactable = true;
        BotUi.interactable = false;

        playerTurn = true;
        botTurn = false;

        BotTime = SetTime;

        playercount += 1;
        PlayerTurnCount.text = "Turn: " + playercount.ToString();
    }
    public void BotTurn()
    {
        BotUi.interactable = true;
        PlayerUi.interactable = false;

        botTurn = true;
        playerTurn = false;

        PlayerTime = SetTime;

        botcount += 1;
        BotTurnCount.text = "Turn: " + botcount.ToString();
    }
    public void AutoSwitch()
    {
        if(playerTurn == true)
        {
            BotTurn();
        }
        else if(botTurn == true)
        {
            PlayerTurn();
        }
    }
}
