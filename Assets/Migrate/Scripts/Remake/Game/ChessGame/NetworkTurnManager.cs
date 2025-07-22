using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun.UtilityScripts;
using Photon.Realtime;
using TMPro;
using Photon.Pun;
using System.Linq;
using System.Collections;

public class NetworkTurnManager : PunTurnManager, IPunTurnManagerCallbacks
{

    #region Public Variables

    [Tooltip("the duration time of turn")]
    public float TurnTime = 120f;

    [Tooltip("Player Team color")]
    public TeamColor localPlayerType = TeamColor.None;
    public TeamColor currentPlayer = TeamColor.None;
    public bool _isWhiteTurn = true;

    public GameState gameState = GameState.None;

    public GameObject WinPanel;


    public struct step
    {
        public int movedType;
        public int killType;

        public int xFrom;
        public int yFrom;
        public int xTo;
        public int yTo;

        public string SpecialMove;

        public step(int _moveId, int _killId, int _xFrom, int _yFrom, int _xTo, int _yTo, string specialMove)
        {
            movedType = _moveId;
            killType = _killId;
            xFrom = _xFrom;
            yFrom = _yFrom;
            xTo = _xTo;
            yTo = _yTo;
            SpecialMove = specialMove;
        }
    }

    [Tooltip("Save the chess pieces steps")]
    public List<step> _steps = new List<step>();

    [Tooltip("Chess Game Controller")]
    public ChessGameController chessGameController;

    public static NetworkTurnManager Instance;

    #endregion

    #region Private Variables

    [Header("Game UIs")]
    [Tooltip("Game Ui View")]
    [SerializeField]
    private RectTransform GameUiView;

    [Tooltip("Button Canvas Group")]
    [SerializeField]
    private CanvasGroup ButtonCanvasGroup;

    [Tooltip("Disconnected Panel")]
    [SerializeField]
    private RectTransform DisconnectedPanel;

    [Tooltip("Request Panel")]
    [SerializeField]
    private RectTransform RequestPanel;

    [Header("Local player")]
    [Tooltip("Local player text")]
    [SerializeField]
    private TextMeshProUGUI LocalPlayerNameText;
    [Tooltip("LocalPlayer Time Text, Local Score Text, Local Turn Text")]
    [SerializeField]
    private TextMeshProUGUI LocalPlayerTimeText, LocalScoreText, LocalTurnText;
    [Tooltip("Local GameStatus Text")]
    [SerializeField]
    private TextMeshProUGUI LocalGameStatusText;

    [Header("Remote Player")]
    [Tooltip("Remote Player Name Text")]
    [SerializeField]
    private TextMeshProUGUI RemotePlayerNameText;

    [Tooltip("Remote Player Time Text, Remote Score Text, Remote Turn Text")]
    [SerializeField]
    private TextMeshProUGUI RemotePlayerTimeText, RemoteScoreText, RemoteTurnText;

    [Tooltip("Remote GameStatus Text")]
    [SerializeField]
    private TextMeshProUGUI RemoteGameStatusText;


    [Header("Picture sprite")]
    [Tooltip("Win Or Loss Image")]
    [SerializeField]
    private Image WinOrLossImage;

    [Tooltip("Sprite Win")]
    [SerializeField]
    private Sprite SpriteWin;

    [Tooltip("Sprite Lose")]
    [SerializeField]
    private Sprite SpriteLose;

    [Tooltip("Sprite Draw")]
    [SerializeField]
    private Sprite SpriteDraw;

    [Tooltip("Next turn")]
    [SerializeField]
    private Sprite Next;

    private ResultType result = ResultType.None;    // result

    private PunTurnManager turnManager; // pun turn manager

    private bool remoteSelection;   // remote player's Selection

    private bool IsShowingResults;	// same  mean as is finished the game

    // handling these two players are playing chess game in whole processing.
    private Player local;
    private Player remote;

    PhotonHandler photonHandler;

    #endregion

    public enum ResultType  // enum for result type
    {
        None = 0,
        Draw,
        LocalWin,
        LocalLoss
    }

    #region Mono Callbacks 


    public void Awake()
    {
        if (turnManager == null)
        {
            this.turnManager = this.gameObject.AddComponent<PunTurnManager>();  // add the component for pun turn manager
            this.turnManager.TurnManagerListener = this;    // listening all callback events 
            this.turnManager.TurnDuration = TurnTime;       // init the turn durationtime.
        }


        if (chessGameController == null)
            chessGameController = GameObject.FindObjectOfType<Board>().GetComponent<ChessGameController>();

        Instance = this;
        RefreshUIViews(); // refresh the in-game UI Views
        if (photonHandler == null)
            photonHandler = gameObject.AddComponent<PhotonHandler>();
        DisconnectedPanel.gameObject.SetActive(false);
    }

