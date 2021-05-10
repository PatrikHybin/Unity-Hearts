using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Settings
{
    public static int key = 1234;
    public static string folderPath = Application.dataPath;
    public static int losingScore = 1;
    public static float defualtEffectVolume = 0.1f;
    public static float cardEffectVolume = 0.1f;
    public static int losingScoreMp = 100;
    public static string cardSpritesPath = "Sprites/Cards";
    public static Vector3 distanceCameraPlane = new Vector3(0, 400, 650);
    public static float menuVolume;
    public static float gameVolume;
    public static float musicVolume;
    public static int numberOfPlayersToPlay = 4;
}
