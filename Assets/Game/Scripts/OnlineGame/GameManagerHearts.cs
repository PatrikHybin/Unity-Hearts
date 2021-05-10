using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using TMPro;
using Random = UnityEngine.Random;
using System.Linq;

public class GameManagerHearts : NetworkBehaviour
{

    public static GameManagerHearts gameManager;
    public int ready = 0; 

    private float currentTime = 0f;
    private float giveTime = 5f;
    private float playTime = 5f;
    private bool playersAreReady = false;
    private int interval = 1;
    private float nextTime = 0;
    [SerializeField] private List<Sprite> spritesForCards;
    private Sprite cardBack;

    public int currentRound = 0;
    public bool gameEnded = false;
    public bool giveCards;
    public bool timeForPlayer;

    [SyncVar]
    private List<GameObject> cards;

    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private GameObject playerScorePrefab, roundPrefab;

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

    void Awake()
    {
        if (gameManager == null)
        {
            gameManager = this;
        }

    }

    // Update is called once per frame
    void Update()
    {
        if (!gameEnded) {

            if (timeForPlayer)
            {
                ServerCountDownForPlayer();
            }
            if (Time.time >= nextTime)
            {
                if (giveCards)
                {
                    ServerNotifyForCards();
                }
                nextTime += interval;
            }
            if (playersAreReady)
            {
                ServerCountDownForGive();
            }
        }
        
    }

    public override void OnStartServer()
    {
        spritesForCards = new List<Sprite>();
    }

    [Server]
    public void StartGame()
    {
        if (spritesForCards.Count == 0)
        {
            RpcSetNames();
            LoadCards(Settings.cardSpritesPath);
            SpawnCards();
            SpawnHolders();
        }
        else
        {
            DropAuthorityForCards();
        }
        SuffleCards();
        AssignCard();
        RpcClearText();
    }
    #region ScoreBoardNames
    [ClientRpc]
    private void RpcSetNames()
    {
        for (int i = 0; i < Player.players.Count; i++)
        {
            Player.players[i].name = Room.GamePlayers[Room.GamePlayers.Count - 1 - i].displayName;
            Player.players[i].IsHost = Room.GamePlayers[Room.GamePlayers.Count - 1 - i].IsHost;
        }
        FillScoreBoardNames();

    }

    private void FillScoreBoardNames()
    {
        foreach (Player player in Player.players)
        {
            int index = 0;
            foreach (GameObject gameObject in player.ScoreBoardNameHolders)
            {
                if (index < Player.players.Count)
                {
                    gameObject.GetComponent<TextMeshProUGUI>().text = Player.players[index].name;
                }
                index++;
            }
        }

    }
    #endregion

    #region LoadSpawnSetCard
    [Server]
    private void LoadCards(string path)
    {
        Sprite[] cardSprites;
        cardSprites = Resources.LoadAll<Sprite>(path);
        Debug.Log("LoadCards obmedzenie na 8");
        foreach (Sprite card in cardSprites)
        {
            spritesForCards.Add(card);

            //if (spritesForCards.Count == 8)
            //{
            //    break;
            //}
        }

    }

    [Server]
    private void SpawnCards()
    {
        cards = new List<GameObject>();
        cardBack = spritesForCards.Find(item => item.name.Contains("background"));
        spritesForCards.RemoveAll(item => item.name.Contains("background"));
        //cardBack = spritesForCards[0];

        foreach (Sprite cardSprite in spritesForCards)
        {
            GameObject card = Instantiate(cardPrefab);
            NetworkServer.Spawn(card);
            cards.Add(card);
            RpcSetCard(card, cardSprite.name, cardBack.name);
        }

    }

