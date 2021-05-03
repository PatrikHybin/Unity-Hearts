using Mirror;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NetworkGamePlayerHearts : NetworkBehaviour
{
   
    [SyncVar]
    public string displayName = " ";

    private bool isHost;
    public bool IsHost
    {
        set { isHost = value; }
        get { return isHost; }
    }

    private NetworkManagerHearts room;

    private NetworkManagerHearts Room {
        get {
            if (room != null) {
                return room;
            }
            return room = NetworkManager.singleton as NetworkManagerHearts;
        }
    }

    public override void OnStartClient()
    {
        DontDestroyOnLoad(gameObject);

        Room.GamePlayers.Add(this);
    }

    public override void OnStopClient()
    {
        Room.GamePlayers.Remove(this); 
    }

    [Server]
    public void SetDisplayName(string displayName) {
        this.displayName = displayName;
    }
}
