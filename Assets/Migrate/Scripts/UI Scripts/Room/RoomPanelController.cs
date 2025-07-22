
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using System;
using System.Collections.Generic;
using System.Linq;

public class RoomPanelController : MonoBehaviourPunCallbacks
{
    public static RoomPanelController instance;
    public TextMeshProUGUI roomName;
    public Transform whiteTeamPanel;
    public Transform blackTeamPanel;
    public TeamMemberItem[] Team1;
    public TeamMemberItem[] Team2;
    public Button backButton;

    public TextMeshProUGUI promptMessage;
    public Button switchButton;
    public Button readyButton;

    private PhotonView pView;
    private int teamSize;
    public string RooMName;

    [SerializeField] private RoundTimeDropdown roundTimeOption;

    Hashtable customProperties;

    private void Start()
    {
        instance = this;
    }

    public override void OnEnable()
    {
        base.OnEnable();
        pView = GetComponent<PhotonView>();
        if (!PhotonNetwork.IsConnected)
            return;
        roomName.text = "Room: " + PhotonNetwork.CurrentRoom.Name;
        RooMName = PhotonNetwork.CurrentRoom.Name;
        promptMessage.text = "";


        backButton.gameObject.SetActive(true);
        backButton.onClick.RemoveAllListeners();
        backButton.onClick.AddListener(delegate ()
        {
            OnClickBackButton();
            print("Back Button CLicked()");
        });

        teamSize = PhotonNetwork.CurrentRoom.MaxPlayers;
        Init();
    }
    public override void OnDisable()
    {
        base.OnDisable();
        if (roundTimeOption != null)
            Destroy(roundTimeOption.GetComponent<RoundTimeDropdown>());
    }

    #region UI information init
    private void Init()
    {
        DisableTeamPanel();
        UpdateTeamPanel(false);
        InitDropdownOptions();
        DistributeTeamPosition();
        ReadyButtonControl();
    }
    private void DistributeTeamPosition()
    {
        for (int i = 0; i < teamSize; i++)
        {

            if (!Team1[i].gameObject.activeSelf)
            {
                InitTeamPanelInfo(Team1[i]);
                InitCustomPropertiesInTeamPanel(TeamColor.White, i);
                break;

            }
            else if (!Team2[i].gameObject.activeSelf)
            {
                InitTeamPanelInfo(Team2[i]);
                InitCustomPropertiesInTeamPanel(TeamColor.Black, i);
                break;
            }
        }
    }

    private void InitDropdownOptions()
    {
        if (roundTimeOption != null)
            Destroy(roundTimeOption.GetComponent<RoundTimeDropdown>());

        GameObject go = GameObject.Find("RoundTimeDropdown");
        go.AddComponent<RoundTimeDropdown>();
        if (roundTimeOption == null)
            roundTimeOption = go.GetComponentInChildren<RoundTimeDropdown>();

    }
    public void InitTeamPanelInfo(TeamMemberItem memberItem)
    {
        memberItem.getKickPlayerButton().HideButton();

        memberItem.SetPlayerName(PhotonNetwork.LocalPlayer.NickName);
        memberItem.SetPlayerStatusText(PhotonNetwork.IsMasterClient ? PlayerStatusInfo.ROOMMASTER : PlayerStatusInfo.NOTREADY);
        memberItem.SetPlayerStatusIcon(PhotonNetwork.IsMasterClient ? PlayerStatusInfo.ReadyColor : PlayerStatusInfo.notReadyColor);
        memberItem.gameObject.SetActive(true);
    }
    public void InitCustomPropertiesInTeamPanel(TeamColor team, int teamNumber)
    {
        customProperties = PlayerProperties.getPlayerProperties();
        customProperties[PlayerProperties.Team] = (int)team;
        customProperties[PlayerProperties.TeamNumber] = teamNumber;

        PhotonNetwork.LocalPlayer.SetCustomProperties(customProperties);
    }

    public void ReadyButtonControl()
    {
        readyButton.onClick.RemoveAllListeners();
        readyButton.GetComponentInChildren<TextMeshProUGUI>().text = PhotonNetwork.IsMasterClient ? PlayerStatusInfo.START : PlayerStatusInfo.READY;
        if (PhotonNetwork.IsMasterClient)
        {
            readyButton.onClick.AddListener(delegate () { ClickStartGameButton(); });
            readyButton.interactable = false;
            switchButton.interactable = false;
        }
        else
        {
            readyButton.onClick.AddListener(delegate () { ClickReadyButton(); });
            readyButton.interactable = false;
            switchButton.interactable = false;
        }

    }
    #endregion
    #region Button Functions

    public void ClickReadyButton()
    {
        bool isReady = (bool)PhotonNetwork.LocalPlayer.CustomProperties[PlayerProperties.isReady];
        customProperties[PlayerProperties.isReady] = !isReady;
        PhotonNetwork.LocalPlayer.SetCustomProperties(customProperties);
        TextMeshProUGUI readyButtonText = readyButton.GetComponentInChildren<TextMeshProUGUI>();
        readyButtonText.text = (isReady) ? "Ready" : "Cancel";
    }


