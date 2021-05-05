using Mirror;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;


public class Player : NetworkBehaviour
{
    [SerializeField] public GameObject parentHolder, handHolderPrefab, showHolderPrefab, enemyShowHolderPrefab, giveHolderPrefab, playAreaHolderPrefab, usedCardHolderPrefab, camera;
    [SerializeField] private TMP_Text text, text1 = null;
    [SerializeField] private GameObject cardPrefab, fakeCardPrefab;
    [SerializeField] public GameObject scoreBoard, panel;
    [SerializeField] private GameObject playerScorePrefab, roundPrefab;
    public GameObject handHolder, showHolder, giveHolder, playAreaHolder, usedCardHolder;

    //handHolderPrefab, showHolderPrefab, giveHolderPrefab
    public static List<Player> players = new List<Player>();
    public bool Win { get; internal set; }
    public int AllCards { get; internal set; }
    public int AllPoints { get; internal set; }

    [SyncVar]
    public int PlayerId = -1;

    [SyncVar]
    public bool myTurn = false;

    [SyncVar]
    private int roundScore = 0;
    public int RoundScore
    {
        get { return roundScore; }
        set { roundScore = value; }
    }

    [SyncVar]
    private int totalScore = 0;
    public int TotalScore
    {
        get { return totalScore; }
        set { totalScore = value; }
    }

    private string phase;
    public string Phase
    {
        get { return phase; }
        set { phase = value; }
    }

    private Sprite cardBack;
    public static Vector3 DistanceCameraPlane = new Vector3(0, 400, 650);
    private int cardCount;

    private Card selectedCard;
    public Card SelectedCard
    {
        get { return selectedCard; }
        set { selectedCard = value; }
    }
    private GameObject cardZone;
    public GameObject CardZone
    {
        get { return cardZone; }
        set { cardZone = value; }
    }
    private GameObject currentCardZone;
    public GameObject CurrentCardZone
    {
        get { return currentCardZone; }
        set { currentCardZone = value; }
    }

    private GameObject fakeCard;
    private List<Sprite> spritesForCards;
    //private bool randomCard;

    

    [SyncVar]
    private List<GameObject> cards;
    public List<GameObject> ScoreBoardNameHolders { get; internal set; }
    //private List<GameObject> scoreBoardNameHolders;
    private List<GameObject> ScoreBoardScoreHolders;

    [SerializeField]
    [SyncVar]
    private bool canPlayThatCard = false;
    private bool endOfGame;

    private bool isHost;
    public bool IsHost
    {
        set { isHost = value; }
        get { return isHost; }
    }

    private NetworkManagerHearts room;
    private SoundManager soundManager;

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

    public void Awake()
    {
        //string name = InputPlayerName.DisplayName;
        //Debug.Log(name);

        players.Add(this);
        PlayerId = players.Count - 1;
        ScoreBoardNameHolders = new List<GameObject>();
        ScoreBoardScoreHolders = new List<GameObject>();
    }