    private void RefreshUIViews()
    {
        if (GameUiView.gameObject.activeSelf)
            GameUiView.gameObject.SetActive(PhotonNetwork.InRoom);
        ButtonCanvasGroup.interactable = PhotonNetwork.CurrentRoom != null ? PhotonNetwork.CurrentRoom.PlayerCount > 1 : false;
        UpdatePlayerScoreTexts();
    }

    private void UpdateLocalPlayerInfo(string PlayerName, string PlayerStatus)
    {
        LocalPlayerNameText.text = "Player: " + PlayerName;
        LocalGameStatusText.text = PlayerStatus;
        LocalScoreText.text = "Scores: " + local.GetScore().ToString("D2");
        LocalTurnText.text = "Turn: " + PhotonNetwork.CurrentRoom.GetTurn().ToString();
    }

    private void UpdateRemotePlayerInfo(string PlayerName, string PlayerStatus)
    {
        RemotePlayerNameText.text = "Player: " + PlayerName;
        RemoteGameStatusText.text = PlayerStatus;
        RemoteScoreText.text = "Scores: " + remote.GetScore().ToString("D2");
        RemoteTurnText.text = "Turn: " + PhotonNetwork.CurrentRoom.GetTurn().ToString();
    }

    public void Update()
    {
        // check the are we lost the connection
        if (this.DisconnectedPanel == null)
        {
            Destroy(this.gameObject);
        }

        if (!PhotonNetwork.InRoom || turnManager == null)  // if not in room, stop the update
        {
            return;
        }

        // if pun is connected or is connecting, the reconnect panel would be disabled.
        if (PhotonNetwork.IsConnected && this.DisconnectedPanel.gameObject.activeSelf)
        {
            this.DisconnectedPanel.gameObject.SetActive(false);
        }

        if (!PhotonNetwork.IsConnected && !this.DisconnectedPanel.gameObject.activeSelf)
        {
            this.DisconnectedPanel.gameObject.SetActive(true);
        }

        if (PhotonNetwork.CurrentRoom.PlayerCount > 1)
        {
            if (this.turnManager.IsOver)
            {
                return; // end turn
            }

            if (this.turnManager.Turn > 0 && !IsShowingResults)
            {
                if (Input.GetKeyUp(KeyCode.K))
                {
                    this.turnManager.SendMove("Hello World!", false);

                }
                string remainTime = this.turnManager.RemainingSecondsInTurn.ToString("F1") + " Second";
                if (_isWhiteTurn)
                {
                    if (isWhitePlayer())
                    {
                        LocalPlayerTimeText.text = remainTime;
                        RemotePlayerTimeText.text = "00:00";
                    }
                    if (isBlackPlayer())
                    {
                        RemotePlayerTimeText.text = remainTime;
                        LocalPlayerTimeText.text = "00:00";
                    }
                }
                else
                {
                    if (isWhitePlayer())
                    {
                        RemotePlayerTimeText.text = remainTime;
                        LocalPlayerTimeText.text = "00:00";
                    }
                    if (isBlackPlayer())
                    {
                        LocalPlayerTimeText.text = remainTime;
                        RemotePlayerTimeText.text = "00:00";
                    }
                }
            }
        }

        InGeameUicontroller();
    }
    #endregion

    #region public methods
    public void Judge()
    {
        if (_steps.Count > 0) // prevent empty step in list
        {
            if (chessGameController.getBoard().CheckForCheckmate())
            {
                JudgeWinner();
            }

            if (chessGameController.getBoard().CheckStalemate())
            {
                SetResultDrawAndTexts("StaleMate Draw!");
            }
        }

        if (_steps.Count == 0)
        {
            SetResultDrawAndTexts("No Any Moves, Both Draw!");

        }
    }


    public void JudgeWinner()
    {
        // Get the team that made the last move
        TeamColor lastMoveTeam = getLastMoveTeamColor();
        
        // Determine the winner (opposite of last move team)
        bool whiteWins = lastMoveTeam == TeamColor.Black;
        bool localPlayerWins = (whiteWins && isWhitePlayer()) || (!whiteWins && isBlackPlayer());
        
        // Set appropriate result
        result = localPlayerWins ? ResultType.LocalWin : ResultType.LocalLoss;
        
        // Reset the playing state
        if (VictoryManager.instance.playing == 1)
            VictoryManager.instance.playing = 0;
        
        // Set the winner message based on which team won
        string winMessage = whiteWins ? "White Win !" : "Black Win !";
        LocalGameStatusText.text = winMessage;
        
        // Handle the appropriate UI for the local player
        if (localPlayerWins)
        {
            // Show victory panel for the winner
            VictoryManager.instance.ShowVictoryPanel(winMessage);
            VictoryManager.instance.active = true;
            
            // Calculate rewards using a common method
            CalculateAndDisplayReward();
        }
        else
        {
            // Show defeat panel for the loser
            VictoryManager.instance.ShowDefeatPanel(winMessage);
            VictoryManager.instance.active = true;
            VictoryManager.instance.winnerDescriptionText.text = "Great game!\nYou played well, but next time, I'll be ready for an even tougher challenge.\nLet's see who can outsmart the other!";
        }
    }

