using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class OfflinePlayer : MonoBehaviour
{
    [SerializeField] public GameObject parentHolder, handHolder, giveHolder, usedCardHolder, playAreaHolder, fakeCardPrefab, scoreBoard;
    [SerializeField] public TMP_Text text;

    private GameObject fakeCard;
    private OfflineCard selectedCard, previousCard, nextCard;
    public OfflineCard SelectedCard { 
        get { return selectedCard; } 
    }

    private GameObject cardZone;
    public GameObject CardZone
    {
        get { return cardZone; }
        set { cardZone = value; }
    }

    private int roundScore = 0;
    public int RoundScore
    {
        get { return roundScore; }
        set { roundScore = value; }
    }

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

    private GameObject currentZone;
    private int cardCountInHand;

    public bool endOfGame = false;
    public bool myTurn = false;
    private int playerId;
    private SoundManager soundManager;
   

    public int PlayerId
    {
        get { return playerId; }
        set { playerId = value; }
    }

    public bool win { get; internal set; }
    public int allCards { get; internal set; }
    public int allPoints { get; internal set; }


    void Start()
    {
        try
        {
            soundManager = GameObject.Find("SoundManager(Clone)").GetComponent<SoundManager>();
        }
        catch { }
        gameObject.name = PlayerPrefs.GetString("PlayerNameKey"); 
        List<GameObject> tmpPlayers = new List<GameObject>();
        tmpPlayers = OfflineGameManagerHearts.instanceOfflineManager.players;
        tmpPlayers.Add(this.gameObject);
        tmpPlayers = tmpPlayers.OrderBy(x => x.transform.GetSiblingIndex()).ToList();
        OfflineGameManagerHearts.instanceOfflineManager.players = tmpPlayers;

    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.Tab) && !OfflineGameManagerHearts.instanceOfflineManager.endOfGame && !endOfGame)
        {
            scoreBoard.transform.position = new Vector3(scoreBoard.transform.position.x, scoreBoard.transform.position.y, -250);
            scoreBoard.GetComponent<RectTransform>().anchorMin = new Vector2(0.3f, 0.3f);
            scoreBoard.GetComponent<RectTransform>().anchorMax = new Vector2(0.7f, 0.7f);
            scoreBoard.transform.transform.localScale = new Vector3(1, 1, 1);
        } else {
            scoreBoard.transform.transform.localScale = new Vector3(0, 0, 0);
        }
        
        if (endOfGame || Input.GetKey(KeyCode.Escape))
        {
            scoreBoard.transform.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            scoreBoard.transform.position = new Vector3(scoreBoard.transform.position.x, scoreBoard.transform.position.y, -300);
            scoreBoard.GetComponent<RectTransform>().anchorMin = new Vector2(0.4f, 0.4f);
            scoreBoard.GetComponent<RectTransform>().anchorMax = new Vector2(0.8f, 0.8f);
        }


    }

    public void SetSelectedCard(OfflineCard offlineCard)
    {
        int selectedCardIndex = offlineCard.transform.GetSiblingIndex();
        selectedCard = offlineCard;
        //selectedCard.index = selectedCardIndex;
        currentZone = selectedCard.transform.parent.transform.gameObject;

        GetFakeCard().SetActive(true);
        GetFakeCard().transform.SetSiblingIndex(selectedCardIndex);

        selectedCard.transform.SetParent(parentHolder.transform);

        cardCountInHand = handHolder.transform.childCount;

        if (selectedCardIndex + 1 < cardCountInHand)
        {
            nextCard = handHolder.transform.GetChild(selectedCardIndex + 1).GetComponent<OfflineCard>();
        }
        else
        {
            nextCard = null;
        }

        if (selectedCardIndex - 1 >= 0)
        {
            //card that will replace me
            previousCard = handHolder.transform.GetChild(selectedCardIndex - 1).GetComponent<OfflineCard>();
        }
        else
        {
            previousCard = null;
        }

        try
        {
            soundManager.PlayCardSound("playCard");
        }
        catch { }
    }

    public void ReleaseCard()
    {
        if (selectedCard != null)
        {
            GetFakeCard().SetActive(false);


            if (CardZone != null)
            {
                try
                {
                    soundManager.PlayCardSound("placeCard");
                }
                catch { }
                CheckCriteria();
                selectedCard.transform.SetParent(CardZone.transform);
                if (CardZone.transform.name == "PlayAreaHolder")
                {

                    //selectedCard.setAspect(true);
                    //selectedCard.transform.position = new Vector3(150, selectedCard.transform.position.y, selectedCard.transform.position.z);
                    //selectedCard.transform.Rotate(-20, 0, 0);
                    selectedCard.transform.rotation = Quaternion.Euler(70, 0, 0);
                    selectedCard.transform.position = new Vector3(playAreaHolder.transform.position.x, playAreaHolder.transform.position.y, 0);
                    selectedCard.transform.localScale = new Vector2(1.2f, 1.7f);
                    myTurn = false;
                    OfflineGameManagerHearts.instanceOfflineManager.PlayCard(selectedCard, this.gameObject);
                }
                else
                {
                    selectedCard.transform.localScale = new Vector3(1f, 1f, 1f);
                    //selectedCard.setAspect(false);
                    selectedCard.transform.position = fakeCard.transform.position;
                }
            }
            else
            {
                selectedCard.transform.SetParent(currentZone.transform, false);
                selectedCard.transform.position = fakeCard.transform.position;
            }

            selectedCard.transform.SetSiblingIndex(fakeCard.transform.GetSiblingIndex());
            GetFakeCard().transform.SetParent(parentHolder.transform, false);

            
            selectedCard = null;
        }
    }

    public void PlayRandomCard()
    {
        bool haveToMatch = false;
        List<OfflineCard> tmpOfflineCards = new List<OfflineCard>();
        foreach (OfflineCard card in handHolder.GetComponentsInChildren<OfflineCard>())
        {
            if (OfflineGameManagerHearts.instanceOfflineManager.FirstPlayedCard != null)
            {
                if (OfflineGameManagerHearts.instanceOfflineManager.FirstPlayedCard.CardSuite == card.CardSuite)
                {
                    tmpOfflineCards.Add(card);
                    haveToMatch = true;
                }
            }
        }
        if (!haveToMatch)
        {
            tmpOfflineCards.Clear();
            foreach (OfflineCard card in handHolder.GetComponentsInChildren<OfflineCard>())
            {
                tmpOfflineCards.Add(card);
            }
        }
        try
        {
            soundManager.PlayCardSound("placeCard");
        }
        catch { }
        OfflineCard selectedCard;
        Random.InitState(DateTime.Now.Second);
        int idOfCard = Random.Range(0, tmpOfflineCards.Count - 1);
        selectedCard = tmpOfflineCards[idOfCard];
        selectedCard.transform.SetParent(FindObjectOfType<OfflinePlayer>().playAreaHolder.transform, false);
        selectedCard.transform.localScale = new Vector2(1.2f, 1.7f);
        selectedCard.transform.rotation = Quaternion.Euler(70, 0, 0);
        OfflineGameManagerHearts.instanceOfflineManager.PlayCard(selectedCard, this.gameObject);

    }

    private void CheckCriteria()
    {
        if (CardZone.transform.name == "GiveHolder" && giveHolder.GetComponentsInChildren<OfflineCard>().Length == 2 && Phase == "GivePhase")
        {
            CardZone = currentZone;
        }

        if (CardZone.transform.name == "PlayAreaHolder" && Phase == "GivePhase")
        {
            CardZone = currentZone;
        }

        if (CardZone.transform.name == "GiveHolder" && Phase == "PlayPhase")
        {
            CardZone = currentZone;
        }

        if (CardZone.transform.name == "PlayAreaHolder" && !myTurn)
        {
            CardZone = currentZone;
        }


        if (OfflineGameManagerHearts.instanceOfflineManager.FirstPlayedCard != null)
        {

            bool haveToMatch = false;
            foreach (OfflineCard card in handHolder.GetComponentsInChildren<OfflineCard>())
            {
                if (card.CardSuite == OfflineGameManagerHearts.instanceOfflineManager.FirstPlayedCard.CardSuite)
                {
                    haveToMatch = true;
                }
            }

            if (haveToMatch && Phase == "PlayPhase" && selectedCard.CardSuite != OfflineGameManagerHearts.instanceOfflineManager.FirstPlayedCard.CardSuite)
            {
                CardZone = currentZone;
            }
        }
    }

    public void SetText(string message)
    {
        text.text = message;
    }

    public void MoveCard(Vector3 position)
    {
        if (selectedCard != null)
        {
            selectedCard.transform.position = position;
            CheckNextCard();
            CheckPreviousCard();
        }
    }

    private GameObject GetFakeCard()
    {
        if (fakeCard != null)
        {
            if (fakeCard.transform.parent != handHolder.transform)
            {
                fakeCard.transform.SetParent(handHolder.transform, false);
            }

        }
        else
        {
            fakeCard = Instantiate(fakeCardPrefab);
            fakeCard.name = "Fake";
            fakeCard.transform.SetParent(handHolder.transform, false);
        }

        return fakeCard;
    }


    private void CheckNextCard()
    {
        if (nextCard != null)
        {
            if (selectedCard.transform.position.x > nextCard.transform.position.x)
            {
                int index = nextCard.transform.GetSiblingIndex();
                nextCard.transform.SetSiblingIndex(fakeCard.transform.GetSiblingIndex());
                fakeCard.transform.SetSiblingIndex(index);

                previousCard = nextCard;

                if (index + 1 < cardCountInHand)
                {
                    nextCard = handHolder.transform.GetChild(index + 1).GetComponent<OfflineCard>();
                }
                else
                {
                    nextCard = null;
                }
            }
        }

    }

    public void FillPlayerGiveHolder(int number)
    {
        for (int i = 0; i < number; i++)
        {
            OfflineCard selectedCard;
            Random.InitState(DateTime.Now.Second);
            int idOfCard = Random.Range(0, handHolder.GetComponentsInChildren<OfflineCard>().Length - 1);
            selectedCard = handHolder.GetComponentsInChildren<OfflineCard>()[idOfCard];
            if (selectedCard.name == "FakeCard")
            {
                i = i - 1;
            }
            else {
                selectedCard.transform.SetParent(giveHolder.transform, false);
            }
        }
    }

    private void CheckPreviousCard()
    {
        if (previousCard != null)
        {
            if (selectedCard.transform.position.x < previousCard.transform.position.x)
            {
                int index = previousCard.transform.GetSiblingIndex();
                previousCard.transform.SetSiblingIndex(fakeCard.transform.GetSiblingIndex());
                fakeCard.transform.SetSiblingIndex(index);

                nextCard = previousCard;

                if (index - 1 >= 0)
                {
                    previousCard = handHolder.transform.GetChild(index - 1).GetComponent<OfflineCard>();
                }
                else
                {
                    previousCard = null;
                }
            }
        }

    }
}