    public void ClickStartGameButton()
    {
        if (!IsTeamEmpty(Team1) && !IsTeamEmpty(Team2) && IsPlayerOnReady())
        {
            promptMessage.text = "";
            PhotonNetwork.CurrentRoom.IsOpen = false;
            pView.RPC("StartGame", RpcTarget.All);
        }
        else if (IsTeamEmpty(Team1) || IsTeamEmpty(Team2))
        {
            promptMessage.text = "Please a player for White team, another a player for Black team. ";
            StartUP.instance.playNottwoSound();
            return;
        }
        else if (!IsPlayerOnReady())
        {
            promptMessage.text = "Your opponent is not ready, so cannot start the game.";
            StartUP.instance.playNottwoSound();
            return;
        }
    }
    public void ClickSwitchButton()
    {
        customProperties = PhotonNetwork.LocalPlayer.CustomProperties;

        if ((bool)customProperties[PlayerProperties.isReady])
        {
            promptMessage.text = "Cannot change team during READY";
            return;
        }

        bool isSwitched = false;

        TeamColor currentTeamColor = (TeamColor)customProperties[PlayerProperties.Team];
        TeamColor anotherTeamColor = (currentTeamColor == TeamColor.White) ? TeamColor.Black : TeamColor.White;
        TeamMemberItem[] currentTeam = (Team1[0].gameObject.activeSelf) ? Team1 : Team2;
        TeamMemberItem[] anotherTeam = (currentTeamColor == TeamColor.White) ? Team2 : Team1;

        if ((int)customProperties[PlayerProperties.Team] == (int)currentTeamColor)
        {
            for (int i = 0; i < teamSize; i++)
            {
                if (!anotherTeam[i].gameObject.activeSelf)
                {
                    isSwitched = true;
                    TeamMemberItem currentItem = currentTeam[(int)customProperties[PlayerProperties.TeamNumber]];

                    currentItem.gameObject.SetActive(false);
                    InitTeamPanelInfo(anotherTeam[i]);

                    customProperties[PlayerProperties.Team] = (int)anotherTeamColor;
                    customProperties[PlayerProperties.TeamNumber] = i;
                    PhotonNetwork.LocalPlayer.SetCustomProperties(customProperties);
                    print("CurrentTeam: " + customProperties[PlayerProperties.Team]);
                    break;
                }

            }
        }

        promptMessage.text = (!isSwitched) ? "Another team is full, cannnot change" : "";
    }
    // leave room logic
    public void OnClickBackButton()
    {
        if (PhotonNetwork.IsMasterClient && NetworkTurnManager.Instance.gameState == GameState.Room)
        {
            KickAllPlayers();
            PhotonNetwork.CurrentRoom.EmptyRoomTtl = 0;
            PhotonNetwork.CurrentRoom.PlayerTtl = 0;
            PhotonNetwork.CurrentRoom.IsOpen = false;
            PhotonNetwork.CurrentRoom.IsVisible = false;
            PhotonNetwork.SendAllOutgoingCommands();
        }

        Invoke("LeaveRoom", 0.42f);
        //Update the cash when accepting 
        GameObject Main = Instantiate(Motherboard.instance.NotifyUser, Motherboard.instance.SpawnPoint.transform);
        TMP_Text TextTopic = Main.transform.Find("Topic").GetComponent<TMP_Text>();
        TextTopic.text = " Closing Room... ";
    }
    #endregion


