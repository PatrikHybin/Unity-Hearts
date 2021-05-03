using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AudioSlider : MonoBehaviour
{
    [SerializeField] private Slider menuSlider;
    [SerializeField] private Slider gameSlider;
    [SerializeField] private TMP_Text menuText;
    [SerializeField] private TMP_Text gameText;
    private const string PlayerMenuVolumeKey = "PlayerMenuVolumeKey";
    private const string PlayerGameVolumeKey = "PlayerGameVolumeKey";

    void Start()
    {
        menuSlider.minValue = 0;
        menuSlider.maxValue = 1;
        menuSlider.wholeNumbers = false;
        
        gameSlider.minValue = 0;
        gameSlider.maxValue = 1;
        gameSlider.wholeNumbers = false;
        
        LoadPreferences();
    }

    private void LoadPreferences()
    {
        if (!PlayerPrefs.HasKey(PlayerMenuVolumeKey))
        {
            menuSlider.value = 0.2f;
            gameSlider.value = 0.2f;
            menuText.text = menuSlider.value.ToString("F1");
            gameText.text = gameSlider.value.ToString("F1");
            return;
        }

        menuSlider.value = float.Parse(PlayerPrefs.GetString(PlayerMenuVolumeKey));
        gameSlider.value = float.Parse(PlayerPrefs.GetString(PlayerGameVolumeKey));
        menuText.text = menuSlider.value.ToString("F1");
        gameText.text = gameSlider.value.ToString("F1");

        Settings.defualtEffectVolume = float.Parse(PlayerPrefs.GetString(PlayerMenuVolumeKey));
        Settings.menuVolume = float.Parse(PlayerPrefs.GetString(PlayerMenuVolumeKey));
        Settings.gameVolume = float.Parse(PlayerPrefs.GetString(PlayerGameVolumeKey));
    }

    public void OnValueMenuChanged(float value) {
        menuSlider.value = value;
        menuText.text = menuSlider.value.ToString("F1");
        PlayerPrefs.SetString(PlayerMenuVolumeKey, menuText.text);
        Settings.defualtEffectVolume = float.Parse(PlayerPrefs.GetString(PlayerMenuVolumeKey));
        Settings.menuVolume = float.Parse(PlayerPrefs.GetString(PlayerMenuVolumeKey));
    }

    public void OnValueGameChanged(float value)
    {
        gameSlider.value = value;
        gameText.text = gameSlider.value.ToString("F1");
        PlayerPrefs.SetString(PlayerGameVolumeKey, gameText.text);
        Settings.gameVolume = float.Parse(PlayerPrefs.GetString(PlayerGameVolumeKey));
    }

}