    [ClientRpc]
    public void RpcSetCard(GameObject card, string cardSpriteName, string cardBackName)
    {
        Sprite cardSprite = Resources.Load<Sprite>("Sprites/Cards/" + cardSpriteName);
        Sprite cardBack = Resources.Load<Sprite>("Sprites/Cards/" + cardBackName);
        string[] splitSpriteName;
        splitSpriteName = cardSprite.name.Split("_"[0]);
        card.name = "Card " + splitSpriteName[0] + " " + splitSpriteName[1];
        card.GetComponent<Card>().Face = cardSprite;
        card.GetComponent<Card>().Back = cardBack;
        card.GetComponent<Card>().CardValue = EnumCardValue.getValue(splitSpriteName[0]);
        card.GetComponent<Card>().CardSuite = splitSpriteName[1];
        //card.GetComponent<Card>().OwnerId = indexOfPlayer;        
    }
    #endregion

    #region Holders
    [Server]
    private void SpawnHolders()
    {
        for (int i = 0; i < Player.players.Count; i++)
        {
            GameObject handHolderInstance = Instantiate(Player.players[i].handHolderPrefab);
            NetworkServer.Spawn(handHolderInstance, Player.players[i].GetComponent<NetworkIdentity>().connectionToClient);

            GameObject showHolderInstance = Instantiate(Player.players[i].showHolderPrefab);
            NetworkServer.Spawn(showHolderInstance, Player.players[i].GetComponent<NetworkIdentity>().connectionToClient);

            GameObject enemyShowHolderInstance = Instantiate(Player.players[i].enemyShowHolderPrefab);
            NetworkServer.Spawn(enemyShowHolderInstance, Player.players[i].GetComponent<NetworkIdentity>().connectionToClient);

            GameObject giveHolderInstance = Instantiate(Player.players[i].giveHolderPrefab);
            NetworkServer.Spawn(giveHolderInstance, Player.players[i].GetComponent<NetworkIdentity>().connectionToClient);

            GameObject playAreaHolderInstance = Instantiate(Player.players[i].playAreaHolderPrefab);
            NetworkServer.Spawn(playAreaHolderInstance, Player.players[i].GetComponent<NetworkIdentity>().connectionToClient);

            GameObject usedCardHolderInstance = Instantiate(Player.players[i].usedCardHolderPrefab);
            NetworkServer.Spawn(usedCardHolderInstance, Player.players[i].GetComponent<NetworkIdentity>().connectionToClient);

            RpcSetHolder(handHolderInstance, showHolderInstance, enemyShowHolderInstance, giveHolderInstance, playAreaHolderInstance, usedCardHolderInstance, i);
        }

    }

    [ClientRpc]
    public void RpcSetHolder(GameObject handHolderInstance, GameObject showHolderInstance, GameObject enemyShowHolderInstance, GameObject giveHolderInstance, GameObject playAreaHolderInstance, GameObject usedCardHolderInstance, int indexOfPlayer)
    {

        Player player = Player.players[indexOfPlayer];

        player.handHolder = handHolderInstance;
        player.handHolder.transform.SetParent(player.parentHolder.transform, false);
        //player.handHolder.GetComponent<CardZone>().OwnerId = indexOfPlayer;


        if (player.hasAuthority)
        {

            player.showHolder = showHolderInstance;
            player.showHolder.transform.SetParent(player.parentHolder.transform, false);
            enemyShowHolderInstance.transform.SetParent(player.parentHolder.transform, false);
            enemyShowHolderInstance.SetActive(false);
            //player.showHolder.GetComponent<CardZone>().OwnerId = indexOfPlayer;
        }
        else
        {

            player.showHolder = enemyShowHolderInstance;
            player.showHolder.transform.SetParent(player.parentHolder.transform, false);
            showHolderInstance.transform.SetParent(player.parentHolder.transform, false);
            showHolderInstance.SetActive(false);
            //player.showHolder.GetComponent<CardZone>().OwnerId = indexOfPlayer;
        }


        player.giveHolder = giveHolderInstance;
        player.giveHolder.transform.SetParent(player.parentHolder.transform, false);
        //player.giveHolder.GetComponent<CardZone>().OwnerId = indexOfPlayer;

        player.playAreaHolder = playAreaHolderInstance;
        player.playAreaHolder.transform.SetParent(player.parentHolder.transform, false);

        player.usedCardHolder = usedCardHolderInstance;
        player.usedCardHolder.transform.SetParent(player.parentHolder.transform, false);

        if (!player.hasAuthority)
        {
            player.handHolder.GetComponent<CanvasGroup>().blocksRaycasts = false;
            player.showHolder.GetComponent<CanvasGroup>().blocksRaycasts = false;
            player.giveHolder.GetComponent<CanvasGroup>().blocksRaycasts = false;
            player.playAreaHolder.GetComponent<CanvasGroup>().blocksRaycasts = false;
            player.usedCardHolder.GetComponent<CanvasGroup>().blocksRaycasts = false;
        }
    }
    #endregion 

