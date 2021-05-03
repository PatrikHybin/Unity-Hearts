using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Mirror;
using System;

public class CardZone : NetworkBehaviour, IPointerEnterHandler, IPointerExitHandler
{

    private int ownerId;
   
    public void Start()
    {
        if (gameObject.name.Contains("UsedCardHolder")) {
            gameObject.GetComponent<CanvasGroup>().blocksRaycasts = false;
        }
        try
        {
            Player player = GetComponentInParent<Player>();
            ownerId = player.PlayerId;
        } catch {}
        
    }
    [Client]
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (hasAuthority)
        {
            Player.players[ownerId].CardZone = gameObject;
            //GetComponentInParent<Player>().CardZone = gameObject;
        }

    }
    [Client]
    public void OnPointerExit(PointerEventData eventData)
    {

        if (hasAuthority)
        {
            Player.players[ownerId].CardZone = null;
            //GetComponentInParent<Player>().CardZone = null;
        }

    }

}
