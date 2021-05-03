using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class Card : NetworkBehaviour
{
    private Image img;
    public int index;

    private EnumCardValue.CardValue cardValue;
    public EnumCardValue.CardValue CardValue
    {
        get { return cardValue; }
        set { cardValue = value; }
    }

    private string cardSuite;
    public string CardSuite
    {
        get { return cardSuite; }
        set { cardSuite = value; }
    }
    
    [SerializeField]
    private int ownerId;
    public int OwnerId {
        get { return ownerId; }
        set { ownerId = value; }
    }

    private Sprite face;
    public Sprite Face
    {
        get { return face; }
        set { face = value; }
    }

    private Sprite back;
    public  Sprite Back
    {
        get { return back; }
        set { back = value; }
    }

    private Card nextCard = null;
    public Card NextCard {
        get { return nextCard; }
        set { nextCard = value; }
    }

    private Card previousCard = null;
    public Card PreviousCard
    {
        get { return previousCard; }
        set { previousCard = value; }
    }

    private CardZone currentZone;
    public CardZone CurrentZone
    {
        get { return currentZone; }
        set { currentZone = value; }
    }
    private void Awake()
    {
        img = GetComponent<Image>();
    }

    private void Start()
    {
        try
        {
            Player player = GetComponentInParent<Player>();
            //Debug.Log(player.PlayerId);
            OwnerId = player.PlayerId;
        }
        catch {}
    }

    public void SetImage(Sprite sprite) {
        img.sprite = sprite;
        
    }

    public void SetAspect(bool preserve) {
        img.preserveAspect = preserve;
    }

    public void SetCurrentZone(CardZone cardZone) {
        CmdSetCurrentZone(cardZone);
    }

    [Command]
    private void CmdSetCurrentZone(CardZone cardZone) {
        RpcSetCurrentZone(cardZone);
    }

    [ClientRpc]
    private void RpcSetCurrentZone(CardZone cardZone) {
        currentZone = cardZone;
    }

}
