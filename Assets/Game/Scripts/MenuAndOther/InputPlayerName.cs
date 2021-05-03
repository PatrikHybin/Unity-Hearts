using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InputPlayerName : MonoBehaviour {

    [Header("UI")]
    [SerializeField] private TMP_InputField inputPlayerName = null;
    [SerializeField] private Button confirmPlayerNameButton = null;
    [SerializeField] private Button setDefaultNameButton = null;

    public static string DisplayName
    {
        get;
        private set;
    }

    private const string PlayerPrefsNameKey = "PlayerNameKey";

    private void Start() {
        SetUpInputField();
    }

    private void SetUpInputField()
    {
        if (!PlayerPrefs.HasKey(PlayerPrefsNameKey))
        {
            return;
        }

        string defaultPlayerName = PlayerPrefs.GetString(PlayerPrefsNameKey);
        inputPlayerName.text = defaultPlayerName;
        SetPlayerName(defaultPlayerName);
    }

    public void SetPlayerName(string playerName)
    {
        confirmPlayerNameButton.interactable = !string.IsNullOrEmpty(playerName);
    }

    public void SavePlayerNamePref()
    {
        DisplayName = inputPlayerName.text;

        PlayerPrefs.SetString(PlayerPrefsNameKey, DisplayName);
    }

    public void SetDefaultPlayerName() {
        
        DisplayName = PlayerPrefs.GetString(PlayerPrefsNameKey);
    }
  
}