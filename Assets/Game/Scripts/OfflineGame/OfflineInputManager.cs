using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


public class OfflineInputManager : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    
    public void OnDrag(PointerEventData eventData)
    {
        //Screen space - camera
        Vector3 screenPoint = eventData.position;
        screenPoint.z = Settings.distanceCameraPlane.z;
        var vector3 = Camera.main.ScreenToWorldPoint(screenPoint);
        if (gameObject.GetComponentInParent<OfflinePlayer>()) {
            gameObject.GetComponentInParent<OfflinePlayer>().MoveCard(vector3);
        }
        //CardManager.cardManager.MoveCard(vector3);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log("Down " + gameObject.name);
        if (eventData.pointerCurrentRaycast.gameObject.GetComponent<OfflineCard>() != null)
        {
            GetComponent<CanvasGroup>().blocksRaycasts = false;
            if (gameObject.GetComponentInParent<OfflinePlayer>())
            {
                gameObject.GetComponentInParent<OfflinePlayer>().SetSelectedCard(eventData.pointerCurrentRaycast.gameObject.GetComponent<OfflineCard>());
            }
            
        } 
    }
    public void OnPointerUp(PointerEventData eventData)
    {
        
        GetComponent<CanvasGroup>().blocksRaycasts = true;
        if (gameObject.GetComponentInParent<OfflinePlayer>())
        {
            gameObject.GetComponentInParent<OfflinePlayer>().ReleaseCard();
        }
        //CardManager.cardManager.ReleaseCard();
    }
    

}