    [Server]
    private void DropAuthorityForCards()
    {
        foreach (GameObject card in cards)
        {
            card.GetComponent<NetworkIdentity>().RemoveClientAuthority();
        }
    }

    #region SuffleAssignRpcShowCard
    [Server]
    private void SuffleCards()
    {
        Random.InitState(DateTime.Now.Second);
        for (int i = 0; i < cards.Count; i++)
        {
            GameObject temp = cards[i];
            int randomIndex = Random.Range(0, cards.Count);
            cards[i] = cards[randomIndex];
            cards[randomIndex] = temp;
        }
    }

    [Server]
    private void AssignCard()
    {
        int numberOfCardsPerPlayer = spritesForCards.Count / Player.players.Count;
        int player = 0;
        int index = 0;
        foreach (GameObject card in cards)
        {
            card.GetComponent<NetworkIdentity>().AssignClientAuthority(Player.players[player].connectionToClient);
            RpcShowCard(card, "Assign", player);
            index++;
            if (index % numberOfCardsPerPlayer == 0)
            {
                player++;
                if (player == Player.players.Count)
                {
                    break;
                }
            }
        }
    }

    [ClientRpc]
    public void RpcShowCard(GameObject card, string msg, int indexOfPlayer)
    {

        card.GetComponent<Card>().OwnerId = indexOfPlayer;
        if (card.GetComponent<Card>().hasAuthority)
        {

            card.GetComponent<CanvasGroup>().blocksRaycasts = true;
            card.GetComponent<Card>().SetImage(card.GetComponent<Card>().Face);
            card.transform.SetParent(Player.players[indexOfPlayer].handHolder.transform, false);
        }
        else
        {
            card.GetComponent<CanvasGroup>().blocksRaycasts = false;
            card.GetComponent<Card>().SetImage(card.GetComponent<Card>().Back);
            card.transform.SetParent(Player.players[indexOfPlayer].handHolder.transform, false);
        }

    }
    #endregion

    [Server]
    private void ServerNotifyForCards()
    {
        Debug.Log("give card");
        giveCards = false;
        timeForPlayer = false;
        playersAreReady = true;
        currentTime = giveTime;
        int round = currentRound;
        round++;
        RpcIncreaseRound(round);
        //Player.players[0].StartGame();
        StartGame();

        ServerCountScore();
        RpcGiveScore();  
    }

    [ClientRpc]
    private void RpcClearText()
    {
        foreach (Player player in Player.players) {
            player.SetText(" ");
        }
    }

    #region ScoreToScoreTable
    [ClientRpc]
    public void RpcGiveScore()
    {

        foreach (Player player1 in Player.players)
        {
            GameObject scoreBoard = player1.GetComponentInChildren<ScoreBoard>().gameObject;

            foreach (Transform gameObject in scoreBoard.GetComponentsInChildren<Transform>())
            {
                if (gameObject.gameObject.name == "Panel_RoundNumber")
                {
                    GameObject number = Instantiate(roundPrefab);
                    number.transform.SetParent(gameObject, false);
                    foreach (Transform transform in number.GetComponentsInChildren<Transform>())
                    {
                        if (transform.name == "Round")
                        {
                            transform.gameObject.GetComponent<TextMeshProUGUI>().text = GameManagerHearts.gameManager.currentRound.ToString();
                        }
                    }
                }
                foreach (Player player in Player.players)
                {
                    if (gameObject.gameObject.name == "Panel_Player" + player.PlayerId)
                    {
                        GameObject score = Instantiate(playerScorePrefab);
                        score.transform.SetParent(gameObject, false);
                        foreach (Transform transform in score.GetComponentsInChildren<Transform>())
                        {
                            if (transform.name == "PlayerScore")
                            {
                                transform.gameObject.GetComponent<TextMeshProUGUI>().text = player.TotalScore.ToString();
                            }
                        }
                    }
                }
            }

        }


    }
    #endregion