    // Helper method to calculate and display rewards
    private void CalculateAndDisplayReward()
    {
        float multiplier = 2f;  // This could be made configurable
        float wagerAmount = Motherboard.instance.MyWage;
        float totalWinAmount = multiplier * wagerAmount;
        float platformFee = totalWinAmount * 0.1f; // 10% fee
        float playerReward = totalWinAmount - platformFee;
        
        // Display the reward
        VictoryManager.instance.WageCost.text = "+ " + playerReward.ToString("F2");
        
        // Update the player's balance
        Motherboard.instance.SageGame(playerReward, platformFee);
    }

    #endregion

    #region TurnManager Callbacks
    public void OnTurnBegins(int turn)
    {
        Debug.Log("OnTurnBegins() turn: " + turn);
        this.LocalTurnText.text = "Turn: " + (this.turnManager.Turn).ToString();
        RemoteTurnText.text = LocalTurnText.text;
        this.WinOrLossImage.gameObject.SetActive(false); // invisible the win or loss image
        IsShowingResults = false;   // disable the result
        ButtonCanvasGroup.interactable = true; // switch the buttons?
    }
    public void OnTurnCompleted(int turn)
    {
        this.Judge();   // check the result
        this.UpdateScores();    // update scores
        this.OnEndTurn();	// finish this turn
    }



    public void OnPlayerMove(Player player, int turn, object move)
    {
        Debug.Log("OnPlayerMove: " + player + " turn: " + turn + " action: " + move);
        string strMove = move.ToString();

        if (strMove.Contains("_"))
        {

            if (!player.IsLocal)
            {
                string[] strArr = strMove.Split(char.Parse("_"));
                if (int.Parse(strArr[6]) == (int)SpecialMove.Promotion)
                {
                    chessGameController.MovePieceTo(
                        int.Parse(strArr[0]), int.Parse(strArr[1]),
                        int.Parse(strArr[2]), int.Parse(strArr[3]),
                        int.Parse(strArr[4]), int.Parse(strArr[5])
                        );

                }
            }
        }
        else
        {
            switch (strMove)
            {
                case "Restart":
                    if (!player.IsLocal)
                        PopRequest("Your opponent want to Restart the game");
                    break;
                case "RestartYes":
                    Restart();
                    break;
                case "RestartNo":
                    LocalGameStatusText.text = "Your oppenent reject your request";
                    break;
            }
        }
    }

