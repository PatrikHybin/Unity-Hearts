using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;

public class OfflineTimer : MonoBehaviour
{
    public static OfflineTimer timer;
    private float currentTime = 0f;
    private float giveTime = 5f;
    private float playTime = 15f;
    private string msg = "start";
    private bool giveCards;
    private bool playCard;
    private bool botsGiveCard;
    private Color giveCardColor;

    private void Awake()
    {
        if (timer == null) {
            timer = this;
        }
    }
    void Start()
    {
        giveCardColor.a = 1;
        giveCardColor.r = 0.5f;
        giveCardColor.g = 1;
        giveCardColor.b = 0.3f;
    }

    void Update()
    {
        //FindObjectOfType<OfflinePlayer>().giveHolder.GetComponent<Image>().color = Color(255,0, 0);
        if (msg == "GiveCards" && giveCards && !FindObjectOfType<OfflinePlayer>().endOfGame)
        {
            
            currentTime -= 1 * Time.deltaTime;
            FindObjectOfType<OfflinePlayer>().SetText(currentTime.ToString("0"));
            giveCardColor.a = 1;
            FindObjectOfType<OfflinePlayer>().giveHolder.GetComponent<Image>().color = giveCardColor;
            if (currentTime.ToString("0") == "3" && botsGiveCard)
            {
                botsGiveCard = false;
                OfflineGameManagerHearts.instanceOfflineManager.PlaceRandomCardsForBotsIntoGiveHolder();

            }
            if (currentTime.ToString("0") == "0")
            {
                giveCards = false;
                OfflineGameManagerHearts.instanceOfflineManager.SendGiveHolderCards();
            }
        }
        else {
            giveCardColor.a = 0;
            FindObjectOfType<OfflinePlayer>().giveHolder.GetComponent<Image>().color = giveCardColor;
        }

        if (msg == "PlayerTime" && playCard)
        {
            currentTime -= 1 * Time.deltaTime;
            FindObjectOfType<OfflinePlayer>().SetText(currentTime.ToString("0"));
            // || !FindObjectOfType<OfflinePlayer>().myTurn
            if (currentTime.ToString("0") == "0") {
                playCard = false;
                //FindObjectOfType<OfflinePlayer>().SetText(" ");
                FindObjectOfType<OfflinePlayer>().PlayRandomCard();
            }
            if (!FindObjectOfType<OfflinePlayer>().myTurn) {
                playCard = false;
                FindObjectOfType<OfflinePlayer>().SetText(" ");
            }
        }

    }

    public void StartTime(string message)
    {
        msg = message;
        if (msg == "GiveCards") {
            currentTime = giveTime;
            giveCards = true;
            botsGiveCard = true;
        }

        if (msg == "PlayerTime") {
            currentTime = playTime;
            playCard = true;
        }
    }
}
