using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuitGame : MonoBehaviour
{
    public void ExitGame()
    {
        Debug.Log("Game is exiting");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif    
        Application.Quit();
  
    }
}
