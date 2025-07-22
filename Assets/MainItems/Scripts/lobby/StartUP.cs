using ExitGames.Client.Photon;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class StartUP : MonoBehaviour
{
    public static StartUP instance;

    [Header("SOUNDS")]
    public AudioSource SoundOptions;
    public AudioClip victorySound,Defeatsound,CreatingRoom,NotTwoPlayersound,PlayerhasLeftsound,Myturn, OpponentTurn,
        ReturnTolobbbySound,InGameDanger;

    public TMP_InputField NameInput;
    public TMP_Text NameText;
    public GameObject NameinputObj;
    public Button OkayButton;

    public TMP_Text Rating, PVP, Allmatch, lostmatch, Tournament;
    public float pvp, LostMatch, tournament;

    public float MatchWon,rating;
    public string MatchTire;
    public Slider Tirelider;
    public TMP_Text TireDisplay;

    public GameObject NewlevelPanel;
    public TMP_Text NewTireDisplay;

    public GameObject GoldOBJ, SliverOBJ, platinumOBJ, DimondOBJ, AceOBJ;
    public Color colorG, colorS, colorP, colorD, colorA;
    public Image FillIMG;

    public float AllMatchPlayed;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //if (PlayerPrefs.HasKey("name"))
        //{
        //    NameinputObj.SetActive(false);
        //    NameText.text = PlayerPrefs.GetString("name");  
        //}
        //else
        //{
        //    NameinputObj.SetActive(true);
        //}

        instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        Application.targetFrameRate = 100;

        string Tire1 = "Gold";
        string Tire2 = "Sliver";
        string Tire3 = "Platinum";
        string Tire4 = "Dimond";
        string Tire5 = "Ace";

        float TireAmount1 = 5;
        float TireAmount2 = 30;
        float TireAmount3 = 120;
        float TireAmount4 = 260;
        float TireAmount5 = 510;

        if (MatchWon >= TireAmount1)
        {
            MatchTire = Tire1;
            Tirelider.maxValue = TireAmount2;
            TireDisplay.text = Tire1;
            NewRankPanel();

            if (PlayerPrefs.HasKey("tire1"))
            {
                GoldOBJ.SetActive(true);
                FillIMG.color = colorG;
            }
            else
            {
                PlayerPrefs.SetString("tire1", MatchTire);
                Motherboard.instance.UpdateTire();
                NewTireDisplay.text = "Gold";
                NewlevelPanel.SetActive(true);
                SoundOptions.clip = victorySound;
                SoundOptions.Play();
                //PlayerPrefs.DeleteKey("tire5");
            }
        }
        if (MatchWon >= TireAmount2)
        {
            MatchTire = Tire2;
            Tirelider.maxValue = TireAmount3;
            TireDisplay.text = Tire2;
            NewRankPanel1();

            if (PlayerPrefs.HasKey("tire2"))
            {
                SliverOBJ.SetActive(true);
                FillIMG.color = colorS;
            }
            else
            {
                PlayerPrefs.SetString("tire2", MatchTire);
                Motherboard.instance.UpdateTire();
                NewTireDisplay.text = "Sliver";
                NewlevelPanel.SetActive(true);
                SoundOptions.clip = victorySound;
                SoundOptions.Play();
                //PlayerPrefs.DeleteKey("tire1");
            }
        }
        if (MatchWon >= TireAmount3)
        {
            MatchTire = Tire3;
            Tirelider.maxValue = TireAmount4;
            TireDisplay.text = Tire3;
            NewRankPanel2();

            if (PlayerPrefs.HasKey("tire3"))
            {
                platinumOBJ.SetActive(true);
                FillIMG.color = colorP;
            }
            else
            {
                PlayerPrefs.SetString("tire3", MatchTire);
                Motherboard.instance.UpdateTire();
                NewTireDisplay.text = "platinum";
                NewlevelPanel.SetActive(true);
                SoundOptions.clip = victorySound;
                SoundOptions.Play();
                //PlayerPrefs.DeleteKey("tire2");
            }
        }
        if (MatchWon >= TireAmount4)
        {
            MatchTire = Tire4;
            Tirelider.maxValue = TireAmount5;
            TireDisplay.text = Tire4;
            NewRankPanel3();

            if (PlayerPrefs.HasKey("tire4"))
            {
                DimondOBJ.SetActive(true);
                FillIMG.color = colorD;
            }
            else
            {
                PlayerPrefs.SetString("tire4", MatchTire);
                Motherboard.instance.UpdateTire();
                NewTireDisplay.text = "Dimond";
                NewlevelPanel.SetActive(true);
                SoundOptions.clip = victorySound;
                SoundOptions.Play();
                //PlayerPrefs.DeleteKey("tire3");
            }
        }
        if (MatchWon >= TireAmount5)
        {
            MatchTire = Tire5;
            Tirelider.maxValue = TireAmount5;
            TireDisplay.text = Tire5;
            NewRankPanel4();

            if (PlayerPrefs.HasKey("tire5"))
            {
                AceOBJ.SetActive(true);
                FillIMG.color = colorA;
            }
            else
            {
                PlayerPrefs.SetString("tire5", MatchTire);
                Motherboard.instance.UpdateTire();
                NewTireDisplay.text = "Ace";
                NewlevelPanel.SetActive(true);
                SoundOptions.clip = victorySound;
                SoundOptions.Play();
                //PlayerPrefs.DeleteKey("tire4");
            }
        }

        Tirelider.value = MatchWon;

        if(NameInput.text == "")
        {
            OkayButton.interactable = false;
        }
        else
        {
            OkayButton.interactable = true;
        }
    }

    public void NewRankPanel()
    {
        
    }
    public void NewRankPanel1()
    {
        
    }
    public void NewRankPanel2()
    {
        
    }
    public void NewRankPanel3()
    {
        
    }
    public void NewRankPanel4()
    {
        
    }

    public void SaveName()
    {
        NameText.text = NameInput.text; 
        PlayerPrefs.SetString("name", NameInput.text);
    }

    public void CreatingCustomRoomSound()
    {
        SoundOptions.clip = CreatingRoom;
        SoundOptions.Play();
    }
    public void playNottwoSound()
    {
        SoundOptions.clip = NotTwoPlayersound;
        SoundOptions.Play();
    }
    public void playerhasLeftsound()
    {
        SoundOptions.clip = PlayerhasLeftsound;
        SoundOptions.Play();
    }
    public void MyturnSound()
    {
        SoundOptions.clip = Myturn;
        SoundOptions.Play();
    }
    public void opponentTurnSound()
    {
        SoundOptions.clip = OpponentTurn;
        SoundOptions.Play();
    }
    public void VictorySound()
    {
        SoundOptions.clip = victorySound;
        SoundOptions.Play();
    }
    public void DefeatSound()
    {
        SoundOptions.clip = Defeatsound;
        SoundOptions.Play();
    }
    public void ReturnToLobby()
    {
        SoundOptions.clip = ReturnTolobbbySound;
        SoundOptions.Play();
    }
}