    public void OnPlayerFinished(Player player, int turn, object move)
    {
        Debug.Log("OnTurnFinished: " + player.ToString() + " turn: " + turn + " action: " + move);
        string tmpStr = move.ToString();

        if (tmpStr.Contains("_"))
        {
            if (!player.IsLocal) // Update the opponent move.
            {
                string[] strArr = tmpStr.Split(char.Parse("_"));
                
                // Handle promotion
                if (strArr[0] == "+Promotion")
                {
                    // Format: [0]: "+Promotion", [1]: PieceNewType;
                    try {
                        chessGameController.getBoard().pawnPromotion((ChessPieceType)(int.Parse(strArr[1])));
                    }
                    catch (System.Exception e) {
                        Debug.LogError("Error processing promotion: " + e.Message);
                    }
                }
                else if (strArr.Length >= 7) // Ensure we have enough parameters
                {
                    // Try-catch to avoid crashes on malformed messages
                    try {
                        chessGameController.MovePieceTo(
                            int.Parse(strArr[0]), 
                            int.Parse(strArr[1]),
                            int.Parse(strArr[2]), 
                            int.Parse(strArr[3]),
                            int.Parse(strArr[4]), 
                            int.Parse(strArr[5]),
                            int.Parse(strArr[6])
                        );
                        
                        // Switch turn after successful move
                        SwitchPlayerTurn();
                        
                        // Check for checkmate/stalemate after each move
                        if (_steps.Count > 0)
                        {
                            if (chessGameController.getBoard().CheckForCheckmate())
                            {
                                JudgeWinner();
                            }
                            else if (chessGameController.getBoard().CheckStalemate())
                            {
                                SetResultDrawAndTexts("Stalemate - Draw!");
                            }
                        }
                    }
                    catch (System.Exception e) {
                        Debug.LogError("Error processing move: " + e.Message);
                    }
                }
            }
        }
        else
        {
            switch (tmpStr)
            {
                case "BlackDefeat":
                    RemoteGameStatusText.text = "Black lose!";
                    if (isWhitePlayer())
                    {
                        result = ResultType.LocalWin;

                        if (VictoryManager.instance.playing == 1)
                            VictoryManager.instance.playing = 0;
                        VictoryManager.instance.ShowVictoryPanel("Black Win !");
                        VictoryManager.instance.active = true;
                        VictoryManager.instance.winnerDescriptionText.text = "The player surrendered";

                        float value = 2f;  // Example value
                        float multipliedValue = value * Motherboard.instance.MyWage;  // Multiply by the wage
                        // Calculate the 10% (the amount to subtract)
                        float tenPercent = multipliedValue * 0.1f;
                        // Subtract 10% to get WageWon
                        float WageWon = multipliedValue - tenPercent;  // WageWon is 90% of multipliedValue
                        VictoryManager.instance.WageCost.text = "+ " + WageWon;
                        // Store the 10% in PerGameMake
                        float PerGameMake = tenPercent;  // The 10% value
                        // Now call SageGame method with WageWon and PerGameMake
                        Motherboard.instance.SageGame(WageWon, PerGameMake);
                    }
                    else if (isBlackPlayer())
                    {
                        result = ResultType.LocalLoss;

                        if (VictoryManager.instance.playing == 1)
                            VictoryManager.instance.playing = 0;
                        VictoryManager.instance.ShowDefeatPanel("Black Win !");
                        VictoryManager.instance.active = true;
                        VictoryManager.instance.winnerDescriptionText.text = "You surrender to your opponent!";

                    }
                    break;

                case "WhiteDefeat":
                    RemoteGameStatusText.text = "White lose!";
                    if (isBlackPlayer())
                    {
                        result = ResultType.LocalWin;

                        if (VictoryManager.instance.playing == 1)
                            VictoryManager.instance.playing = 0;
                        VictoryManager.instance.ShowVictoryPanel("White Win !");
                        VictoryManager.instance.winnerDescriptionText.text = "The player surrendered";

                        float value = 2f;  // Example value
                        float multipliedValue = value * Motherboard.instance.MyWage;  // Multiply by the wage
                        // Calculate the 10% (the amount to subtract)
                        float tenPercent = multipliedValue * 0.1f;
                        // Subtract 10% to get WageWon
                        float WageWon = multipliedValue - tenPercent;  // WageWon is 90% of multipliedValue
                        VictoryManager.instance.WageCost.text = "+ " + WageWon;
                        // Store the 10% in PerGameMake
                        float PerGameMake = tenPercent;  // The 10% value
                        // Now call SageGame method with WageWon and PerGameMake
                        Motherboard.instance.SageGame(WageWon, PerGameMake);

                    }
                    else if (isWhitePlayer())
                    {
                        result = ResultType.LocalLoss;

                        if (VictoryManager.instance.playing == 1)
                            VictoryManager.instance.playing = 0;
                        VictoryManager.instance.ShowDefeatPanel("White Win !");
                        VictoryManager.instance.active = true;
                        VictoryManager.instance.winnerDescriptionText.text = "You surrender to your opponent!";

                    }
                    break;

                case "Draw":
                    result = ResultType.Draw;

                    if (VictoryManager.instance.playing == 1)
                        VictoryManager.instance.playing = 0;
                    VictoryManager.instance.ShowVictoryPanel("Draw");
                    VictoryManager.instance.active = true;
                    VictoryManager.instance.winnerDescriptionText.text = "Draw";


                    float WageWon1 = Motherboard.instance.balance -= Motherboard.instance.MyWage;  // Step 1: Multiply by 100
                    VictoryManager.instance.WageCost.text = "- " + WageWon1;
                    float drawncash = 0;
                    Motherboard.instance.SageGame(WageWon1, drawncash);
                    break;

                default:
                    break;
            }
        }
    }


    public void OnTurnTimeEnds(int turn)
    {
        if (!IsShowingResults)
        {
            // Determine winner based on whose turn timed out
            bool localPlayerLost = (_isWhiteTurn && isWhitePlayer()) || (!_isWhiteTurn && isBlackPlayer());
            result = localPlayerLost ? ResultType.LocalLoss : ResultType.LocalWin;
            
            string timeoutMessage = localPlayerLost ? "Your time is over" : "Your opponent's time is over";
            
            // Update game status texts
            if (localPlayerLost)
            {
                LocalGameStatusText.text = "Your time is over";
                RemoteGameStatusText.text = "Victory";
                
                // Show defeat panel
                VictoryManager.instance.ShowDefeatPanel(timeoutMessage);
            }
            else
            {
                LocalGameStatusText.text = "Your opponent's time is over";
                RemoteGameStatusText.text = "Defeat!";
                
                // Show victory panel
                VictoryManager.instance.ShowVictoryPanel(timeoutMessage);
                
                // Calculate and display reward
                CalculateAndDisplayReward();
            }
            
            // Reset playing state
            if (VictoryManager.instance.playing == 1)
                VictoryManager.instance.playing = 0;
            
            VictoryManager.instance.active = true;
            
            // Complete the turn
            OnTurnCompleted(-1);
        }
    }