    [ClientRpc]
    private void RpcIncreaseRound(int round)
    {
        currentRound = round;
    }

    [Server]
    private void ServerCountScore()
    {
        bool onePlayerAllCards = false;
        foreach (Player player in Player.players)
        {
            foreach (CardZone cardZone in player.GetComponentsInChildren<CardZone>())
            {
                if (cardZone.name.Contains("UsedCardHolder"))
                {
                    if (cardZone.GetComponentsInChildren<Card>().Length == 32)
                    {

                        Player playerWithAll = player;
                        foreach (Player giveScorePlayer in Player.players) {
                            if (giveScorePlayer == player)
                            {
                                RpcIncreasePlayerScore(giveScorePlayer, -32);
                                player.AllCards++;
                            }
                            else {
                                RpcIncreasePlayerScore(giveScorePlayer, 32);
                            }                       
                        }
                    }
                }
            }

        }

        if (onePlayerAllCards) {
            return;
        }

        foreach (Player player in Player.players)
        {
            int playerScore = 0;
            foreach (CardZone cardZone in player.GetComponentsInChildren<CardZone>())
            {
                if (cardZone.name.Contains("UsedCardHolder"))
                {
                    foreach (Card card in cardZone.GetComponentsInChildren<Card>())
                    {
                        Debug.Log(card);
                        if (card.name.Contains("heart"))
                        {
                            //playerScore++;
                            playerScore += 30;
                        }
                        if (card.name.Contains("upperKnave leaf"))
                        {
                            playerScore += 12;

                        }
                        if (card.name.Contains("upperKnave acorn"))
                        {
                            playerScore += 8;

                        }
                        if (card.name.Contains("upperKnave ball"))
                        {
                            playerScore += 4;

                        }

                    }
                }
            }
            if (playerScore == 32) {
                player.AllPoints++;
            }
            RpcIncreasePlayerScore(player, playerScore);
        }
    }

    [ClientRpc]
    private void RpcIncreasePlayerScore(Player player, int score)
    {
        player.TotalScore += score;
        Debug.Log(player.name + "  " + score + "  score");
    }

    [Server]
    private void ServerCountDownForPlayer()
    {
        currentTime -= 1 * Time.deltaTime;
        if (currentTime.ToString("0") == "0")
        {
            timeForPlayer = false;
        }
        RpcCountDownForPlayer(currentTime);
        
    }

    [ClientRpc]
    private void RpcCountDownForPlayer(float currentTime)
    {
        foreach (Player player in Player.players) {
            if (player.hasAuthority && player.myTurn) {
                player.SetText(currentTime.ToString("0"));
                if (currentTime.ToString("0") == "0")
                {
                    player.PlayRandomCard();
                }
            }
            player.Phase = "PlayTime";
            if (!player.myTurn)
            {
                player.SetText(" ");
            }
        }
    }

    [Server]
    private void ServerCountDownForGive()
    {
        currentTime -= 1 * Time.deltaTime;
        RpcCountDown(currentTime.ToString("0"));
        if (currentTime.ToString("0") == "0") {
            playersAreReady = false;
            //Player.players[0].CmdSendGiveHolderCards();
            SendGiveHolderCards();
            RpcGiveTurn();
        }
    }

