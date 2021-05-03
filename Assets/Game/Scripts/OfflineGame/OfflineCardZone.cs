using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class OfflineCardZone : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (gameObject.GetComponentInParent<OfflinePlayer>())
        {
            GetComponentInParent<OfflinePlayer>().CardZone = gameObject;
        }   
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (gameObject.GetComponentInParent<OfflinePlayer>())
        {
            GetComponentInParent<OfflinePlayer>().CardZone = null;
        }
        
    }
}
