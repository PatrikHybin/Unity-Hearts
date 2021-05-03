using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;
using System;
using System.Linq;
using System.Collections.Generic;

public class NetworkManagerHearts : NetworkManager
{
    [Scene] [SerializeField] private string menuScene = string.Empty;
    [Scene] [SerializeField] private string gameScene = string.Empty;

    [Header("Room")]
    [SerializeField] private NetworkRoomPlayerHearts roomPlayerPrefab = null;

    [Header("Game")]
    [SerializeField] private NetworkGamePlayerHearts gamePlayerPrefab = null;
    [SerializeField] private GameObject spawnSystemPrefab = null;
    [SerializeField] private GameObject gameManagerPrefab = null;
    

    public static event Action OnClientConnected;
    public static event Action OnClientDisconnected;
    public static event Action<NetworkConnection> OnServerReadied;

    public List<NetworkRoomPlayerHearts> RoomPlayers { get; } = new List<NetworkRoomPlayerHearts>();
    public List<NetworkGamePlayerHearts> GamePlayers { get; } = new List<NetworkGamePlayerHearts>();
    public override void OnStartServer() {
        RoomPlayers.Clear();
        spawnPrefabs = Resources.LoadAll<GameObject>("SpawnPrefabs").ToList();
    } //=> spawnPrefabs = Resources.LoadAll<GameObject>("SpawnPrefabs").ToList();

    public override void OnStartClient()
    {
        var prefabs = Resources.LoadAll<GameObject>("SpawnPrefabs");

        foreach (var prefab in prefabs) {
            ClientScene.RegisterPrefab(prefab);
        } 
    }

    public override void OnClientConnect(NetworkConnection conn)
    {
        base.OnClientConnect(conn);
        
        OnClientConnected?.Invoke();
    }

    public override void OnClientDisconnect(NetworkConnection conn)
    {
        base.OnClientDisconnect(conn);
        
        OnClientDisconnected?.Invoke();
    }

    public override void OnStopClient()
    {
        Debug.Log(" On stop client v manageri");
        ServerChangeScene("Scene_Menu");
        base.OnStopClient();
    }

    public override void OnStopHost()
    {
        Debug.Log(" On stop host v manageri");
        ServerChangeScene("Scene_Menu");
        base.OnStopClient();
    }

    public override void OnServerConnect(NetworkConnection conn)
    {

        if (numPlayers >= maxConnections)
        {
            conn.Disconnect();
            ServerChangeScene("Scene_Menu");
            return;
        }

        if (SceneManager.GetActiveScene().path != menuScene)
        {
            conn.Disconnect();
            return;
        }
        
    }

    public override void OnServerDisconnect(NetworkConnection conn)
    {
        
        if (conn.identity != null) {
            var player = conn.identity.GetComponent<NetworkRoomPlayerHearts>();
            var playerGame = conn.identity.GetComponent<NetworkGamePlayerHearts>();

            
            GamePlayers.Remove(playerGame);

            RoomPlayers.Remove(player);


            NotifyPlayersOfReadyState();

        }
        
        base.OnServerDisconnect(conn);
    }

    public void NotifyPlayersOfReadyState()
    {
        foreach (var player in RoomPlayers) {
            //host can interact with start button
            player.HandleReadyToStart(IsReadyToStart());
        }
    }

    private bool IsReadyToStart()
    {
        foreach (var player in RoomPlayers)
        {
            if (!player.IsReady) {
                return false;
            }
        }

        return true;
    }

    public override void OnServerAddPlayer(NetworkConnection conn)
    {
        if (SceneManager.GetActiveScene().path == menuScene) {
            
            bool isHost = false;
            if (RoomPlayers.Count == 0) {
                isHost = true;
            }
            NetworkRoomPlayerHearts roomPlayer = Instantiate(roomPlayerPrefab);
            
            //if (RoomPlayers.Count == 1)
            //{
            //    roomPlayer.IsHost = true;
            //} else {
            //    roomPlayer.IsHost = false;
            //}

            roomPlayer.IsHost = isHost;
            Debug.Log(RoomPlayers.Count);
            NetworkServer.AddPlayerForConnection(conn, roomPlayer.gameObject);
            
        }
       
    }