    [Server]
    private void SendGiveHolderCards()
    {
        timeForPlayer = false;
        foreach (Player player in Player.players)
        {
            if (player.SelectedCard != null)
            {
                player.ReleaseCard(player.SelectedCard);
            }
            Random.InitState(1234);
            Card[] cards = new Card[2];
            int numberOfCardsInGiveHolder = 0;
            numberOfCardsInGiveHolder = player.giveHolder.GetComponentsInChildren<Card>().Count<Card>();

            if (numberOfCardsInGiveHolder == 0)
            {
                cards = SelectRandomCards(2, player);
            }
            if (numberOfCardsInGiveHolder == 1)
            {
                for (int i = 0; i < 2; i++)
                {

                    if (i == 0)
                    {
                        cards[i] = player.giveHolder.GetComponentInChildren<Card>();
                    }
                    else
                    {
                        int idOfCard = Random.Range(0, player.handHolder.GetComponentsInChildren<Card>().Count<Card>() - 1);
                        Debug.Log(player.handHolder.GetComponentsInChildren<Card>()[idOfCard]);
                        cards[1] = player.handHolder.GetComponentsInChildren<Card>()[idOfCard];
                    }

                }

            }
            if (numberOfCardsInGiveHolder == 2)
            {
                cards = player.giveHolder.GetComponentsInChildren<Card>();
            }
            int indexOfPlayer = (player.PlayerId + 1) % Player.players.Count;
            foreach (Card card in cards)
            {
                //player.CmdRemoveObjectAuthority(card.gameObject);
                //player.CmdAssignPlayerAuthorityOverObject(Player.players[indexOfPlayer], card.gameObject);
                player.ServerRemoveObjectAuthority(card.gameObject);
                player.ServerAssignPlayerAuthorityOverObject(Player.players[indexOfPlayer], card.gameObject);
                player.RpcUpdateCard(card, Player.players[indexOfPlayer].handHolder);
                RpcChangeOwnerId(card, indexOfPlayer);
            }
            RpcChangeRaycast();

        }
        GameManagerHearts.gameManager.PlayerPlayTime();
    }

    private Card[] SelectRandomCards(int numberOfRandomCards, Player player)
    {
        Random.InitState(12345);
        Card[] cards = new Card[numberOfRandomCards];
        for (int i = 0; i < numberOfRandomCards; i++)
        {
            int idOfCard = Random.Range(0, player.handHolder.GetComponentsInChildren<Card>().Count<Card>() - 1);
            cards[i] = player.handHolder.GetComponentsInChildren<Card>()[idOfCard];
            player.handHolder.GetComponentsInChildren<Card>()[idOfCard].transform.parent = null;
        }
        return cards;
    }

    [ClientRpc]
    private void RpcChangeOwnerId(Card card, int indexOfPlayer)
    {
        card.GetComponent<Card>().OwnerId = Player.players[indexOfPlayer].PlayerId;
    }

    [ClientRpc]
    private void RpcChangeRaycast()
    {
        foreach (Player player in Player.players)
        {
            if (player.hasAuthority)
            {
                foreach (Card card in player.handHolder.GetComponentsInChildren<Card>())
                {
                    if (card.GetComponent<CanvasGroup>())
                    {
                        card.GetComponent<CanvasGroup>().blocksRaycasts = true;
                    }

                }
            }
            else
            {
                foreach (Card card in player.handHolder.GetComponentsInChildren<Card>())
                {
                    if (card.GetComponent<CanvasGroup>())
                    {
                        card.GetComponent<CanvasGroup>().blocksRaycasts = false;
                    }
                }
            }
        }
    }

    [ClientRpc]
    private void RpcCountDown(string time)
    {
        foreach (Player player in Player.players)
        {
            if (player.hasAuthority)
            {
                player.SetText(time);
                player.Phase = "GivePhase";
            }
        }
    }

    [ClientRpc]
    private void RpcGiveTurn()
    {
        foreach (Player player in Player.players) {
            if (player.hasAuthority)
            {
                player.Phase = "PlayPhase";
            }
            
        }
        Player.players[0].myTurn = true;
    }

    public void ResetPlayerPlayTime()
    {
        currentTime = playTime;
        timeForPlayer = true;
    }

    public void GiveCardTime()
    {
        currentTime = giveTime;
        ready++;
        if (ready == Player.players.Count)
        {
            ready = 0;
            timeForPlayer = false;
            playersAreReady = true;
        }  
        
    }
    [Server]
    public void PlayerPlayTime() {
        currentTime = playTime;
        timeForPlayer = true;
    }

    
}
