using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;
using System;

public class GameMenuMP : NetworkBehaviour
{

    private NetworkManagerHearts room;
    private NetworkManagerHearts Room
    {
        get
        {
            if (room != null)
            {
                return room;
            }
            return room = NetworkManager.singleton as NetworkManagerHearts;
        }

    }

    // Update is called once per frame
    public void ReturnToMenu()
    {
        StopConnection();
        //SceneManager.LoadScene("Scene_Menu");
    }
    public void QuitGame() {

        StopConnection();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
         Application.Quit();

    }

    public void StopConnection() {

        Player player = GetComponentInParent<Player>();

        if (player.IsHost)
        {
            player.StopAll();
            Room.StopHost();
        }
        else
        {
            if (Player.players.Count - 1 < Settings.numberOfPlayersToPlay)
            {
                player.StopAll();
                Room.StopServer();
            }
            else
            {
                Room.StopClient();
            }
            //Room.StopClient();

        }
    }
    
}