    #region Core game play methods

    public Button TurnWhite, TurnBlack, homeButton, SurrenderButton;
    public void StartTurn()
    {
        gameState = GameState.Playing;

        if (PhotonNetwork.IsMasterClient)
        {
            this.turnManager.BeginTurn();
            print("Begin turn: " + this.turnManager.Turn);
        }

        this.turnManager.isTurnStarted = true;
    }


    public void InGeameUicontroller()
    {
        // Only update UI if we're in an active game
        if (gameState != GameState.Playing && gameState != GameState.Init)
            return;
        
        // Determine if it's the local player's turn
        bool isLocalPlayerTurn = (_isWhiteTurn && isWhitePlayer()) || (!_isWhiteTurn && isBlackPlayer());
        
        // Set button interactivity based on whose turn it is
        homeButton.interactable = isLocalPlayerTurn;
        SurrenderButton.interactable = isLocalPlayerTurn;
        
        // Set turn buttons appropriately
        if (isWhitePlayer())
        {
            TurnWhite.interactable = _isWhiteTurn;
            TurnBlack.interactable = !_isWhiteTurn;
        }
        else if (isBlackPlayer())
        {
            TurnWhite.interactable = !_isWhiteTurn;
            TurnBlack.interactable = _isWhiteTurn;
        }
        
        // Update turn status text
        UpdateTurnIndicators();
    }

    // New method to update turn indicators
    private void UpdateTurnIndicators()
    {
        // This will be called after switching turns to update all UI elements
        if (_isWhiteTurn)
        {
            if (isWhitePlayer())
            {
                LocalGameStatusText.text = "Your Turn!";
                RemoteGameStatusText.text = "Waiting...";
            }
            else if (isBlackPlayer())
            {
                LocalGameStatusText.text = "Opponent's Turn!";
                RemoteGameStatusText.text = "Thinking...";
            }
        }
        else
        {
            if (isBlackPlayer())
            {
                LocalGameStatusText.text = "Your Turn!";
                RemoteGameStatusText.text = "Waiting...";
            }
            else if (isWhitePlayer())
            {
                LocalGameStatusText.text = "Opponent's Turn!";
                RemoteGameStatusText.text = "Thinking...";
            }
        }
    }

    private void OnEndTurn()
    {
        ButtonCanvasGroup.interactable = false; // disable buttons switch
        IsShowingResults = true;
        this.turnManager.isTurnStarted = false;
        switch (result) // show the result image
        {
            case ResultType.None:
                this.StartTurn();
                break;
            case ResultType.Draw:
                this.WinOrLossImage.sprite = this.SpriteDraw;
                break;
            case ResultType.LocalWin:
                this.WinOrLossImage.sprite = this.SpriteWin;
                break;
            case ResultType.LocalLoss:
                this.WinOrLossImage.sprite = this.SpriteLose;
                break;
        }
        if (result != ResultType.None)
        {
            this.WinOrLossImage.gameObject.SetActive(true);
            chessGameController.getBoard().isGameStart = false;
            gameState = GameState.Finished;
            this.WinPanel.SetActive(true);
        }
        else
        {
            ButtonCanvasGroup.interactable = true;
        }

    }
    #endregion

    public void EndGame()
    {
        Debug.Log("EndGame");
        Application.Quit();
    }
    #endregion
    #region button event handle
    public void OnClickConnect()
    {
        PhotonNetwork.ConnectUsingSettings();
        photonHandler.StopFallbackSendAckThread(); // this is used in the demo to timeout in background!
    }
    public void OnClickReConnectAndRejoin()
    {
        PhotonNetwork.ReconnectAndRejoin();
        photonHandler.StopFallbackSendAckThread();  // this is used in the demo to timeout in background!
    }
    public void OnDefeat()
    {
        result = ResultType.LocalLoss;
        if (isBlackPlayer())
        {
            this.turnManager.SendMove("BlackDefeat", true);
        }
        if (isWhitePlayer())
        {
            this.turnManager.SendMove("WhiteDefeat", true);
        }
    }

    #endregion
    private void UpdateScores()
    {
        if (this.result == ResultType.LocalWin)
        {
            PhotonNetwork.LocalPlayer.AddScore(1);
            UpdatePlayerScoreTexts();
        }
    }
    private void UpdatePlayerScoreTexts()
    {
        if (local != null)
        {
            // ToString("D2") formate should be: "00"
            UpdateLocalPlayerInfo(local.NickName, LocalGameStatusText.text);
        }
        if (remote != null)
        {
            UpdateRemotePlayerInfo(remote.NickName, LocalGameStatusText.text);
        }

    }
    void PopRequest(string title)
    {
        if (RequestPanel == null && RequestPanel.gameObject.activeSelf)
        {
            return;
        }
        else
        {
            Debug.Log("PopRequest()");
            RequestPanel.gameObject.SetActive(true);
            // RequestPanel.transform.Find("Title/Text").GetComponent<Text>().text = title;
        }

    }


