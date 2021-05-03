using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonSelectReset : MonoBehaviour
{
    public void UnselectButton() {
        EventSystem.current.SetSelectedGameObject(null);
    }
}
