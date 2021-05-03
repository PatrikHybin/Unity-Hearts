using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class OfflineInGameMenu : MonoBehaviour
{
    // Start is called before the first frame update
    public void PlayAgain() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // Update is called once per frame
    public void ReturnToMenu ()
    {
        ScoreBoard score = GetComponentInChildren<ScoreBoard>();
        score.transform.parent = null;
        DontDestroyOnLoad(score);
        SceneManager.LoadScene("Scene_Menu");
    }

    public void QuitGame()
    {

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
        Application.Quit();

    }
}