    #region Pun Call backs
    public override void OnLeftRoom()
    {
        Debug.Log("OnLeftRoom() via networkTurnManager.cs");
        if (VictoryManager.instance.playing == 1)
        {
            GameObject Main = Instantiate(Motherboard.instance.NotifyUser, Motherboard.instance.SpawnPoint.transform);
            TMP_Text TextTopic = Main.transform.Find("Topic").GetComponent<TMP_Text>();
            TextTopic.text = "server timeout.\nyou have lost connections, returning to lobby";
            Motherboard.instance.LoadingScreen.SetActive(true);

            float value = 2f;  // Example value
            float multipliedValue = value * Motherboard.instance.MyWage;  // Multiply by the wage
            // Calculate the 10% (the amount to subtract)
            float tenPercent = multipliedValue * 0.1f;
            // Subtract 10% to get WageWon
            float WageWon = multipliedValue - tenPercent;  // WageWon is 90% of multipliedValue
            VictoryManager.instance.WageCost.text = "+ " + WageWon;
            // Store the 10% in PerGameMake
            float PerGameMake = tenPercent;  // The 10% value
            // Now call SageGame method with WageWon and PerGameMake
            Motherboard.instance.SageGame(WageWon, PerGameMake);
        }
        else if (VictoryManager.instance.playing == 0)
        {
            GameObject Main = Instantiate(Motherboard.instance.NotifyUser, Motherboard.instance.SpawnPoint.transform);
            TMP_Text TextTopic = Main.transform.Find("Topic").GetComponent<TMP_Text>();
            TextTopic.text = "room has ended\nreturning to lobby";
            Motherboard.instance.LoadingScreen.SetActive(true);
        }

        base.OnLeftRoom();
        RefreshUIViews();
        chessGameController.InitChessInBoard();
        gameState = GameState.None;

    }

    public override void OnJoinedRoom()
    {
        local = PhotonNetwork.LocalPlayer;
        remote = PhotonNetwork.LocalPlayer.GetNext();
        gameState = GameState.Room;


        if (remote != null)
        {
            RemoteGameStatusText.text = "Matched sucessfully!";
            // the format should be: "name        00"
            this.RemotePlayerNameText.text = remote.NickName;
        }
        else
        {
            this.RemotePlayerNameText.text = "Matching";
            LocalGameStatusText.text = "Waiting another one...";
            RemoteGameStatusText.text = "Matching";
        }

        RefreshUIViews();
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.Log("Disconnected with cause: " + cause.ToString());
        
        // Show disconnection panel
        this.DisconnectedPanel.gameObject.SetActive(true);
        
        // Try to handle specific disconnect causes
        switch (cause)
        {
            case DisconnectCause.ServerTimeout:
            case DisconnectCause.ClientTimeout:
                GameObject timeoutNotification = Instantiate(Motherboard.instance.NotifyUser, Motherboard.instance.SpawnPoint.transform);
                TMP_Text timeoutText = timeoutNotification.transform.Find("Topic").GetComponent<TMP_Text>();
                timeoutText.text = "Connection timed out.\nTrying to reconnect...";
                
                // Try to automatically reconnect after a timeout
                StartCoroutine(AttemptReconnect());
                break;
                
            case DisconnectCause.DisconnectByServerLogic:
            case DisconnectCause.Exception:
            case DisconnectCause.ExceptionOnConnect:
                // More severe issues - inform the user
                GameObject errorNotification = Instantiate(Motherboard.instance.NotifyUser, Motherboard.instance.SpawnPoint.transform);
                TMP_Text errorText = errorNotification.transform.Find("Topic").GetComponent<TMP_Text>();
                errorText.text = "Connection error.\nPlease try again later.";
                break;
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        //Debug.Log("Other player arrived");
        RemoteGameStatusText.text = "Welcome, " + newPlayer.NickName + " joined this game!";
        if (remote == null)
        {
            remote = newPlayer;
        }
        RefreshUIViews();
        // play();
    }
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        //Debug.Log("Other player disconnected! isInactive: " + otherPlayer.IsInactive);
        RemoteGameStatusText.text = "Player: " + otherPlayer.NickName + " has left this room.";
        
        // Only handle opponent disconnection if the game is in progress and we have moves in the game
        if (!otherPlayer.IsLocal && !IsShowingResults && _steps.Count > 0)
        {
            // Set result to local win since opponent disconnected
            result = ResultType.LocalWin;
            
            if (VictoryManager.instance.playing == 1)
            {
                // Update UI to show victory due to opponent disconnection
                VictoryManager.instance.ShowVictoryPanel("Opponent disconnected");
                VictoryManager.instance.active = true;
                VictoryManager.instance.winnerDescriptionText.text = "Your opponent has left the game.";

                // Award the win to the remaining player
                CalculateAndDisplayReward();
            }
            else if (VictoryManager.instance.playing == 0)
            {
                // If we're not in play mode, just notify about room ending
                GameObject Main = Instantiate(Motherboard.instance.NotifyUser, Motherboard.instance.SpawnPoint.transform);
                TMP_Text TextTopic = Main.transform.Find("Topic").GetComponent<TMP_Text>();
                TextTopic.text = "Room ended\nRoom has closed, returning to lobby";
                Motherboard.instance.LoadingScreen.SetActive(true);
            }
            
            // Update scores due to opponent disconnection
            UpdateScores();
        }
        
        // Clear remote player reference
        if (remote != null && remote.ActorNumber == otherPlayer.ActorNumber)
            remote = null;
        
        // Refresh UI to reflect player left
        RefreshUIViews();
    }


