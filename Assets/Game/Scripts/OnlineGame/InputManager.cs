using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Mirror;
using System;

public class InputManager : NetworkBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{

    private Card card;
    
    [Client]
    public void OnDrag(PointerEventData eventData)
    {
        if (card != null) {
            
            if (card.hasAuthority)
            {
                Vector3 screenPoint = eventData.position;
                screenPoint.z = Settings.distanceCameraPlane.z;
                var vector3 = Camera.main.ScreenToWorldPoint(screenPoint);
                Player.players[card.OwnerId].MoveCard(vector3, card);
            }
        }

    }

    [Client]
    public void OnPointerDown(PointerEventData eventData)
    {
        card = eventData.pointerCurrentRaycast.gameObject.GetComponent<Card>();

        if (card != null)
        {
            if (card.hasAuthority)
            {
                GetComponentInChildren<CanvasGroup>().blocksRaycasts = false;
                //CardManager.cardManager.SetSelectedCard(eventData.pointerCurrentRaycast.gameObject.GetComponent<Card>());
                Player.players[card.OwnerId].SetSelectedCard(card);
                //GetComponentInParent<Player>().SetSelectedCard(card);
            }

        }

    }


    [Client]
    public void OnPointerUp(PointerEventData eventData)
    {
        if (card != null) {
            if (card.hasAuthority)
            {
                GetComponentInChildren<CanvasGroup>().blocksRaycasts = true;
                Player.players[card.OwnerId].ReleaseCard(card);
                card = null;
            }
        }

    }

}
