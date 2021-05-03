using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

internal class WriteMpGameResults
{
    public void WritePlayersScore(List<Player> players, bool win, int round)
    {
        string folder = Settings.folderPath;
        string textFolder = Settings.folderPath + @"\GameScore.txt";
        try
        {
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

        }
        catch { }

        string tmpStr;
        tmpStr = "newTable;";
        tmpStr = Encryption.EncryptDecrypt(tmpStr);
        File.AppendAllText(textFolder, tmpStr + Environment.NewLine);
        for (int i = 0; i < 4; i++)
        {
            string str;
            if (i < players.Count)
            {
                str = "name;" + players[i].name + ";score;" + players[i].TotalScore.ToString() + ";playerID;" + players[i].PlayerId + ";";

            }
            else
            {
                str = "name;Empty;score; ;playerID;" + i + ";";

            }
            str = Encryption.EncryptDecrypt(str);
            File.AppendAllText(textFolder, str + Environment.NewLine);
        }
        tmpStr = "round;" + round + ";";
        tmpStr = Encryption.EncryptDecrypt(tmpStr);
        File.AppendAllText(textFolder, tmpStr + Environment.NewLine);

        tmpStr = "result;" + win + ";";
        tmpStr = Encryption.EncryptDecrypt(tmpStr);
        File.AppendAllText(textFolder, tmpStr + Environment.NewLine);

        //File.AppendAllText(textFolder, Environment.NewLine);
    }

    public void WritePlayerStats(Player player)
    {
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
        if (!File.Exists(textFolder))
        {
            CreateFile(textFolder);
        }

        List<string> newLines = new List<string>();
        int file_line = 0;
        
        StreamReader sr_temp = new StreamReader(textFolder);
        string line;

        while ((line = sr_temp.ReadLine()) != null)
        {
            line = Encryption.EncryptDecrypt(line);
            string[] lineComp = line.Split(";"[0]);
            int number;
            switch (lineComp[0])
            {
                case "wins":
                    number = Int32.Parse(lineComp[1]);
                    if (player.win)
                    {
                        number++;
                    }
                    newLines.Add("wins;" + number);
                    break;
                case "loses":
                    number = Int32.Parse(lineComp[1]);
                    if (!player.win)
                    {
                        number++;
                    }
                    newLines.Add("loses;" + number);
                    break;
                case "allCards":
                    number = Int32.Parse(lineComp[1]);
                    number += player.allCards;
                    newLines.Add("allCards;" + number);
                    break;
                case "allPoints":
                    number = Int32.Parse(lineComp[1]);
                    number += player.allPoints;
                    newLines.Add("allPoints;" + number);
                    break;
                case "playedGames":
                    number = Int32.Parse(lineComp[1]);
                    number++;
                    newLines.Add("playedGames;" + number);
                    break;
                case "lessThan50":
                    number = Int32.Parse(lineComp[1]);
                    if (player.TotalScore < 50)
                    {
                        number++;
                    }
                    newLines.Add("lessThan50;" + number);
                    break;
                case "lessThan25":
                    number = Int32.Parse(lineComp[1]);
                    if (player.TotalScore < 25)
                    {
                        number++;
                    }
                    newLines.Add("lessThan25;" + number);
                    break;
                default:
                    break;
            }
            file_line++;
        }

        sr_temp.Close();

        StreamWriter sw = new StreamWriter(textFolder);
        string tmpStr;
        for (int i = 0; i < file_line; i++)
        {
            tmpStr = Encryption.EncryptDecrypt(newLines[i]);
            sw.WriteLine(tmpStr);
        }

        sw.Close();
    }

    private void CreateFile(string textFolder)
    {
        string tmpStr = "wins;0";
        tmpStr = Encryption.EncryptDecrypt(tmpStr);
        File.AppendAllText(textFolder, tmpStr + Environment.NewLine);
        tmpStr = "loses;0";
        tmpStr = Encryption.EncryptDecrypt(tmpStr);
        File.AppendAllText(textFolder, tmpStr + Environment.NewLine);
        tmpStr = "allCards;0";
        tmpStr = Encryption.EncryptDecrypt(tmpStr);
        File.AppendAllText(textFolder, tmpStr + Environment.NewLine);
        tmpStr = "allPoints;0";
        tmpStr = Encryption.EncryptDecrypt(tmpStr);
        File.AppendAllText(textFolder, tmpStr + Environment.NewLine);
        tmpStr = "playedGames;0";
        tmpStr = Encryption.EncryptDecrypt(tmpStr);
        File.AppendAllText(textFolder, tmpStr + Environment.NewLine);
        tmpStr = "lessThan50;0";
        tmpStr = Encryption.EncryptDecrypt(tmpStr);
        File.AppendAllText(textFolder, tmpStr + Environment.NewLine);
        tmpStr = "lessThan25;0";
        tmpStr = Encryption.EncryptDecrypt(tmpStr);
        File.AppendAllText(textFolder, tmpStr + Environment.NewLine);
        
    }

}