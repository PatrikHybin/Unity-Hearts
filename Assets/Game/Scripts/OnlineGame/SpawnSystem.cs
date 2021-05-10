using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;
using System.Linq;
using UnityEngine.SceneManagement;

public class SpawnSystem : NetworkBehaviour
{
    [SerializeField] private GameObject playerPrefab = null;
    

    private static List<Transform> spawnPoints = new List<Transform>();
    public int index = 0;

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

    public static void AddSpawnPoint(Transform transform) {

        spawnPoints.Add(transform);

        spawnPoints = spawnPoints.OrderBy(x => x.GetSiblingIndex()).ToList();

    }

    public static void RemoveSpawnPoint(Transform transform) {

        spawnPoints.Remove(transform);
    }

    public override void OnStartServer()
    {
        Debug.Log("OnStartServer");
        NetworkManagerHearts.OnServerReadied += SpawnPlayer;
        NetworkManagerHearts.OnServerReadied += CheckToSpawnCards;
        base.OnStartServer();
    }

    //OnDestroy
    public override void OnStopServer()
    {
        NetworkManagerHearts.OnServerReadied -= SpawnPlayer;
        NetworkManagerHearts.OnServerReadied -= CheckToSpawnCards;

    }

    [Server]
    public void SpawnPlayer(NetworkConnection conn) {
        Transform spawnPoint = spawnPoints.ElementAtOrDefault(index);

        if (spawnPoint == null)
        {
            return;
        }

        GameObject playerInstance = Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation);
        NetworkServer.Spawn(playerInstance, conn);

        index++;
    }

    [Server]
    public void CheckToSpawnCards(NetworkConnection conn)
    {

        if (Room.GamePlayers.Count(x => x.connectionToClient.isReady) != Room.GamePlayers.Count)
        {
            return;
        }

        Debug.Log("Spawni karty v spawnsysteme");
        GameManagerHearts.gameManager.StartGame();
        //GameManagerHearts.gameManager.giveCards = true;
        //Player.players[0].StartGame();
    }

}