    public void Update()
    {
        try
        {
            GameObject.Find("PlayAreaHolderMaster").GetComponent<Transform>().rotation = Quaternion.Euler(
                GameObject.Find("PlayAreaHolderMaster").GetComponent<Transform>().eulerAngles.x,
                this.GetComponent<Transform>().eulerAngles.y,
                GameObject.Find("PlayAreaHolderMaster").GetComponent<Transform>().eulerAngles.z);
        }
        catch { }
        

        if (Input.GetKey(KeyCode.Tab) == true)
        {
            if (hasAuthority)
            {
                //GameObject gameObject = GetComponentInChildren<ScoreBoard>().gameObject;
                GetComponentInChildren<ScoreBoard>().GetComponent<CanvasGroup>().blocksRaycasts = true;
                scoreBoard.GetComponent<RectTransform>().anchorMin = new Vector2(0.3f, 0.3f);
                scoreBoard.GetComponent<RectTransform>().anchorMax = new Vector2(0.7f, 0.7f);
                scoreBoard.transform.transform.localScale = new Vector3(1, 1, 1);
            }
            else
            {
                scoreBoard.transform.transform.localScale = new Vector3(0, 0, 0);
                GetComponentInChildren<ScoreBoard>().GetComponent<CanvasGroup>().blocksRaycasts = false;
            }
        }
        else
        {
            if (hasAuthority)
            {
                //GameObject gameObject = GetComponentInChildren<ScoreBoard>().gameObject;
                GetComponentInChildren<ScoreBoard>().GetComponent<CanvasGroup>().blocksRaycasts = false;
                scoreBoard.transform.transform.localScale = new Vector3(0, 0, 0);

            }
            else
            {
                scoreBoard.transform.transform.localScale = new Vector3(0, 0, 0);
                GetComponentInChildren<ScoreBoard>().GetComponent<CanvasGroup>().blocksRaycasts = false;
            }

        }

        if (hasAuthority)
        {
            if (Input.GetKey(KeyCode.Escape) && !GameManagerHearts.gameManager.gameEnded)
            {
                panel.SetActive(true);
                panel.transform.localScale = new Vector3(1, 1, 1);
            }
            else
            {
                panel.SetActive(false);
            }
        }
        else {
            panel.transform.localScale = new Vector3(0, 0, 0);
            panel.GetComponent<CanvasGroup>().blocksRaycasts = false;
        }
        

        if (GameManagerHearts.gameManager.gameEnded) {
            if (hasAuthority) {
                scoreBoard.transform.transform.localScale = new Vector3(0.9f, 0.9f, 0.9f);
                //scoreBoard.transform.position = new Vector3(scoreBoard.transform.position.x, scoreBoard.transform.position.y, -300);
                scoreBoard.GetComponent<RectTransform>().anchorMin = new Vector2(0.4f, 0.4f);
                scoreBoard.GetComponent<RectTransform>().anchorMax = new Vector2(0.8f, 0.8f);
                panel.SetActive(true);
            }
        }

        if (this.TotalScore >= Settings.losingScoreMp && !endOfGame) {
            endOfGame = true;
            Debug.Log("endOfGame");
            CmdEndOfGame();
        }

    }

    [Command]
    private void CmdEndOfGame()
    {
        RpcEndOfGame();
    }

    [ClientRpc]
    private void RpcEndOfGame()
    {
        GameManagerHearts.gameManager.gameEnded = true;
        endOfGame = true;
        parentHolder.SetActive(false);
        int losingScore = 100;

        foreach (Player player in players) {
            if (player.TotalScore > losingScore) {
                losingScore = player.TotalScore;
            }
        }
        List<Player> losingPlayers = new List<Player>();
        foreach (Player player in players)
        {
            if (player.TotalScore == losingScore)
            {
                losingPlayers.Add(player);
                player.Win = false;
            } else
            {
                player.Win = true;
            }
        }

        foreach (Player player in players)
        {
            if (player.hasAuthority)
            {
                WriteMpGameResults gameScore = new WriteMpGameResults();
                gameScore.WritePlayersScore(players, player.Win, GameManagerHearts.gameManager.currentRound);
                gameScore.WritePlayerStats(player);
            }
        }

    }
    //public override void OnStartServer()
    //{
    //    //spritesForCards = new List<Sprite>();
    //}
    ////NetworkIdentity netId = NetworkClient.connection.identity;
    ////Player player = netId.GetComponent<Player>();
    ////player.SetText("a mam id " + netId);

    public override void OnStopClient()
    {
        players.Remove(this);

        //ScoreBoard score = GetComponentInChildren<ScoreBoard>();
        //score.transform.parent = null;
        //DontDestroyOnLoad(score);

        base.OnStopClient();
    }


    public override void OnStartAuthority()
    {
        camera.gameObject.SetActive(true);
        CmdGameManagerGiveCardTime();
        try
        {
            soundManager = GameObject.Find("SoundManager(Clone)").GetComponent<SoundManager>();
        }
        catch { }
    }
    
    [Command]
    private void CmdGameManagerGiveCardTime()
    {
        GameManagerHearts.gameManager.GiveCardTime();
    }

    public override void OnStartServer()
    {
        spritesForCards = new List<Sprite>();
    }

    //[Server]
    //public void StartGame()
    //{

    //    if (spritesForCards.Count == 0)
    //    {
    //        RpcSetNames();
    //        LoadCards(Settings.cardSpritesPath);
    //        SpawnCards();
    //        SpawnHolders();
    //    }
    //    else
    //    {
    //        DropAuthorityForCards();
    //    }
    //    SuffleCards();
    //    AssignCard();