    public override void OnStopServer()
    {
        RoomPlayers.Clear();
        GamePlayers.Clear();
    }

    public void StartGame() {
        Debug.Log("Start Game");
        if (SceneManager.GetActiveScene().path == menuScene) {

            if (!IsReadyToStart()) {
                return;    
            }
            ServerChangeScene("Scene_Game");
        }
        
    }

    public override void ServerChangeScene(string newSceneName)
    {
        // From menu to game
        if (SceneManager.GetActiveScene().path == menuScene && newSceneName.StartsWith("Scene_Game"))
        {
            Debug.Log(GamePlayers.Count + " kolko hracov tam je");
            for (int i = RoomPlayers.Count - 1; i >= 0; i--)
            {
                var conn = RoomPlayers[i].connectionToClient;
                var gamePlayerInstance = Instantiate(gamePlayerPrefab);
                
                gamePlayerInstance.SetDisplayName(RoomPlayers[i].DisplayName);
                gamePlayerInstance.GetComponent<NetworkGamePlayerHearts>().IsHost = RoomPlayers[i].IsHost;

                gamePlayerInstance.name = RoomPlayers[i].DisplayName;
                //destroy roomplayer
                NetworkServer.Destroy(conn.identity.gameObject);
                //give them authority of objects
                NetworkServer.ReplacePlayerForConnection(conn, gamePlayerInstance.gameObject, true);
                //NetworkServer.ReplacePlayerForConnection(conn, gamePlayerInstance.gameObject);

            }
        }
        //if (SceneManager.GetActiveScene().path == menuScene && newSceneName.StartsWith("Scene_Menu"))
        //{
        //    for (int i = RoomPlayers.Count - 1; i >= 0; i--)
        //    {
        //        var conn = RoomPlayers[i].connectionToClient;
        //        //destroy roomplayer
        //        NetworkServer.Destroy(conn.identity.gameObject);

        //    }
        //}

        if (SceneManager.GetActiveScene().path == gameScene && newSceneName.StartsWith("Scene_Menu"))
        {
            Debug.Log(GamePlayers.Count);
            //for (int i = GamePlayers.Count - 1; i >= 0; i--)
            //{
            //    var conn = GamePlayers[i].connectionToClient;

            //    //NetworkServer.Destroy(conn.identity.gameObject);
            //    NetworkServer.Destroy(GameObject.Find("SpawnSystem(Clone)").gameObject);
            //    NetworkServer.Destroy(GameObject.Find("GameManagerHearts(Clone)").gameObject);
            //}
            //Debug.Log(GamePlayers.Count);
            NetworkServer.Destroy(GameObject.Find("SpawnSystem(Clone)").gameObject);
            NetworkServer.Destroy(GameObject.Find("GameManagerHearts(Clone)").gameObject);
        }
        base.ServerChangeScene(newSceneName);
    }

   
    public override void OnServerSceneChanged(string sceneName)
    {
        Debug.Log(sceneName + "OnServerSceneChanged");
        if (sceneName.StartsWith("Scene_Game"))
        { 
            GameObject spawnSystemInstance = Instantiate(spawnSystemPrefab);
            NetworkServer.Spawn(spawnSystemInstance);
            //GameObject cardManagerInstance = Instantiate(cardManagerPrefab);
            //NetworkServer.Spawn(cardManagerInstance);
            GameObject gameManagerInstance = Instantiate(gameManagerPrefab);
            NetworkServer.Spawn(gameManagerInstance);

        }
    }

    public override void OnServerReady(NetworkConnection conn)
    {
        base.OnServerReady(conn);

        OnServerReadied?.Invoke(conn);
    }

}