    #region Photon call backs
    // update methods
    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        UpdateRoundTimeInfo();
    }
    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
        if (propertiesThatChanged.ContainsKey(RoomProperty.RoundTime))
            UpdateRoundTimeInfo();
    }
    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        UpdateInfo();
    }
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        // Call the base method to ensure any default logic is executed
        base.OnPlayerEnteredRoom(newPlayer);

        // Check if the newPlayer has the necessary custom properties assigned
        if (newPlayer.CustomProperties == null ||
            !newPlayer.CustomProperties.ContainsKey(PlayerProperties.Team) ||
            !newPlayer.CustomProperties.ContainsKey(PlayerProperties.TeamNumber))
        {
            // Assign the player to a team if they don't have team information
            AssignPlayerToTeam(newPlayer);
        }

        // Update the team panel to include the new player
        UpdateTeamPanel(false); // 'false' to update all players, not just the local one
    }

    // Method to assign a new player to a team based on custom logic
    private void AssignPlayerToTeam(Player player)
    {
        // Example logic to balance teams based on player count
        int team1Count = PhotonNetwork.PlayerList.Count(p =>
            p.CustomProperties.ContainsKey(PlayerProperties.Team) &&
            (int)p.CustomProperties[PlayerProperties.Team] == (int)TeamColor.White);

        int team2Count = PhotonNetwork.PlayerList.Count(p =>
            p.CustomProperties.ContainsKey(PlayerProperties.Team) &&
            (int)p.CustomProperties[PlayerProperties.Team] == (int)TeamColor.Black);

        int assignedTeam = team1Count <= team2Count ? (int)TeamColor.White : (int)TeamColor.Black;
        int assignedTeamNumber = assignedTeam == (int)TeamColor.White ? team1Count : team2Count;

        // Set the custom properties for the new player
        ExitGames.Client.Photon.Hashtable playerProps = new ExitGames.Client.Photon.Hashtable
    {
        { PlayerProperties.Team, assignedTeam },
        { PlayerProperties.TeamNumber, assignedTeamNumber }
    };

        player.SetCustomProperties(playerProps);
    }


    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (!otherPlayer.IsMasterClient)
            UpdateRoomStringInfo(RoomProperty.Player2, null);
        UpdateInfo();
        print(otherPlayer.NickName + " has left this room");
        StartUP.instance.playerhasLeftsound();
    }


    #endregion
    #region UI controll functions
    private void DisableTeamPanel()
    {
        for (int i = 0; i < Team1.Length; i++)
            Team1[i].gameObject.SetActive(false);
        for (int i = 0; i < Team2.Length; i++)
            Team2[i].gameObject.SetActive(false);
    }

    private void UpdateTeamPanel(bool isUpdateSelf)
    {
        // Loop through all players in the room
        foreach (var player in PhotonNetwork.PlayerList)
        {
            if (player == null) continue;

            // Skip updating the local player's information if not needed
            if (!isUpdateSelf && player.IsLocal) continue;

            // Get the player's custom properties
            customProperties = player.CustomProperties;

            // Initialize custom properties if they are null or missing expected keys
            if (customProperties == null ||
                !customProperties.ContainsKey(PlayerProperties.Team) ||
                !customProperties.ContainsKey(PlayerProperties.TeamNumber))
            {
                // Assign default values or call a method to assign them based on some logic
                InitCustomPropertiesInTeamPanel(TeamColor.White, 0);
                customProperties = PhotonNetwork.LocalPlayer.CustomProperties;
            }

            // Determine the correct team and team member item based on custom properties
            TeamMemberItem memberItem;
            if (customProperties[PlayerProperties.Team].Equals((int)TeamColor.White))
            {
                memberItem = Team1[(int)customProperties[PlayerProperties.TeamNumber]];
                //return;
            }
            else
            {
                memberItem = Team2[(int)customProperties[PlayerProperties.TeamNumber]];
            }

            // Activate the member item and set the player's name
            memberItem.gameObject.SetActive(true);
            memberItem.SetPlayerName(player.NickName);

            // Additional code to handle other player-specific logic, e.g., setting the player avatar, etc.
            // Example:
            // memberItem.SetPlayerAvatar(player.CustomProperties[PlayerProperties.Avatar]);
        }
    }

    public void UpdateInfo()
    {
        DisableTeamPanel();
        UpdateTeamPanel(true);
    }

    private void KickPlayerButtonControl(TeamMemberItem memberItem, Player player)
    {
        if (player.IsMasterClient || (player.IsLocal))
        {
            memberItem.getKickPlayerButton().HideButton();
        }
        else
        {
            memberItem.getKickPlayerButton().ShowButton();
        }

    }

    // update info methods

    private void UpdateRoundTimeInfo()
    {
        if (roundTimeOption.roundTimeDropdown != null)
        {
            // get the setting of round time from enum method
            roundTimeOption.UpdateRoundTimeMessage();
            // roundTimeOption.SetAllOptionsStatus(PhotonNetwork.IsMasterClient);
        }
    }
    private void UpdateRoomStringInfo(string key, string DataValue)
    {
        Hashtable newInfoProprey = new Hashtable();
        newInfoProprey.Add(key, DataValue);
        PhotonNetwork.CurrentRoom.SetCustomProperties(newInfoProprey);
    }
    #endregion
    // tools


    public void KickAllPlayers()
    {
        if (PhotonNetwork.CurrentRoom.PlayerCount > 1)
        {
            var playerList = PhotonNetwork.PlayerListOthers;
            foreach (var p in playerList)
            {
                if (!p.IsMasterClient)
                {
                    PhotonNetwork.CloseConnection(p);
                }
            }
        }
    }
    // Boolean logic
    private bool IsTeamEmpty(TeamMemberItem[] teamMembers)
    {
        foreach (var t in teamMembers)
        {
            if (t.gameObject.activeSelf)
                return false;
        }
        return true;
    }
    private bool IsPlayerOnReady()
    {
        foreach (var player in PhotonNetwork.CurrentRoom.Players)
        {
            if (player.Value.IsLocal) continue;
            if ((bool)player.Value.CustomProperties[PlayerProperties.isReady])
                return true;
        }
        return false;
    }


    [PunRPC]
    public void StartGame()
    {
        // disable this room panel -> open the game UI
        NonGameUIManager.Instance.roomPanel.SetActive(false);
        backButton.gameObject.SetActive(false);
        NetworkTurnManager.Instance.StartGame();
    }
    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom(false);
    }

}