    #endregion

    #region Controll the Players And UI
    public void CreateLocalPlayer()
    {
        // Before call Play() method
        // when local player has the customeProperties about the team -> local.CustomProperties[PlayerProperties.Team]

        TeamColor LocalTeamColor = (TeamColor)getPlayerTeamColor(local);
        if (LocalTeamColor != TeamColor.None)
        {
            localPlayerType = LocalTeamColor;
            string teamString = isWhitePlayer() ? "White" : "Black";
            string PlayerName = string.Format("{0} :({1})", local.NickName, teamString);
            string LocalGameStatus = string.Format("You is {0} team...", teamString);
            UpdateLocalPlayerInfo(PlayerName, LocalGameStatus);
        }
        else
        {
            Debug.Log("Team color is None, cannot create the Local Player");
        }

    }

    void Play()
    {
        print("Play() methods");
        VictoryManager.instance.playing = 1;

        InitGameBoard();

        // init UI info
        GameUIController.Instance.OnStartGame((CameraAngel)((int)localPlayerType)); // change the camera view
        LocalGameStatusText.text = "Start Game!";
        RemoteGameStatusText.text = "Start Game!";
        UpdatePlayerScoreTexts();
        RefreshUIViews();

        StartCoroutine("MatchSuccessfully");
    }
    IEnumerator MatchSuccessfully()
    {
        yield return new WaitForSeconds(2.5f);
        this.StartTurn();
    }
    public void SwitchPlayerTurn()
    {
        // Toggle turn flag
        _isWhiteTurn = !_isWhiteTurn;
        
        // Update current player based on turn
        currentPlayer = _isWhiteTurn ? TeamColor.White : TeamColor.Black;
        
        // Update UI to reflect current turn
        UpdateTurnIndicators();
    }
    public void StartGame()
    {
        GameUiView.gameObject.SetActive(true);
        // Create Local player -> check the conditions -> setup the time and Board -> Start the Game;
        if (isFullPlayers() && isZeroTurn())
        {
            // when two players, triggers play() methods
            SetRoundTime();
            CreateLocalPlayer();
            Play();
        }

        if (PhotonNetwork.CurrentRoom.PlayerCount > 2) // For guest
        {
            localPlayerType = TeamColor.None;
            LocalGameStatusText.text = "The game has started..";
        }
    }
    private void Restart()
    {
        if (PhotonNetwork.IsMasterClient && PhotonNetwork.CurrentRoom.PlayerCount == 2)
        {
            this.turnManager.RestartTurn();
            _isWhiteTurn = true;

        }
        localPlayerType = (TeamColor)getPlayerTeamColor(local);
        currentPlayer = TeamColor.White;
        chessGameController.RestartGame();
        CleanAllStep();
        RefreshUIViews();
    }
    public void InitGameBoard()
    {
        gameState = GameState.Init;
        // init player info
        _isWhiteTurn = true;
        result = ResultType.None;
        currentPlayer = TeamColor.White;
        CleanAllStep();

        chessGameController.RestartGame();
    }

    public void OnClickExitButton()
    {
        if (PhotonNetwork.InRoom)
        {
            // OnDefeat();
            PhotonNetwork.LeaveRoom(false);
            Debug.Log("On Click the exit button in Game");

            if (VictoryManager.instance.playing == 1)
            {
                VictoryManager.instance.playing = 0;
                VictoryManager.instance.HideVictoryPanel();
                Motherboard.instance.LoadingScreen.SetActive(true);
            }
            else if (VictoryManager.instance.playing == 0)
            {
                VictoryManager.instance.playing = 0;
                VictoryManager.instance.HideVictoryPanel();
            }
        }
    }
    #endregion
    public void SaveStep(int movePieceTypeNumber, int killPieceTypeNumber, int fromX, int fromY, int toX, int toY, string specialMove)
    {
        step tmpStep = new step();

        tmpStep.movedType = movePieceTypeNumber;
        tmpStep.killType = killPieceTypeNumber;
        tmpStep.xFrom = fromX;
        tmpStep.yFrom = fromY;
        tmpStep.xTo = toX;
        tmpStep.yTo = toY;
        tmpStep.SpecialMove = specialMove;

        _steps.Add(tmpStep);

    }

