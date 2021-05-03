using Mirror;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NetworkRoomPlayerHearts : NetworkBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject lobby = null;
    [SerializeField] private TMP_Text[] playerNameTexts = new TMP_Text[4];
    [SerializeField] private TMP_Text[] playerReadyTexts = new TMP_Text[4];
    [SerializeField] private Image[] playerReadyImages = new Image[4];
    [SerializeField] private Button startGameButton = null;
    [SerializeField] private Button readyButton = null;
    private Sprite ready;
    private Sprite notReady;

    [SyncVar(hook = nameof(HandleDisplayNameChanged))]
    public string DisplayName = " ";
    [SyncVar(hook = nameof(HandleReadyStatusChanged))]
    public bool IsReady = false;

    private bool isHost;
    public bool IsHost {
        set {
            isHost = value;
            startGameButton.gameObject.SetActive(value);
        }
        get { return isHost; }
    }

    private NetworkManagerHearts room;
    private NetworkManagerHearts Room {
        get {
            if (room != null) {
                return room;
            }
            return room = NetworkManager.singleton as NetworkManagerHearts;
        }
    }

    private void Awake()
    {
        ready = Resources.Load<Sprite>("Sprites/ready");
        notReady = Resources.Load<Sprite>("Sprites/readyNot");
    }

    internal void HandleReadyToStart(bool readyToStart)
    {
        if (!isHost) {
            return;
        }

        startGameButton.interactable = readyToStart;
    }

    public override void OnStartAuthority()
    {
        CmdSetDisplayName(InputPlayerName.DisplayName);
        lobby.SetActive(true);
    }

    public override void OnStartClient()
    {
        Room.RoomPlayers.Add(this);
        UpdateDisplay();
    }

    public override void OnStopClient()
    {
        Room.RoomPlayers.Remove(this);
        UpdateDisplay();
    }

    
    public void HandleReadyStatusChanged(bool oldValue, bool newValue) => UpdateDisplay();
    public void HandleDisplayNameChanged(string oldValue, string newValue) => UpdateDisplay();

    public void UpdateDisplay()
    {
        if (!hasAuthority) {

            foreach (var player in Room.RoomPlayers) {
                if (player.hasAuthority) {

                    player.UpdateDisplay();
                    break;
                }
            }

            return;
        }

        for (int i = 0; i < playerNameTexts.Length; i++) {
            playerNameTexts[i].text = " ";
            playerReadyTexts[i].text = " ";
            Color normalColor;
            ColorUtility.TryParseHtmlString("#81ACCA", out normalColor);
            playerReadyImages[i].color = normalColor;
            playerReadyImages[i].sprite = null;
        }

        for (int i = 0; i < Room.RoomPlayers.Count; i++)
        {
            playerNameTexts[i].text = Room.RoomPlayers[i].DisplayName;
            //playerReadyTexts[i].text = Room.RoomPlayers[i].IsReady ? "<color=green>Ready</color>" : "<color=red>Not Ready</color>";
            playerReadyImages[i].sprite = Room.RoomPlayers[i].IsReady ?Room.RoomPlayers[i].ready : Room.RoomPlayers[i].notReady;
            playerReadyImages[i].color = Color.white;
        }

        Room.NotifyPlayersOfReadyState();
    }

    [Command]
    private void CmdSetDisplayName(string displayName) {
        DisplayName = displayName;
        gameObject.name = displayName;
    }

    public void Ready() {
        ColorBlock color = ColorBlock.defaultColorBlock;
        Color normalColor;
        Color highlightedColor;
        Color pressedColor;
        if (!IsReady)
        {
            ColorUtility.TryParseHtmlString("#2FB444", out normalColor);
            ColorUtility.TryParseHtmlString("#DB3943", out highlightedColor);
            ColorUtility.TryParseHtmlString("#B7242C", out pressedColor);
        }
        else
        {
            ColorUtility.TryParseHtmlString("#FFFFFF", out normalColor);
            ColorUtility.TryParseHtmlString("#2FB444", out highlightedColor);
            ColorUtility.TryParseHtmlString("#258235", out pressedColor);
        }
        color.normalColor = normalColor;
        color.highlightedColor = highlightedColor;
        color.pressedColor = pressedColor;
        foreach (Button button in gameObject.GetComponentsInChildren<Button>())
        {
            if (button.name == "Button_Ready")
            {
                button.colors = color;
            }
        }
        CmdReady();
    }
    [Command]
    public void CmdReady() {
        IsReady = !IsReady;
        Room.NotifyPlayersOfReadyState();
    }


    public void Leave()
    {
        if (hasAuthority) {
            
        }
        if (isHost)
        {
            Room.StopHost();
        }
        else
        {
            Room.StopClient();
        }
        Room.NotifyPlayersOfReadyState();
    }

    [Command]
    public void CmdStartGame() {
        if (Room.RoomPlayers[0].connectionToClient != connectionToClient) { 
            return;
        }
        Room.StartGame();
    }
}
