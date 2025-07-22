// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RoomListView.cs" company="Exit Games GmbH">
//   Part of: Pun Cockpit
// </copyright>
// <author>developer@exitgames.com</author>
// --------------------------------------------------------------------------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using Photon.Realtime;
using UnityEngine.UI;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Photon.Pun.Demo.Cockpit
{
    /// <summary>
    /// PhotonNetwork.GetPing() UI property.
    /// </summary>
    public class PingsDisplay : MonoBehaviour
    {
        public PingsDisplay instance;

        public TMP_Text Text, BoardDisplay, InGameDisplay;
        public string UserId { get; set; }
        public GameObject RetryPanel;

        public string gameVersion = "1.0";

        int _cache = -1;

        public void Awake()
        {
            instance = this;
            Connect();
        }

        void Update()
        {
            if (PhotonNetwork.IsConnectedAndReady)
            {
                if (PhotonNetwork.GetPing() != _cache)
                {
                    _cache = PhotonNetwork.GetPing();
                    Text.text = "Pings: " + _cache.ToString() + " ms";
                    //BoardDisplay.text = "Pings: " + _cache.ToString() + " ms";
                    InGameDisplay.text = "Pings: " + _cache.ToString() + " ms";
                    //this.OnValueChanged();
                    RetryPanel.SetActive(false);
                }
            }
            else
            {
                if (_cache != -1)
                {
                    _cache = -1;
                    Text.text = "n/a";
                    //BoardDisplay.text = "n/a";
                    InGameDisplay.text = "n/a";
                    // RetryPanel.SetActive(true);
                }
            }
        }

        public void Connect()
        {
            PhotonNetwork.AuthValues = new AuthenticationValues();
            PhotonNetwork.AuthValues.UserId = this.UserId;


            PhotonNetwork.ConnectUsingSettings();
            RetryPanel.SetActive(false);
        }

        public void ReConnect()
        {
            PhotonNetwork.AuthValues = new AuthenticationValues();
            PhotonNetwork.AuthValues.UserId = this.UserId;

            PhotonNetwork.Reconnect();
        }
    }
}