using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] public static NetworkManagerHearts networkManager = null;
    [SerializeField] public GameObject networkManagerPrefab;
    [SerializeField] public GameObject soundManagerPrefab;
    

    [Header("UI")]
    [SerializeField] private GameObject panel_play = null;
    [SerializeField] private GameObject background = null;
    [SerializeField] private GameObject panel_menu = null;
    [SerializeField] private GameObject panel_scoreHolder = null;
    [SerializeField] private GameObject panel_results = null;
    [SerializeField] private GameObject button_retunFromScoreBoard = null;

    private bool gotScore = false;

    //private void Start()
    //{
    //    Encrypt();
    //}

    private void Awake()
    {
        if (networkManager == null)
        {
            networkManager = Instantiate(networkManagerPrefab.GetComponent<NetworkManagerHearts>());
        }
        CheckNetworkManager();

        if (!GameObject.Find("SoundManager(Clone)")) {
            Instantiate(soundManagerPrefab);
        }
    }

    private void Update()
    {
        if (FindObjectOfType<ScoreBoard>() && !gotScore && !panel_scoreHolder.activeSelf)
        {
            SetUpScoreBoard();
            gotScore = true;

        }
    }


    public void HostLobby() {
        CheckNetworkManager();
        networkManager.StartHost();
        panel_play.SetActive(false);
        background.SetActive(false);
        
    }

    public void PlayOffline()
    {
        CheckNetworkManager();
        panel_play.SetActive(false);
        background.SetActive(false);
        SceneManager.LoadScene("Scene_Offline");

    }

    public void ViewResults() {
        panel_scoreHolder.SetActive(true);
        background.SetActive(false);
        button_retunFromScoreBoard.SetActive(true);
        ReadGameResults read = GetComponent<ReadGameResults>();
        read.GetPlayerGameHistory();
        read.GetPlayerStats();
    }

    

    private void SetUpScoreBoard()
    {
        GameObject scoreBoard = FindObjectOfType<ScoreBoard>().gameObject;
        GameObject canvasMenu = GameObject.Find("Canvas_Menu");
        scoreBoard.transform.SetParent(canvasMenu.transform, false);
        scoreBoard.GetComponent<RectTransform>().anchorMin = new Vector2(0.3f, 0.3f);
        scoreBoard.GetComponent<RectTransform>().anchorMax = new Vector2(0.7f, 0.7f);
        scoreBoard.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
        scoreBoard.GetComponent<RectTransform>().offsetMin = new Vector2(0, 0);
        scoreBoard.GetComponent<RectTransform>().offsetMax = new Vector2(0, 0);
        scoreBoard.transform.rotation = canvasMenu.transform.rotation;
        panel_menu.SetActive(false);
        background.SetActive(false);
        button_retunFromScoreBoard.SetActive(true);
    }

    public void HideScoreBoard() {
        if (FindObjectOfType<ScoreBoard>()) {
            GameObject scoreBoard = FindObjectOfType<ScoreBoard>().gameObject;
            Destroy(GameObject.Find("Panel_ScoreBoard"));
            Destroy(GameObject.Find("Panel_ScoreBoard"));
            scoreBoard.SetActive(false);
            
        }
        if (panel_scoreHolder.activeSelf) {
            foreach (Transform transform in panel_results.transform)
            {
                GameObject.Destroy(transform.gameObject);
            }
            panel_scoreHolder.SetActive(false);
        }
    }

    private void CheckNetworkManager()
    {
        if (networkManager == null)
        {
            networkManager = Instantiate(networkManagerPrefab.GetComponent<NetworkManagerHearts>());
        }
    }

    //private void Encrypt()
    //{
    //    string textFolder = Settings.folderPath + @"\GameScore.txt";

    //    string[] lines = File.ReadAllLines(textFolder);
    //    string tmpStr;
    //    List<string> newLines = new List<string>();
    //    foreach (string line in lines)
    //    {
    //        tmpStr = Encryption.EncryptDecrypt(line);
    //        newLines.Add(tmpStr);
    //    }

    //    foreach (string line in newLines)
    //    {
    //        File.AppendAllText(textFolder, line + Environment.NewLine);
    //    }
    //    Debug.Log("Hotovo");
    //}
}