    public void CleanAllStep()
    {
        if (_steps != null)
            _steps.Clear();
    }
    public TeamColor getLastMoveTeamColor()
    {
        int x = _steps.Last().xTo,
            y = _steps.Last().yTo;

        return chessGameController.getBoard().chessPieces[x, y].team == 0 ? TeamColor.White : TeamColor.Black;
    }

    public bool isZeroTurn() => this.turnManager.Turn == 0;
    public bool isFullPlayers() => PhotonNetwork.CurrentRoom.PlayerCount == 2;

    public bool isMyturn() => localPlayerType == currentPlayer;
    public bool isWhitePlayer() => localPlayerType == TeamColor.White;
    public bool isBlackPlayer() => localPlayerType == TeamColor.Black;
    public bool isPlayingGame()
    {
        return gameState == GameState.Init || gameState == GameState.Playing;
    }
    public int getPlayerTeamColor(Player player)
    {
        if (player != null || player.CustomProperties.ContainsKey(PlayerProperties.Team))
        {
            return (int)player.CustomProperties[PlayerProperties.Team];
        }
        return (int)TeamColor.None;
    }
    public int getRoundTimeInRoom() => (int)PhotonNetwork.CurrentRoom.CustomProperties[RoomProperty.RoundTime] * 60;
    public void SetRoundTime()
    {
        // Before start game, setup the round time for each turn
        // (x) * 60 seconds, x is according to Room Property
        this.turnManager.TurnDuration = getRoundTimeInRoom();
    }
    public void SetResultDrawAndTexts(string messageText)
    {
        // Set the result to draw and update UI for both players
        result = ResultType.Draw;
        LocalGameStatusText.text = messageText;
        RemoteGameStatusText.text = messageText;
        
        // Display draw UI
        if (VictoryManager.instance.playing == 1)
            VictoryManager.instance.playing = 0;
        
        VictoryManager.instance.ShowVictoryPanel("Draw");
        VictoryManager.instance.active = true;
        VictoryManager.instance.winnerDescriptionText.text = messageText;
        
        // Handle draw reward logic (players get their wager back)
        float returnedWager = Motherboard.instance.MyWage;
        VictoryManager.instance.WageCost.text = "+ " + returnedWager.ToString("F2") + " (Returned Wager)";
        
        // Return the wager to the player
        Motherboard.instance.SageGame(returnedWager, 0);
        
        // Mark the game as finished
        IsShowingResults = true;
    }
    #region Handling the send move Messages

    public void OnMovingPiece(int movePieceType, int killPieceType, int originalX, int originalY, int x, int y, int specialMove)
    {
        bool isSpecialMove = specialMove != (int)SpecialMove.None;
        bool isPromotionEvent = (specialMove == (int)SpecialMove.Promotion);

        string tmpStr = getMessageFormat(movePieceType, killPieceType, originalX, originalY, x, y);

        tmpStr += "_" + specialMove.ToString();
        if (isSpecialMove)
        {
            this.turnManager.SendMove(tmpStr, !isPromotionEvent);
        }
        else
        {
            this.turnManager.SendMove(tmpStr, true);
        }

    }

    public void OnPromotionPiece(string PieceNewType)
    {
        string tmpStr = string.Format("{0}_{1}", "+Promotion", PieceNewType);
        this.turnManager.SendMove(tmpStr, true);
    }

    public string getMessageFormat(int movePieceType, int killPieceType, int originalX, int originalY, int x, int y)
    {
        string tmpStr = string.Format("{0}_{1}_{2}_{3}_{4}_{5}",
               movePieceType.ToString(), killPieceType.ToString(),
               originalX.ToString(), originalY.ToString(),
               x.ToString(), y.ToString());

        return tmpStr;
    }
    #endregion

    // New method to attempt reconnection
    private IEnumerator AttemptReconnect()
    {
        // Wait a short time before attempting reconnection
        yield return new WaitForSeconds(2f);
        
        // Try to reconnect
        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.ReconnectAndRejoin();
            photonHandler.StopFallbackSendAckThread();
        }
        
        // If still not connected after a few seconds, give up
        yield return new WaitForSeconds(5f);
        
        if (!PhotonNetwork.IsConnected)
        {
            // If reconnection failed, return to lobby
            GameObject failedNotification = Instantiate(Motherboard.instance.NotifyUser, Motherboard.instance.SpawnPoint.transform);
            TMP_Text failedText = failedNotification.transform.Find("Topic").GetComponent<TMP_Text>();
            failedText.text = "Reconnection failed.\nReturning to lobby.";
            
            // Show loading screen before returning to lobby
            Motherboard.instance.LoadingScreen.SetActive(true);
        }
    }

}
