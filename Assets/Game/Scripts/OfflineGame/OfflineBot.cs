using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class OfflineBot : MonoBehaviour
{
    [SerializeField] public GameObject parentHolder, handHolder, giveHolder, usedCardHolder;
    private int interval = 1;
    private float nextTime = 0;
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

    private int playerId;
    public int PlayerId
    {
        get { return playerId; }
        set { playerId = value; }
    }

    public bool win { get; internal set; }

    public bool myTurn = false;
    private SoundManager soundManager;

    void Start()
    {
        try
        {
            soundManager = GameObject.Find("SoundManager(Clone)").GetComponent<SoundManager>();
        }
        catch { }

        Random.InitState(DateTime.Now.Second);
        int nameId = Random.Range(0, OfflineGameManagerHearts.instanceOfflineManager.BotNames.Count);
        gameObject.name = OfflineGameManagerHearts.instanceOfflineManager.BotNames[nameId];
        OfflineGameManagerHearts.instanceOfflineManager.BotNames.RemoveAt(nameId);

        List<GameObject> tmpPlayers = new List<GameObject>();
        tmpPlayers = OfflineGameManagerHearts.instanceOfflineManager.players;
        tmpPlayers.Add(this.gameObject);
        tmpPlayers = tmpPlayers.OrderBy(x => x.transform.GetSiblingIndex()).ToList();
        OfflineGameManagerHearts.instanceOfflineManager.players = tmpPlayers;

    }

    // Update is called once per frame
    void Update()
    {
        if (Time.time >= nextTime && !OfflineGameManagerHearts.instanceOfflineManager.endOfGame)
        {
            if (myTurn && handHolder.GetComponentsInChildren<OfflineCard>().Length != 0)
            {
                PlayRandomCard();
                myTurn = false;
            }
            nextTime += interval;
        }
    }

    public void PlaceRandomCardsIntoGiveHolder()
    {
        for (int i = 0; i < 2; i++) {
            OfflineCard selectedCard;
            Random.InitState(DateTime.Now.Second);
            int idOfCard = Random.Range(0, handHolder.GetComponentsInChildren<OfflineCard>().Length - 1);
            selectedCard = handHolder.GetComponentsInChildren<OfflineCard>()[idOfCard];
            selectedCard.transform.SetParent(giveHolder.transform, false);
        }   
    }

    private void PlayRandomCard()
    {
        bool haveToMatch = false;
        List<OfflineCard> tmpOfflineCards = new List<OfflineCard>();
        foreach (OfflineCard card in handHolder.GetComponentsInChildren<OfflineCard>()) {
            if (OfflineGameManagerHearts.instanceOfflineManager.FirstPlayedCard != null) {
                if (OfflineGameManagerHearts.instanceOfflineManager.FirstPlayedCard.CardSuite == card.CardSuite)
                {
                    tmpOfflineCards.Add(card);
                    haveToMatch = true;
                }
            }
        }
        if (!haveToMatch) {
            tmpOfflineCards.Clear();
            foreach (OfflineCard card in handHolder.GetComponentsInChildren<OfflineCard>()) {
                tmpOfflineCards.Add(card);
            }
        }
        try
        {
            soundManager.PlayCardSound("placeCard");
        }
        catch { }
        List<OfflineCard> lowerTmpOfflineCards = new List<OfflineCard>();
        OfflineCard selectedCard = null;

        Random.InitState(DateTime.Now.Second);
        if (OfflineGameManagerHearts.instanceOfflineManager.FirstPlayedCard == null)
        {
            int idOfCard = Random.Range(0, tmpOfflineCards.Count - 1);
            selectedCard = tmpOfflineCards[idOfCard];
        }
        else 
        {
            if (Random.Range(0, 100) < 80)
            {
                
                if (haveToMatch)
                {
                   
                    foreach (OfflineCard card in tmpOfflineCards)
                    {
                        if (OfflineGameManagerHearts.instanceOfflineManager.HighestPlayedCard.CardValue > card.CardValue)
                        {
                            lowerTmpOfflineCards.Add(card);
                        }
                    }
                    if (lowerTmpOfflineCards.Count == 0)
                    {
                        lowerTmpOfflineCards = tmpOfflineCards;
                    }

                    lowerTmpOfflineCards.OrderBy(item => item.CardValue).ToArray();

                    //int idOfCard = Random.Range(0, lowerTmpOfflineCards.Count - 1);
                    selectedCard = lowerTmpOfflineCards[0];
                }
                else
                {
                    foreach (OfflineCard card in tmpOfflineCards)
                    {
                        if (selectedCard == null)
                        {
                            selectedCard = card;
                        }
                        if (selectedCard.CardValue < card.CardValue)
                        {
                            selectedCard = card;
                        }
                    }
                }
            }
            else
            {
                Debug.Log("Nepodliezam");
                int idOfCard = Random.Range(0, tmpOfflineCards.Count - 1);
                selectedCard = tmpOfflineCards[idOfCard];

            }
        }

        //OfflineCard selectedCard;
        selectedCard.transform.SetParent(FindObjectOfType<OfflinePlayer>().playAreaHolder.transform, false);
        selectedCard.transform.localScale = new Vector2(1.2f, 1.7f);
        selectedCard.transform.rotation = Quaternion.Euler(70, 0, 0);
        OfflineGameManagerHearts.instanceOfflineManager.PlayCard(selectedCard, this.gameObject);
  
    }
    
}
