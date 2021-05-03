using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class OfflineGameManagerHearts : MonoBehaviour
{
    public static OfflineGameManagerHearts instanceOfflineManager;
    [SerializeField] public GameObject player, enemy1, enemy2, enemy3, cardPrefab, fakeCardPrefab, roundPrefab, playerScorePrefab, panel;

    //public int playerId = 0;
    public List<GameObject> players = new List<GameObject>();
    public bool endOfGame = false;

    private OfflineCard firstPlayedCard;
    public OfflineCard FirstPlayedCard
    {
        get { return firstPlayedCard; }
        set { firstPlayedCard = value; }
    }

    private OfflineCard highestPlayedCard;
    public OfflineCard HighestPlayedCard
    {
        get { return highestPlayedCard; }
        set { highestPlayedCard = value; }
    }

    private List<OfflineCard> playedCards;
    private List<GameObject> losingPlayers;

    private Sprite cardBack;
    private List<GameObject> cards;
    private List<Sprite> spritesForCards;
    private bool startedGame = true;
    private bool decideWhoTakes;
    private int interval = 2;
    private float playTime;
    private bool outOutOfCards;
    private int currentRound = 1;

    private List<string> botNames = new List<string>(new string[] { "James","John","Robert","Michael","William","David","Richard","Joseph","Thomas","Charles","Christopher",
                                                                        "Daniel","Matthew","Anthony","Donald","Mark","Paul","Steven","Andrew","Kenneth","Joshua","Kevin",
                                                                         "Brian","Edward","Ronald","Timothy","Jason","Jeffrey","Ryan","Jacob","Gary","Nicholas","Eric",
                                                                        "Jonathan","Stephen","Larry","Justin","Scott","Brandon","Mary","Patricia","Jennifer","Linda","Elizabeth",
                                                                         "Barbara","Susan","Jessica","Sarah","Karen","Nancy","Lisa","Margaret","Betty","Sandra","Ashley",
                                                                        "Dorothy","Kimberly","Emily","Donna","Michelle","Carol","Amanda","Melissa","Deborah","Stephanie",
                                                                        "Rebecca","Laura","Sharon","Cynthia","Kathleen","Amy","Shirley","Angela","Helen","Anna","Brenda",
                                                                        "Pamela","Nicole","Samantha"});
    private bool setScoreBoardNames;
    private float nextTime;
    private bool timerStart;

    public List<string> BotNames {
        get { return botNames; }
    }
        
    private void Awake()
    {
        if (instanceOfflineManager == null)
        {
            instanceOfflineManager = this;
        }
    }

    private void Start()
    {
        playedCards = new List<OfflineCard>();
        spritesForCards = new List<Sprite>();
        cards = new List<GameObject>();

        
    }

    private void Update()
    {
        if (players.Count == 4 && startedGame && !endOfGame) {
            if (!setScoreBoardNames) {
                SetScoreBoardNames();
                player.GetComponent<OfflinePlayer>().allPoints = 0;
                player.GetComponent<OfflinePlayer>().allCards = 0;
            }
            SetPlayersId();
            OfflineTimer.timer.StartTime("GiveCards");
            LoadCards("Sprites/Cards");
            SpawnCards();
            SuffleCards();
            AssignCards();
            player.GetComponent<OfflinePlayer>().myTurn = false;
            startedGame = false;
            player.GetComponent<OfflinePlayer>().Phase = "GivePhase";
        }

        if (player.GetComponent<OfflinePlayer>().myTurn && !timerStart)
        {
            timerStart = true;
            OfflineTimer.timer.StartTime("PlayerTime");
        }
        if (!player.GetComponent<OfflinePlayer>().myTurn)
        {
            timerStart = false;
        }

        if (Time.time >= playTime)
        {
            if (decideWhoTakes)
            {
                DecideWhoTakes();
                playedCards.Clear();
                decideWhoTakes = false;
                
            }  
        }

        if (endOfGame) {
            player.GetComponent<OfflinePlayer>().endOfGame = true;
            SetTurnToPlayer(-1);
            //endOfGame = false;
            outOutOfCards = false;
            panel.SetActive(true);
        }

        if (Input.GetKey(KeyCode.Escape) || endOfGame) {
            panel.SetActive(true);
        } else
        {
            panel.SetActive(false);
        }

        
        if (outOutOfCards)
        {
            if (Time.time >= nextTime)
            {
                //CountScore();
                if (!player.GetComponent<OfflinePlayer>().endOfGame && !endOfGame)
                {
                    SetTurnToPlayer(-1);
                    OfflineTimer.timer.StartTime("GiveCards");
                    outOutOfCards = false;
                    
                    SuffleCards();
                    AssignCards();
                    player.GetComponent<OfflinePlayer>().Phase = "GivePhase";
                }
                
                nextTime += 1;
            }

        }
        
    }


    private void SetScoreBoardNames()
    {
        List<Transform> nameHolders = new List<Transform>();
        foreach (Transform transform in player.GetComponent<OfflinePlayer>().scoreBoard.GetComponentsInChildren<Transform>())
        {
            if (transform.gameObject.name.Contains("Panel_PlayerName"))
            {
                nameHolders.Add(transform.gameObject.transform);

            }

        }
        int index = 0;
        foreach (Transform transform in nameHolders)
        {

            if (index < players.Count)
            {
                transform.gameObject.GetComponentInChildren<TextMeshProUGUI>().text = players[index].name;
            }
            index++;
        }
    }

    private void SetPlayersId()
    {
        for (int i = 0; i < players.Count; i++) {
            if (players[i].GetComponent<OfflineBot>())
            {
                players[i].GetComponent<OfflineBot>().PlayerId = i;
            }
            if (players[i].GetComponent<OfflinePlayer>())
            {
                players[i].GetComponent<OfflinePlayer>().PlayerId = i;
            }
        }
    }

    private void LoadCards(string path)
    {
        Sprite[] cardSprites;
        cardSprites = Resources.LoadAll<Sprite>(path);

        foreach (Sprite card in cardSprites)
        {
            spritesForCards.Add(card);

        }
    }

    private void SpawnCards()
    {
        cards = new List<GameObject>();
        cardBack = spritesForCards.Find(item => item.name.Contains("background"));
        spritesForCards.RemoveAll(item => item.name.Contains("background"));
        string[] splitSpriteName;

        
        foreach (Sprite cardSprite in spritesForCards)
        {
            splitSpriteName = cardSprite.name.Split("_"[0]);
            GameObject card = Instantiate(cardPrefab);
            card.name = "Card " + splitSpriteName[0] + " " + splitSpriteName[1];
            cards.Add(card);
            card.GetComponent<OfflineCard>().SetImage(cardSprite);
            card.GetComponent<OfflineCard>().CardValue = EnumCardValue.getValue(splitSpriteName[0]);
            card.GetComponent<OfflineCard>().CardSuite = splitSpriteName[1];
            card.GetComponent<OfflineCard>().Face = cardSprite;
            
        }
    }

    private void AssignCards()
    {
        int index = 0;
        int player = 0;

        foreach (GameObject cardGM in cards)
        {
            OfflineCard card = cardGM.GetComponent<OfflineCard>();

            if (players[player].GetComponent<OfflinePlayer>())
            {
                card.transform.SetParent(players[player].GetComponent<OfflinePlayer>().handHolder.transform, false);
                card.GetComponent<OfflineCard>().OwnerId = players[player].GetComponent<OfflinePlayer>().PlayerId;
                card.transform.rotation = card.transform.parent.rotation;
                card.transform.eulerAngles = new Vector3(card.transform.eulerAngles.x + 20, card.transform.eulerAngles.y, card.transform.eulerAngles.z);

            }
            if (players[player].GetComponent<OfflineBot>())
            {
                card.transform.SetParent(players[player].GetComponent<OfflineBot>().handHolder.transform, false);
                card.GetComponent<OfflineCard>().OwnerId = players[player].GetComponent<OfflineBot>().PlayerId;
                card.transform.rotation = card.transform.parent.rotation;
                card.transform.eulerAngles = new Vector3(card.transform.eulerAngles.x + 20, card.transform.eulerAngles.y, card.transform.eulerAngles.z);
            }

            if (card.GetComponent<OfflineCard>().OwnerId != this.player.GetComponent<OfflinePlayer>().PlayerId)
            {
                card.GetComponent<OfflineCard>().SetImage(cardBack);
            }
            else {
                card.GetComponent<OfflineCard>().SetImage(card.Face);
            }
            index++;
            if (index % 8 == 0)
            {
                player++;
                if (player == players.Count)
                {
                    break;
                }
            }
        }

    }

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

    public void PlayCard(OfflineCard selectedCard, GameObject player)
    {
        selectedCard.SetImage(selectedCard.Face);
        if (playedCards.Count == 0) {
            FirstPlayedCard = selectedCard;
            HighestPlayedCard = selectedCard;
        }
        if (HighestPlayedCard.CardValue < selectedCard.CardValue)
        {
            HighestPlayedCard = selectedCard;
        }
        playedCards.Add(selectedCard);

        if (playedCards.Count != players.Count) {
            ChangeTurn(player);
        }
        if (playedCards.Count == players.Count) {
            decideWhoTakes = true;
            playTime = Time.time + interval;
        }
    }

    public void ChangeTurn(GameObject player)
    {
        int id;
        if (player.GetComponent<OfflinePlayer>()) {
            id = player.GetComponent<OfflinePlayer>().PlayerId;
            int indexOfPlayer = (id + 1) % players.Count;
            players[indexOfPlayer].GetComponent<OfflineBot>().myTurn = true;
        }
        if (player.GetComponent<OfflineBot>()) {
            id = player.GetComponent<OfflineBot>().PlayerId;
            int indexOfPlayer = (id + 1) % players.Count;
            if (players[indexOfPlayer].GetComponent<OfflineBot>())
            {
                players[indexOfPlayer].GetComponent<OfflineBot>().myTurn = true;
            }
            if (players[indexOfPlayer].GetComponent<OfflinePlayer>()) 
            {
                players[indexOfPlayer].GetComponent<OfflinePlayer>().myTurn = true;
            }
            
        }
    }

    public void PlaceRandomCardsForBotsIntoGiveHolder()
    {
        foreach (GameObject player in players)
        {
            if (player.GetComponent<OfflineBot>())
            {
                player.GetComponent<OfflineBot>().PlaceRandomCardsIntoGiveHolder();
            }
        }
    }

    public void SendGiveHolderCards()
    {
        CheckPlayerGiveHolder();
        for (int i = 0; i < players.Count; i++) {
            int indexOfPlayer = (i + 1) % players.Count;
            //Will be bot
            if (players[indexOfPlayer].GetComponent<OfflineBot>()) { 
                if (players[i].GetComponent<OfflineBot>())
                {
                    foreach (OfflineCard card in players[i].GetComponent<OfflineBot>().giveHolder.GetComponentsInChildren<OfflineCard>())
                    {
                        card.transform.SetParent(players[indexOfPlayer].GetComponent<OfflineBot>().handHolder.transform, false);
                        card.OwnerId = players[indexOfPlayer].GetComponent<OfflineBot>().PlayerId;
                        
                    }
                }
                if (players[i].GetComponent<OfflinePlayer>())
                {
                    foreach (OfflineCard card in players[i].GetComponent<OfflinePlayer>().giveHolder.GetComponentsInChildren<OfflineCard>())
                    {
                        card.transform.SetParent(players[indexOfPlayer].GetComponent<OfflineBot>().handHolder.transform, false);
                        card.OwnerId = players[indexOfPlayer].GetComponent<OfflineBot>().PlayerId;
                        card.SetImage(cardBack);
                    }
                }
            }
            //Will be player
            if (players[indexOfPlayer].GetComponent<OfflinePlayer>())
            {
                foreach (OfflineCard card in players[i].GetComponent<OfflineBot>().giveHolder.GetComponentsInChildren<OfflineCard>()) {
                    card.transform.SetParent(player.GetComponent<OfflinePlayer>().handHolder.transform, false);
                    card.OwnerId = players[indexOfPlayer].GetComponent<OfflinePlayer>().PlayerId;
                    card.SetImage(card.Face);
                } 
            }
        }
        player.GetComponent<OfflinePlayer>().Phase = "PlayPhase";
        player.GetComponent<OfflinePlayer>().myTurn = true;
        
    }

    private void CheckPlayerGiveHolder()
    {
        int numberOfCards = player.GetComponent<OfflinePlayer>().giveHolder.GetComponentsInChildren<OfflineCard>().Length;
        if (numberOfCards < 2) {
            player.GetComponent<OfflinePlayer>().FillPlayerGiveHolder(2 - numberOfCards);
        }
    }

    private void DecideWhoTakes()
    {
        
        OfflineCard highestCard = FirstPlayedCard;

        foreach (OfflineCard card in playedCards) {
            if (card.CardSuite == FirstPlayedCard.CardSuite) {
                if (card.CardValue > highestCard.CardValue) {
                    highestCard = card;
                }
            }
        }
        FirstPlayedCard = null;
        GiveWonCardToPlayer(highestCard.OwnerId);
        SetTurnToPlayer(highestCard.OwnerId);
        
    }

    private void SetTurnToPlayer(int ownerId)
    {

        foreach (GameObject player in players) {
            if (player.GetComponent<OfflineBot>())
            {
                player.GetComponent<OfflineBot>().myTurn = false;
            }
            if (player.GetComponent<OfflinePlayer>())
            {
                player.GetComponent<OfflinePlayer>().myTurn = false;
            }
        }
        if (ownerId != -1)
        {
            if (players[ownerId].GetComponent<OfflineBot>())
            {
                players[ownerId].GetComponent<OfflineBot>().myTurn = true;
            }
            if (players[ownerId].GetComponent<OfflinePlayer>())
            {
                players[ownerId].GetComponent<OfflinePlayer>().myTurn = true;
                OfflineTimer.timer.StartTime("PlayerTime");
            }
        }
        
    }

    private void GiveWonCardToPlayer(int ownerId)
    {
        foreach (OfflineCard card in playedCards)
        {
            if (players[ownerId].GetComponent<OfflineBot>())
            {
                card.transform.SetParent(players[ownerId].GetComponent<OfflineBot>().usedCardHolder.transform, false);
                card.transform.localScale = new Vector3(1f, 1f, 1f);
                card.SetImage(cardBack);
            }
            if (players[ownerId].GetComponent<OfflinePlayer>())
            {
                card.transform.SetParent(players[ownerId].GetComponent<OfflinePlayer>().usedCardHolder.transform, false);
                card.transform.localScale = new Vector3(1f, 1f, 1f);
                card.SetImage(cardBack);
            }
        }
        CheckNumberOfCards();
    }

    private void CheckNumberOfCards()
    {
        int count = 0;
        foreach (GameObject player in players) {
            if (player.GetComponent<OfflineBot>())
            {
                if (player.GetComponent<OfflineBot>().handHolder.GetComponentsInChildren<OfflineCard>().Length == 0) {
                    count++;
                }
            }
            if (player.GetComponent<OfflinePlayer>())
            {
                if (player.GetComponent<OfflinePlayer>().handHolder.GetComponentsInChildren<OfflineCard>().Length == 0)
                {
                    count++;
                }
            }
        }
        if (count == players.Count) {
            CountScore();
            outOutOfCards = true;
        }
    }

    private void CountScore()
    {
        GameObject playerWithAll = null;
        foreach (GameObject player in players)
        {
            if (player.GetComponent<OfflineBot>())
            {
                if (player.GetComponent<OfflineBot>().usedCardHolder.GetComponentsInChildren<OfflineCard>().Length == cards.Count) {
                    playerWithAll = player;
                    break;
                }
            }
            if (player.GetComponent<OfflinePlayer>())
            {
                if (player.GetComponent<OfflinePlayer>().usedCardHolder.GetComponentsInChildren<OfflineCard>().Length == cards.Count)
                {
                    player.GetComponent<OfflinePlayer>().allCards++;
                    playerWithAll = player;
                    break;
                }
            }
        }
        
        if (playerWithAll != null) {
            Debug.Log(playerWithAll.name);
            foreach (GameObject player in players)
            {
                if (playerWithAll == player)
                {
                    if (player.GetComponent<OfflineBot>())
                    {
                        player.GetComponent<OfflineBot>().TotalScore -= 32;
                    }
                    if (player.GetComponent<OfflinePlayer>())
                    {
                        player.GetComponent<OfflinePlayer>().TotalScore -= 32;
                    }
                }
                else {
                    if (player.GetComponent<OfflineBot>())
                    {
                        player.GetComponent<OfflineBot>().TotalScore += 32;
                    }
                    if (player.GetComponent<OfflinePlayer>())
                    {
                        player.GetComponent<OfflinePlayer>().TotalScore += 32;
                    }
                }
            }
            
            PutScoreOnScoreBoard();
            return;
        }
        int score = 0;
        foreach (GameObject player in players)
        {
            
            if (player.GetComponent<OfflineBot>())
            {
                player.GetComponent<OfflineBot>().TotalScore += GetPlayerScore(player.GetComponent<OfflineBot>().usedCardHolder.GetComponentsInChildren<OfflineCard>()); ;
            }
            if (player.GetComponent<OfflinePlayer>())
            {
                player.GetComponent<OfflinePlayer>().TotalScore += GetPlayerScore(player.GetComponent<OfflinePlayer>().usedCardHolder.GetComponentsInChildren<OfflineCard>());
                score += GetPlayerScore(player.GetComponent<OfflinePlayer>().usedCardHolder.GetComponentsInChildren<OfflineCard>());
            }
        }
        if (score == 32) {
            player.GetComponent<OfflinePlayer>().allPoints++;
        }
        CheckIfSomeoneLostGame();
        PutScoreOnScoreBoard();
    }

    private void PutScoreOnScoreBoard()
    {
        GameObject scoreBoard = player.GetComponentInChildren<ScoreBoard>().gameObject;

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
                        transform.gameObject.GetComponent<TextMeshProUGUI>().text = currentRound.ToString();
                    }
                }

            }
            foreach (GameObject player in players)
            {
                if (player.GetComponent<OfflinePlayer>()) {
                    if (gameObject.gameObject.name == "Panel_Player" + player.GetComponent<OfflinePlayer>().PlayerId)
                    {
                        GameObject score = Instantiate(playerScorePrefab);
                        score.transform.SetParent(gameObject, false);
                        foreach (Transform transform in score.GetComponentsInChildren<Transform>())
                        {
                            if (transform.name == "PlayerScore")
                            {
                                transform.gameObject.GetComponent<TextMeshProUGUI>().text = player.GetComponent<OfflinePlayer>().TotalScore.ToString();
                            }
                        }
                    }
                }

                if (player.GetComponent<OfflineBot>())
                {
                    if (gameObject.gameObject.name == "Panel_Player" + player.GetComponent<OfflineBot>().PlayerId)
                    {
                        GameObject score = Instantiate(playerScorePrefab);
                        score.transform.SetParent(gameObject, false);
                        foreach (Transform transform in score.GetComponentsInChildren<Transform>())
                        {
                            if (transform.name == "PlayerScore")
                            {
                                transform.gameObject.GetComponent<TextMeshProUGUI>().text = player.GetComponent<OfflineBot>().TotalScore.ToString();
                            }
                        }
                    }
                }

            }
        }
        currentRound++;
        //CheckIfSomeoneLostGame();
    }

    private void CheckIfSomeoneLostGame()
    {
        int losingScore = 1;
        List<GameObject> losingPlayers = new List<GameObject>();
        foreach (GameObject player in players)
        {
            if (player.GetComponent<OfflineBot>())
            {
                OfflineBot bot = player.GetComponent<OfflineBot>();
                if (player.GetComponent<OfflineBot>().TotalScore >= losingScore)
                {

                    losingPlayers.Add(player);
                    bot.win = false;
                }
                else {
                    bot.win = true;
                }
            }
            if (player.GetComponent<OfflinePlayer>())
            {
                OfflinePlayer offlinePlayer = player.GetComponent<OfflinePlayer>();

                if (player.GetComponent<OfflinePlayer>().TotalScore >= losingScore) {
                    
                    losingPlayers.Add(player);
                    offlinePlayer.win = false;
                } else
                {
                    offlinePlayer.win = true;
                }
            }
        }
        if (losingPlayers.Count != 0) {
            endOfGame = true;
            WritePlayersScore();
            WritePlayerStats();
        } 
        
    }
    private int GetPlayerScore(OfflineCard[] offlineCards)
    {
        int score = 0;

        foreach (OfflineCard card in offlineCards)
        {
            if (card.name.Contains("heart"))
            {
                score++;
            }
            if (card.name.Contains("upperKnave leaf"))
            {
                score += 12;

            }
            if (card.name.Contains("upperKnave acorn"))
            {
                score += 8;

            }
            if (card.name.Contains("upperKnave ball"))
            {
                score += 4;

            }
        }
        return score;
    }

    private void WritePlayerStats()
    {
        string folder = Settings.folderPath;
        string textFolder = Settings.folderPath + @"\PlayerStats.txt";
        try
        {
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

        }
        catch { }
        if (!File.Exists(textFolder)) {
            CreateFile(textFolder);
        }
        
        List<string> newLines = new List<string>();
        int file_line = 0;

        StreamReader sr_temp = new StreamReader(textFolder);
        string line;
        string tmpStr;
        while ((line = sr_temp.ReadLine()) != null)
        {
            tmpStr = Encryption.EncryptDecrypt(line);
            string[] lineComp = tmpStr.Split(";"[0]);
            int number;
            switch (lineComp[0])
            {
                case "wins":
                    number = Int32.Parse(lineComp[1]);
                    if (player.GetComponent<OfflinePlayer>().win)
                    {
                        number++;
                    }
                    newLines.Add("wins;" + number);
                    break;
                case "loses":
                    number = Int32.Parse(lineComp[1]);
                    if (!player.GetComponent<OfflinePlayer>().win)
                    {
                        number++;
                    }
                    newLines.Add("loses;" + number);
                    break;
                case "allCards":
                    number = Int32.Parse(lineComp[1]);
                    number += player.GetComponent<OfflinePlayer>().allCards;
                    newLines.Add("allCards;" + number);
                    break;
                case "allPoints":
                    number = Int32.Parse(lineComp[1]);
                    number += player.GetComponent<OfflinePlayer>().allPoints;
                    newLines.Add("allPoints;" + number);
                    break;
                case "playedGames":
                    number = Int32.Parse(lineComp[1]);
                    number++;
                    newLines.Add("playedGames;" + number);
                    break;
                case "lessThan50":
                    number = Int32.Parse(lineComp[1]);
                    if (player.GetComponent<OfflinePlayer>().TotalScore < 50)
                    {
                        number++;
                    }
                    newLines.Add("lessThan50;" + number);
                    break;
                case "lessThan25":
                    number = Int32.Parse(lineComp[1]);
                    if (player.GetComponent<OfflinePlayer>().TotalScore < 25)
                    {
                        number++;
                    }
                    newLines.Add("lessThan25;" + number);
                    break;
                default:
                    break;
            }
            file_line++;
        }

        sr_temp.Close();

        StreamWriter sw = new StreamWriter(textFolder);
        for (int i = 0; i < file_line; i++)
        {
            tmpStr = Encryption.EncryptDecrypt(newLines[i]);
            sw.WriteLine(tmpStr);
        }

        sw.Close();
    }


    //private void CreateFile(string textFolder)
    //{
    //        File.AppendAllText(textFolder, "wins;0" + Environment.NewLine + "loses;0" + Environment.NewLine + "allCards;0" + Environment.NewLine + "allPoints;0" + Environment.NewLine
    //                                    + "playedGames;0" + Environment.NewLine + "lessThan50;0" + Environment.NewLine + "lessThan25;0" + Environment.NewLine);
    //}

    private void CreateFile(string textFolder)
    {
        string tmpStr = "wins;0";
        tmpStr = Encryption.EncryptDecrypt(tmpStr);
        File.AppendAllText(textFolder, tmpStr + Environment.NewLine);
        tmpStr = "loses;0";
        tmpStr = Encryption.EncryptDecrypt(tmpStr);
        File.AppendAllText(textFolder, tmpStr + Environment.NewLine);
        tmpStr = "allCards;0";
        tmpStr = Encryption.EncryptDecrypt(tmpStr);
        File.AppendAllText(textFolder, tmpStr + Environment.NewLine);
        tmpStr = "allPoints;0";
        tmpStr = Encryption.EncryptDecrypt(tmpStr);
        File.AppendAllText(textFolder, tmpStr + Environment.NewLine);
        tmpStr = "playedGames;0";
        tmpStr = Encryption.EncryptDecrypt(tmpStr);
        File.AppendAllText(textFolder, tmpStr + Environment.NewLine);
        tmpStr = "lessThan50;0";
        tmpStr = Encryption.EncryptDecrypt(tmpStr);
        File.AppendAllText(textFolder, tmpStr + Environment.NewLine);
        tmpStr = "lessThan25;0";
        tmpStr = Encryption.EncryptDecrypt(tmpStr);
        File.AppendAllText(textFolder, tmpStr + Environment.NewLine);

    }

    private void WritePlayersScore() {
        string folder = Settings.folderPath;
        string textFolder = Settings.folderPath + @"\GameScore.txt";
        try
        {
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

        }
        catch { }
        string tmpStr;
        tmpStr = "newTable;";
        tmpStr = Encryption.EncryptDecrypt(tmpStr);
        File.AppendAllText(textFolder, tmpStr + Environment.NewLine);
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].GetComponent<OfflineBot>()) {

                tmpStr = "name;" + players[i].name + ";score;" + players[i].GetComponent<OfflineBot>().TotalScore.ToString() + ";playerID;" + players[i].GetComponent<OfflineBot>().PlayerId + ";";
                tmpStr = Encryption.EncryptDecrypt(tmpStr);
                File.AppendAllText(textFolder, tmpStr + Environment.NewLine);
            }
            if (players[i].GetComponent<OfflinePlayer>())
            {

                tmpStr = "name;" + players[i].name + ";score;" + players[i].GetComponent<OfflinePlayer>().TotalScore.ToString() + ";playerID;" + players[i].GetComponent<OfflinePlayer>().PlayerId + ";";
                tmpStr = Encryption.EncryptDecrypt(tmpStr);
                File.AppendAllText(textFolder, tmpStr + Environment.NewLine);
            }
        }
        tmpStr = "round;" + currentRound + ";";
        tmpStr = Encryption.EncryptDecrypt(tmpStr);
        File.AppendAllText(textFolder, tmpStr + Environment.NewLine);
        if (player.GetComponent<OfflinePlayer>().win)
        {
            tmpStr = "result;" + "Win" + ";";
        }
        else {
            tmpStr = "result;" + "Lose" + ";";
        }
        tmpStr = Encryption.EncryptDecrypt(tmpStr);
        File.AppendAllText(textFolder, tmpStr + Environment.NewLine);
        //File.AppendAllText(textFolder, Environment.NewLine);
    }
}
