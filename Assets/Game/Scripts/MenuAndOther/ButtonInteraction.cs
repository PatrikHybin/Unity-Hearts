using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonInteraction : MonoBehaviour, IPointerEnterHandler, IPointerDownHandler
{
    public void OnPointerDown(PointerEventData eventData)
    {
        try
        {
            if (gameObject.name == "Button_Leave") {
                GameObject.Find("SoundManager(Clone)").GetComponent<SoundManager>().soundManager.PlaySound("leaveLobby");
                return;
            } 
            GameObject.Find("SoundManager(Clone)").GetComponent<SoundManager>().soundManager.PlaySound("btnClick");
        }
        catch { }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        //try
        //{
        //    GameObject.Find("SoundManager").GetComponent<SoundManager>().soundManager.PlaySound("btnHover");
        //}
        //catch { }
    }
}
