using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OfflineCard : MonoBehaviour
{
    private Image img;
    public int index;

    private Sprite face;
    public Sprite Face
    {
        get { return face; }
        set { face = value; }
    }

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

    private int ownerId;
    public int OwnerId
    {
        get { return ownerId; }
        set { ownerId = value; }
    }
    private void Awake()
    {
        img = GetComponent<Image>();
    }
    
    public void SetImage(Sprite sprite) {
        img.sprite = sprite;
        
    }

    public void setAspect(bool preserve) {
        img.preserveAspect = preserve;
    }
}