    //}

    #region OldSolution

    [ClientRpc]
    private void RpcSetNames()
    {
        for (int i = 0; i < players.Count; i++)
        {
            players[i].name = Room.GamePlayers[Room.GamePlayers.Count - 1 - i].displayName;
            players[i].IsHost = Room.GamePlayers[Room.GamePlayers.Count - 1 - i].IsHost;
        }
        FillScoreBoardNames();

    }

    private void FillScoreBoardNames()
    {
        foreach (Player player in players)
        {
            int index = 0;
            foreach (GameObject gameObject in player.ScoreBoardNameHolders)
            {
                if (index < players.Count)
                {
                    gameObject.GetComponent<TextMeshProUGUI>().text = players[index].name;
                }
                index++;
            }
        }

    }

    [Server]
    private void LoadCards(string path)
    {
        Sprite[] cardSprites;
        cardSprites = Resources.LoadAll<Sprite>(path);
        Debug.Log("LoadCards obmedzenie na 8");
        foreach (Sprite card in cardSprites)
        {

            spritesForCards.Add(card);

            if (spritesForCards.Count == 8)
            {
                break;
            }
        }

    }

    [Server]
    private void SpawnCards()
    {
        cards = new List<GameObject>();
        //cardBack = spritesForCards.Find(item => item.name.Contains("background"));
        spritesForCards.RemoveAll(item => item.name.Contains("background"));
        cardBack = spritesForCards[0];

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

    [Server]
    private void DropAuthorityForCards()
    {
        foreach (GameObject card in cards)
        {
            card.GetComponent<NetworkIdentity>().RemoveClientAuthority();
        }
    }



    [Server]
    private void SpawnHolders()
    {
        Debug.Log(players.Count);
        for (int i = 0; i < players.Count; i++)
        {
            GameObject handHolderInstance = Instantiate(players[i].handHolderPrefab);
            NetworkServer.Spawn(handHolderInstance, players[i].GetComponent<NetworkIdentity>().connectionToClient);

            GameObject showHolderInstance = Instantiate(players[i].showHolderPrefab);
            NetworkServer.Spawn(showHolderInstance, players[i].GetComponent<NetworkIdentity>().connectionToClient);

            GameObject enemyShowHolderInstance = Instantiate(players[i].enemyShowHolderPrefab);
            NetworkServer.Spawn(enemyShowHolderInstance, players[i].GetComponent<NetworkIdentity>().connectionToClient);

            GameObject giveHolderInstance = Instantiate(players[i].giveHolderPrefab);
            NetworkServer.Spawn(giveHolderInstance, players[i].GetComponent<NetworkIdentity>().connectionToClient);

            GameObject playAreaHolderInstance = Instantiate(players[i].playAreaHolderPrefab);
            NetworkServer.Spawn(playAreaHolderInstance, players[i].GetComponent<NetworkIdentity>().connectionToClient);

            GameObject usedCardHolderInstance = Instantiate(players[i].usedCardHolderPrefab);
            NetworkServer.Spawn(usedCardHolderInstance, players[i].GetComponent<NetworkIdentity>().connectionToClient);

            RpcSetHolder(handHolderInstance, showHolderInstance, enemyShowHolderInstance, giveHolderInstance, playAreaHolderInstance, usedCardHolderInstance, i);
        }

    }


    [ClientRpc]
    public void RpcSetHolder(GameObject handHolderInstance, GameObject showHolderInstance, GameObject enemyShowHolderInstance, GameObject giveHolderInstance, GameObject playAreaHolderInstance, GameObject usedCardHolderInstance, int indexOfPlayer)
    {

        Player player = players[indexOfPlayer];

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
            card.GetComponent<NetworkIdentity>().AssignClientAuthority(players[player].connectionToClient);
            RpcShowCard(card, "Assign", player);
            index++;
            if (index % numberOfCardsPerPlayer == 0)
            {
                player++;
                if (player == players.Count)
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
            card.transform.SetParent(players[indexOfPlayer].handHolder.transform, false);
        }
        else
        {
            card.GetComponent<CanvasGroup>().blocksRaycasts = false;
            card.GetComponent<Card>().SetImage(card.GetComponent<Card>().Back);
            card.transform.SetParent(players[indexOfPlayer].handHolder.transform, false);
        }

    }

    [Command]
    public void CmdGiveScore()
    {
        RpcGiveScore();
    }

    [ClientRpc]
    public void RpcGiveScore()
    {

        foreach (Player player1 in players)
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
                foreach (Player player in players)
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


    [Command]
    public void CmdSendGiveHolderCards()
    {
        SendGiveHolderCards();
    }

    [Server]
    private void SendGiveHolderCards()
    {
        GameManagerHearts.gameManager.timeForPlayer = false;
        foreach (Player player in players)
        {
            if (player.SelectedCard != null)
            {
                ReleaseCard(SelectedCard);
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
            int indexOfPlayer = (player.PlayerId + 1) % players.Count;
            foreach (Card card in cards)
            {
                CmdRemoveObjectAuthority(card.gameObject);
                CmdAssignPlayerAuthorityOverObject(players[indexOfPlayer], card.gameObject);
                RpcUpdateCard(card, players[indexOfPlayer].handHolder);
                RpcChangeOwnerId(card, indexOfPlayer);
            }
            RpcChangeRaycast();

        }
        GameManagerHearts.gameManager.PlayerPlayTime();
    }

    [ClientRpc]
    private void RpcChangeOwnerId(Card card, int indexOfPlayer)
    {
        card.GetComponent<Card>().OwnerId = players[indexOfPlayer].PlayerId;
    }

    [ClientRpc]
    private void RpcChangeRaycast()
    {
        foreach (Player player in players)
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
    #endregion

    public void PlayRandomCard()
    {        
        bool haveToMatch = false;
        List<Card> tmpCards = new List<Card>();
        foreach (Card card in handHolder.GetComponentsInChildren<Card>())
        {
            NetworkIdentity foundNetworkIdentity = null;
            NetworkIdentity.spawned.TryGetValue(PlayArea.playArea.FirstPlayedCard, out foundNetworkIdentity);
            if (foundNetworkIdentity)
            {
                if (NetworkIdentity.spawned[PlayArea.playArea.FirstPlayedCard].GetComponent<Card>().CardSuite == card.CardSuite)
                {
                    tmpCards.Add(card);
                    haveToMatch = true;
                }
            }
        }
        if (SelectedCard != null && !haveToMatch)
        {
            CardZone = playAreaHolder;
            StartCoroutine(WaitForAuthorityAssignment(SelectedCard));
            return;
        }
        if (SelectedCard != null && haveToMatch) {
            ReleaseCard(SelectedCard);
        }
        if (!haveToMatch)
        {
            tmpCards.Clear();
            foreach (Card card in handHolder.GetComponentsInChildren<Card>())
            {
                tmpCards.Add(card);
            }
        }
        //try
        //{
        //    soundManager.PlayCardSound("placeCard");
        //}
        //catch { }
        Card selectedCard;
        Random.InitState(DateTime.Now.Second);
        int idOfCard = Random.Range(0, tmpCards.Count - 1);
        selectedCard = tmpCards[idOfCard];
        CardZone = playAreaHolder;
        Debug.Log("selected " +selectedCard);
        SetSelectedCard(selectedCard);
        StartCoroutine(WaitForAuthorityAssignment(selectedCard));

    }

    IEnumerator WaitForAuthorityAssignment(Card selectedCard)
    {
        yield return new WaitForSeconds(0.2f);
        
        ReleaseCard(selectedCard);
    }

    public void SetSelectedCard(Card card)
    {
        
        int selectedCardIndex = card.transform.GetSiblingIndex();
        int indexOfPlayer = card.OwnerId;
        card.SetCurrentZone(card.GetComponentInParent<CardZone>());

        SelectedCard = card;

        GameObject zone = SelectedCard.transform.parent.transform.gameObject;
        CurrentCardZone = zone;

        GetFakeCard(indexOfPlayer).SetActive(true);

        GetFakeCard(indexOfPlayer).transform.SetSiblingIndex(selectedCardIndex);


        SelectedCard.transform.SetParent(Player.players[indexOfPlayer].parentHolder.transform);

        cardCount = Player.players[indexOfPlayer].handHolder.transform.childCount;

        if (selectedCardIndex + 1 < cardCount)
        {
            SelectedCard.NextCard = Player.players[indexOfPlayer].handHolder.transform.GetChild(selectedCardIndex + 1).GetComponent<Card>();
        }
        else
        {
            SelectedCard.NextCard = null;
        }

        if (selectedCardIndex - 1 >= 0)
        {
            //card that will replace me
            SelectedCard.PreviousCard = Player.players[indexOfPlayer].handHolder.transform.GetChild(selectedCardIndex - 1).GetComponent<Card>();
        }
        else
        {
            SelectedCard.PreviousCard = null;
        }
        CmdRemoveObjectAuthority(GameObject.Find("PlayAreaHolderMaster"));
        CmdAssignObjectAuthority(GameObject.Find("PlayAreaHolderMaster"));
        if (SelectedCard.hasAuthority)
        {
            CmdCheckIfPlayerCanPlayThatCard(card);
        }
        try
        {
            soundManager.PlayCardSound("playCard");
        }
        catch { }
        //GameObject.Find("PlayAreaHolderMaster").GetComponent<PlayArea>().gameObject.GetComponent<NetworkIdentity>().AssignClientAuthority(connectionToClient);
    }

    public void ReleaseCard(Card card)
    {
        int indexOfPlayer = card.OwnerId;
        
        if (SelectedCard != null)
        {
            GetFakeCard(indexOfPlayer).SetActive(false);

            CheckGivePhase();

            if (CardZone != null)
            {
                
                CmdUpdateCard(SelectedCard, CardZone);             
                if (cardZone.name.Contains("PlayAreaHolderPlayer") && players[card.OwnerId].myTurn && players[card.OwnerId].canPlayThatCard) {        
                    PlayArea playArea = GameObject.Find("PlayAreaHolderMaster").GetComponent<PlayArea>();
                    if (card.hasAuthority)
                    {
                        //CmdAssignObjectAuthority(playArea.gameObject);
                        if (playArea.hasAuthority) {
                            
                            playArea.PlayCard(this, card);
                        }
                        CmdRemoveObjectAuthority(playArea.gameObject);
                        
                    }
                    else
                    {
                        Debug.Log("nemas autoritu na kartou");
                    }
                }
                
                //players[indexOfPlayer].UpdateCard(SelectedCard, CardZone);

                //selectedCards[indexOfPlayer].transform.SetParent(cardZones[indexOfPlayer].transform);

            }
            else
            {

                CmdUpdateCard(SelectedCard, CurrentCardZone);
                //UpdateCard(SelectedCard, CurrentCardZone);
                
            }
  
            //Screen space - camera
            /*Vector3 screenPoint = fakeCard.transform.position;
            screenPoint.z = DistanceCameraPlane.z; //distance of the plane from the camera
            screenPoint.y = DistanceCameraPlane.y;
            var vector3 = Camera.main.ScreenToWorldPoint(screenPoint);*/

            SelectedCard.transform.SetSiblingIndex(fakeCard.transform.GetSiblingIndex());
            GetFakeCard(indexOfPlayer).transform.SetParent(players[indexOfPlayer].parentHolder.transform, false);

            SelectedCard = null;
            if (hasAuthority) {
                CmdCantPlayThatCard(this);
            }
        }
    }

    [Command]
    private void CmdCantPlayThatCard(Player player)
    {
        RpcCantPlayThatCard(player);
    }
    [ClientRpc]
    private void RpcCantPlayThatCard(Player player)
    {
        player.canPlayThatCard = false;
    }

    [Command]
    private void CmdCheckIfPlayerCanPlayThatCard(Card releasedcard)
    {
        Player player = players[releasedcard.OwnerId];
        int notSameSuite = 0;

        uint netId = PlayArea.playArea.FirstPlayedCard;


        NetworkIdentity foundNetworkIdentity = null;
        NetworkIdentity.spawned.TryGetValue(netId, out foundNetworkIdentity);
        if (!foundNetworkIdentity)
        {
            player.canPlayThatCard = true;
            RpcCheckIfPlayerCanPlayThatCard(releasedcard, true);
            return;
        }

        if (releasedcard.CardSuite == NetworkIdentity.spawned[netId].GetComponent<Card>().CardSuite)
        {
            player.canPlayThatCard = true;
            RpcCheckIfPlayerCanPlayThatCard(releasedcard, true);
            return;
        }
        else
        {
            foreach (Card card in player.handHolder.GetComponentsInChildren<Card>())
            {

                if (card.CardSuite == NetworkIdentity.spawned[netId].GetComponent<Card>().CardSuite)
                {
                    player.canPlayThatCard = false;
                    RpcCheckIfPlayerCanPlayThatCard(releasedcard, false);
                   
                    return;
                }
                else
                {
                    notSameSuite++;
                }

            }
            if (notSameSuite == player.handHolder.GetComponentsInChildren<Card>().Count<Card>())
            {
                player.canPlayThatCard = true;
                RpcCheckIfPlayerCanPlayThatCard(releasedcard, true); 
            }

        }  
        
    }

    [ClientRpc]
    private void RpcCheckIfPlayerCanPlayThatCard(Card releasedcard, bool decision)
    {
        foreach (Player gamer in players) {
            if (gamer.PlayerId == players[releasedcard.OwnerId].PlayerId) {
                players[releasedcard.OwnerId].canPlayThatCard = decision;
            }
        }

    }

    private void CheckGivePhase()
    {
        GameObject zone;
        if (CardZone != null)
        {
            zone = CardZone;
        }
        else
        {
            zone = CurrentCardZone;
        }

        if (phase == "GivePhase" && !zone.transform.name.Contains("GiveHolder"))
        {
            CardZone = null;
        }
    }

    [Command]
    public void CmdCheckHand()
    {
        int i = 0;
        foreach (Player player in players)
        {  
            if (player.handHolder.GetComponentsInChildren<Card>().Length == 0)
            {
                i++;
            }
        }
        
        if (i == players.Count)
        {
            PlayArea.playArea.FirstPlayedCard = 9999;
            GameManagerHearts.gameManager.giveCards = true;
        }
    }

    //Called in PlayArea

    [Command]
    public void CmdMyTurn(Player player)
    {
        RpcMyTurn(player);
        
    }

    [ClientRpc]
    private void RpcMyTurn(Player player)
    {  
        int indexOfPlayer = (player.PlayerId + 1) % players.Count;
        players[player.PlayerId].myTurn = false;
        players[indexOfPlayer].myTurn = true;
        
        if (hasAuthority)
        {
            CmdPlayerPlayTime();
        }
        
    }
    [Command]
    public void CmdPlayerPlayTime()
    {
        GameManagerHearts.gameManager.PlayerPlayTime();
    }

    [Client]
    public void MoveCard(Vector3 position, Card card)
    {
        //int indexOfPlayer = card.OwnerId;
        if (SelectedCard != null)
        {
            SelectedCard.transform.position = position;
            //CheckNextCard();
            //CheckPreviousCard();
        }

    }

    public void SetText(string msg) {
        text.text = msg;
    }

    
    [Command]
    public void CmdUpdateCard(Card card, GameObject cardZone)
    {
        RpcUpdateCard(card, cardZone);
    }

    [ClientRpc]
    public void RpcUpdateCard(Card card, GameObject cardZone)
    {      
        if (cardZone.transform.name.Contains("ShowHolder")) {
            
            card.SetImage(card.Face);
            if (!hasAuthority)
            {
                foreach (CardZone zone in GetComponentsInChildren<CardZone>())
                {
                    if (zone.name.Contains("ShowHolder"))
                    {
                        Debug.Log("Nasiel som enemyHolder");
                        cardZone = zone.gameObject;
                    }
                }
            }
        } 

        if (cardZone.transform.name.Contains("HandHolder") || cardZone.transform.name.Contains("GiveHolder")) { 
            if (card.hasAuthority) {
                card.SetImage(card.Face);
                try
                {
                    soundManager.PlayCardSound("placeCard");
                }
                catch { }
            } else {
                card.SetImage(card.Back);
            }
            if (cardZone.transform.name.Contains("GiveHolder") && cardZone.GetComponentsInChildren<Card>().Count<Card>() >= 2)
            {
                cardZone = card.CurrentZone.gameObject;

            }
            if (cardZone.transform.name.Contains("GiveHolder") && phase == "PlayPhase") {
                cardZone = card.CurrentZone.gameObject;
            }
            
        }

        if (cardZone.transform.name.Contains("PlayAreaHolder"))
        {
            
            if (players[card.OwnerId].myTurn && players[card.OwnerId].canPlayThatCard)
            {
                card.SetImage(card.Face);
                cardZone = GameObject.Find("PlayAreaHolderMaster");
            }
            if (!players[card.OwnerId].myTurn || !players[card.OwnerId].canPlayThatCard) {
                if (!card.CurrentZone)
                {
                    card.CurrentZone = players[card.OwnerId].handHolder.GetComponent<CardZone>();
                }
                else {
                    cardZone = card.CurrentZone.gameObject;
                }
            }
            if (cardZone.transform.name.Contains("PlayAreaHolderPlayer")) {
                if (!card.CurrentZone)
                {
                    card.CurrentZone = players[card.OwnerId].handHolder.GetComponent<CardZone>();
                }
                else {
                    cardZone = card.CurrentZone.gameObject;
                }
                
            }
            
        }
       
        if (cardZone.transform.name.Contains("UsedCardHolder"))
        {
            card.SetImage(card.Back);
        }

        card.transform.SetParent(cardZone.transform, false);
        //card.SetCurrentZone(cardZone.GetComponent<CardZone>());   
        card.transform.position = new Vector3(cardZone.transform.position.x, cardZone.transform.position.y, cardZone.transform.position.z);
        card.transform.rotation = cardZone.transform.rotation;

        if (cardZone.name == "PlayAreaHolderMaster")
        {
            card.transform.localScale = new Vector2(1.4f, 1.9f);
            foreach (Player player in players) {
                if (player.hasAuthority) {
                    try
                    {
                        soundManager.PlayCardSound("placeCard");
                    }
                    catch { }
                }
            }  
        }
        else
        {
            card.transform.localScale = new Vector2(1f, 1f);
        }



        if (hasAuthority) {
            CmdCheckHand();
        }

    }
    
    [Client]
    private GameObject GetFakeCard(int indexOfPlayer)
    {
        if (fakeCard != null)
        {
            if (fakeCard.transform.parent != players[indexOfPlayer].handHolder.transform)
            {
                fakeCard.transform.SetParent(players[indexOfPlayer].handHolder.transform);
            }

        }
        else
        {
            fakeCard = Instantiate(fakeCardPrefab);
            fakeCard.name = "Fake";
            fakeCard.transform.SetParent(players[indexOfPlayer].handHolder.transform);
        }

        return fakeCard;
    }

    [Command]
    public void CmdAssignPlayerAuthorityOverObject(Player player, GameObject gameObject)
    {
        gameObject.GetComponent<NetworkIdentity>().AssignClientAuthority(player.connectionToClient);
    }

    [Command]
    public void CmdAssignObjectAuthority(GameObject gameObject)
    {
        gameObject.GetComponent<NetworkIdentity>().AssignClientAuthority(connectionToClient);
    }
    
    [Command]
    public void CmdRemoveObjectAuthority(GameObject gameObject)
    {
        gameObject.GetComponent<NetworkIdentity>().RemoveClientAuthority();
    }

    [Server]
    public void ServerAssignPlayerAuthorityOverObject(Player player, GameObject gameObject)
    {
        gameObject.GetComponent<NetworkIdentity>().AssignClientAuthority(player.connectionToClient);
    }

    [Server]
    public void ServerAssignObjectAuthority(GameObject gameObject)
    {
        gameObject.GetComponent<NetworkIdentity>().AssignClientAuthority(connectionToClient);
    }

    [Server]
    public void ServerRemoveObjectAuthority(GameObject gameObject)
    {
        gameObject.GetComponent<NetworkIdentity>().RemoveClientAuthority();
    }

    public void AddNameHolder(GameObject nameHolder)
    {
        ScoreBoardNameHolders.Add(nameHolder);
    }

    public void AddScoreHolder(GameObject scoreHolder)
    {
        ScoreBoardScoreHolders.Add(scoreHolder);
    }

    public void StopAll()
    {
        CmdStopAll();
    }
    [Command]
    private void CmdStopAll()
    {
        RpcStopAll();
    }
    [ClientRpc]
    private void RpcStopAll()
    {
        if (!IsHost) {
            Room.StopClient();
        }
    }
}
