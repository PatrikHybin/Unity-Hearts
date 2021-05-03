using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;

public class ReadGameResults : MonoBehaviour
{
    [SerializeField] private GameObject scoreBoardPrefab, playerScorePrefab, roundPrefab, panel_results, panel_scoreHolder;
    public void GetPlayerGameHistory()
    {
        Debug.Log("");
        string folder = Settings.folderPath;
        string textFolder = Settings.folderPath + @"\GameScore.txt";
        Debug.Log(textFolder);
        try
        {
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
                
            }

        }
        catch { }
        if (!File.Exists(textFolder)) { return; }

        //File.Decrypt(textFolder);
        string[] lines = File.ReadAllLines(textFolder);

        string tmpStr;

        GameObject scoreBoard = null;
        foreach (string line in lines)
        {

            tmpStr = Encryption.EncryptDecrypt(line);
            string[] lineComp = tmpStr.Split(";"[0]);
            if (lineComp[0] == "newTable")
            {
                GameObject game = Instantiate(scoreBoardPrefab, panel_results.transform);
                scoreBoard = game;
            }
            if (lineComp[0] == "name") {
                
                //tableForFour = true;
                foreach (Transform gameObject in scoreBoard.GetComponentsInChildren<Transform>())
                {
                    if (gameObject.gameObject.name == "Panel_PlayerName" + lineComp[5])
                    {
                        gameObject.gameObject.GetComponentInChildren<TextMeshProUGUI>().text = lineComp[1];
                    }
                    if (gameObject.gameObject.name == "Panel_Player" + lineComp[5])
                    {
                        GameObject score = Instantiate(playerScorePrefab);
                        score.transform.SetParent(gameObject, false);
                        foreach (Transform transform in score.GetComponentsInChildren<Transform>())
                        {
                            if (transform.name == "PlayerScore")
                            {
                                transform.gameObject.GetComponent<TextMeshProUGUI>().text = lineComp[3];
                            }
                        }
                    }
                    
                }
            }
            if (lineComp[0] == "round")
            {
                foreach (Transform gameObject in scoreBoard.GetComponentsInChildren<Transform>())
                {

                    if (gameObject.gameObject.name == "Panel_RoundNumber")
                    {
                        GameObject round = Instantiate(roundPrefab);
                        round.transform.SetParent(gameObject, false);
                        foreach (Transform transform in round.GetComponentsInChildren<Transform>())
                        {
                            if (transform.name == "Round")
                            {
                                transform.gameObject.GetComponent<TextMeshProUGUI>().text = lineComp[1];
                            }
                        }
                    }
                }
            }
            if (lineComp[0] == "result")
            {

                //tableForFour = true;..
                foreach (Transform gameObject in scoreBoard.GetComponentsInChildren<Transform>())
                {

                    if (gameObject.gameObject.name == "ScoreBoard_Title")
                    {
                        
                        if (lineComp[1] == "Win")
                        {
                            gameObject.GetComponent<TextMeshProUGUI>().text = "Win";
                            gameObject.GetComponent<TextMeshProUGUI>().color = Color.green;
                        }
                        else {
                            gameObject.GetComponent<TextMeshProUGUI>().text = "Loss";
                            gameObject.GetComponent<TextMeshProUGUI>().color = Color.red;
                        }
                    }
                }
            }
        }

        //GameObject scoreBoard = player1.GetComponentInChildren<ScoreBoard>().gameObject;

        //foreach (Transform gameObject in scoreBoard.GetComponentsInChildren<Transform>())
        //{
        //    if (gameObject.gameObject.name == "Panel_RoundNumber")
        //    {
        //        GameObject number = Instantiate(roundPrefab);
        //        number.transform.SetParent(gameObject, false);
        //        foreach (Transform transform in number.GetComponentsInChildren<Transform>())
        //        {
        //            if (transform.name == "Round")
        //            {
        //                transform.gameObject.GetComponent<TextMeshProUGUI>().text = GameManagerHearts.gameManager.currentRound.ToString();
        //            }
        //        }
        //    }
        //    foreach (Player player in players)
        //    {
        //        if (gameObject.gameObject.name == "Panel_Player" + player.PlayerId)
        //        {
        //            GameObject score = Instantiate(playerScorePrefab);
        //            score.transform.SetParent(gameObject, false);
        //            foreach (Transform transform in score.GetComponentsInChildren<Transform>())
        //            {
        //                if (transform.name == "PlayerScore")
        //                {
        //                    transform.gameObject.GetComponent<TextMeshProUGUI>().text = player.TotalScore.ToString();
        //                }
        //            }
        //        }
        //    }
        //}
       
    }

    public void GetPlayerStats() {
        //string folder = Application.dataPath;
        //string textFolder = Application.dataPath + @"\PlayerStats.txt";
        string folder = Settings.folderPath;
        string textFolder = Settings.folderPath + @"\PlayerStats.txt";
        try
        {
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);

            }

        }
        catch { }

        if (!File.Exists(textFolder)) {
            return; 
        }

        string[] lines = File.ReadAllLines(textFolder);
        string tmpStr;
        foreach (string line in lines) {
            tmpStr = Encryption.EncryptDecrypt(line);
            string[] lineComp = tmpStr.Split(";"[0]);
            switch (lineComp[0])
            {
                case "wins":
                    GameObject.Find("Text_GamesWonValue").GetComponent<TextMeshProUGUI>().text = lineComp[1];
                    //SetValue(panel_scoreHolder, "Text_GamesWonValue", lineComp[1]);
                    break;
                case "loses":
                    GameObject.Find("Text_GamesLostValue").GetComponent<TextMeshProUGUI>().text = lineComp[1];
                    break;
                case "allCards":
                    GameObject.Find("Text_AllCardsValue").GetComponent<TextMeshProUGUI>().text = lineComp[1];
                    break;
                case "allPoints":
                    GameObject.Find("Text_AllPointsValue").GetComponent<TextMeshProUGUI>().text = lineComp[1];
                    break;
                case "playedGames":
                    GameObject.Find("Text_TotalGamesPlayedValue").GetComponent<TextMeshProUGUI>().text = lineComp[1];
                    break;
                case "lessThan50":
                    GameObject.Find("Text_LessThan50Value").GetComponent<TextMeshProUGUI>().text = lineComp[1];
                    break;
                case "lessThan25":
                    GameObject.Find("Text_LessThan25Value").GetComponent<TextMeshProUGUI>().text = lineComp[1];
                    break;
                default:
                    break;
            }
        }
        
    }

    
    //private void SetValue(GameObject obj, string name, string value)
    //{
    //    Debug.Log(value);
    //    //Transform trans = obj.transform;
    //    //Transform childTrans = trans.Find(name);
    //    GameObject.Find(name).GetComponent<TextMeshProUGUI>().text = value;
    //}
}
