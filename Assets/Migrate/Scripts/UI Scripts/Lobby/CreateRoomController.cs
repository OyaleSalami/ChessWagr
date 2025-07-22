using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using System;

public class CreateRoomController : MonoBehaviourPun
{
    public static CreateRoomController instance;
    public GameObject createRoomPanel;
    public GameObject roomLoadingPanel;
    public TMP_InputField roomName,roomCost;
    public TextMeshProUGUI roomNameHint;
    public GameObject roundTimeToggle;
    public float EntryCost;


    private RoundTime[] roundTimeOptions = { RoundTime.One, RoundTime.Two, RoundTime.Three };

    public void Start()
    {
        roomNameHint.text = "";
        instance = this;
    }

    private void Update()
    {
        if(roomCost.text.Length > 1)
            EntryCost = float.Parse(roomCost.text);
        
    }
    public void ClickConfirmCreateRoomButton()
    {
        int toggleValue = getToggleCurrentValue();
        RoomOptions roomOptions = RoomProperty.getRoomOptions(PhotonNetwork.LocalPlayer.NickName, toggleValue);

        if (RoomListing.Instance.isRoomNameRepeat(roomName.text))
        {
            roomNameHint.text = "Duplicate room name";
        }
        else if (roomName.text.Length == 1)
        {
            roomNameHint.text = "Room name empty";
        }
        else
        {
            string Property = $"{roomName.text}@NGN {roomCost.text}";
            PhotonNetwork.CreateRoom(Property, roomOptions, TypedLobby.Default);
            createRoomPanel.SetActive(false);
            roomLoadingPanel.SetActive(true);
            roomNameHint.text = "";
            print("Create room successfully.");
            LobbyPanelController.instance.Costtext.text = "NGN " + roomCost.text;

        }
    }

    public void StartGameButton()
    {
        if(EntryCost >= 1000 && roomName.text.Length >= 3)
        {
            ClickConfirmCreateRoomButton();
            // Debug.LogWarning("oooo");
            //clear field
            roomCost.text = "";
        }
        else
        {
            roomNameHint.text = "Minimum Wage 1000 & Maxmum Wage 50,000 or check RoomName";
        }
    }

    public void ClearRoomCost()
    {
        roomCost.text = "";
    }

    public void ClickCancelCreateRoomButton()
    {
        createRoomPanel.SetActive(false);
        roomNameHint.text = "";
    }



    public int getToggleCurrentValue()
    {
        RectTransform toggleRectTransform = roundTimeToggle.GetComponent<RectTransform>();
        int childCount = toggleRectTransform.childCount;

        for (int i = 0; i < childCount; i++)
        {
            if (toggleRectTransform.GetChild(i).GetComponent<Toggle>().isOn)
            {
                // put the round time from the options into room properties
                return (byte)roundTimeOptions[i];
            }
        }
        return -1;
    }

}
