using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class JoinLobbyMenu : MonoBehaviour
{
    [SerializeField] private NetworkManagerHearts networkManager = null;

    [Header("UI")]
    [SerializeField] private GameObject background = null;
    [SerializeField] private TMP_InputField inputIpAddress = null;
    [SerializeField] private Button joinButton = null;
    

    private void OnEnable()
    {
        NetworkManagerHearts.OnClientConnected += HandleClientConnected;
        NetworkManagerHearts.OnClientDisconnected += HandleClientDisconnected;
    }

    private void OnDisable()
    {
        NetworkManagerHearts.OnClientConnected -= HandleClientConnected;
        NetworkManagerHearts.OnClientDisconnected -= HandleClientDisconnected;
    }

    public void JoinLobby() {
        
        //Set ip address from input field
        string ipAddress = inputIpAddress.text;
        if (ipAddress == "") { return; }
        MainMenu.networkManager.networkAddress = ipAddress;
        MainMenu.networkManager.StartClient();
        
        joinButton.interactable = false;
    }

    private void HandleClientConnected() {
        joinButton.interactable = true;

        gameObject.SetActive(false);
        background.SetActive(false);
     
    }

    private void HandleClientDisconnected()
    {
        joinButton.interactable = true;
       
    }

}
