using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using SteamNet;

//Steam
using _3rdParty.Steamworks.Plugins.Steamworks.NET.types.SteamClientPublic;
using _3rdParty.Steamworks.Plugins.Steamworks.NET.autogen;
using _3rdParty.Steamworks.Plugins.Steamworks.NET;

public class UILobbySettings : MonoBehaviour {

    public Text GameButtonText, TimerText;
    public Button GameButton;
    public Image GameButtonImage, TimerImage;
    public RectTransform TimerOverlay;

    public Color32 NotReady, Ready;

    public int timerDisplay = 5;

    public void Update() {

        //Keep button up to date
        if (SteamManager.Instance.Hosting) {
            GameButtonText.text = "Start Game".ToUpper();

            GameButton.interactable = (SteamManager.Instance.CurrentlyJoinedLobby.ArePlayersReady() ? true : false);
            GameButtonImage.color = (SteamManager.Instance.CurrentlyJoinedLobby.ArePlayersReady() ? Ready : NotReady);
        }
        if (SteamManager.Instance.Connected) {
            GameButtonText.text = "Ready".ToUpper();

            GameButtonImage.color = (SteamManager.Instance.CurrentlyJoinedLobby.LobbyMembers[SteamUser.GetSteamID()].IsReady ? Ready : NotReady);
        }
        if(SteamManager.Instance.NetworkState== ENetworkState.Stopped) {
            GameButtonImage.color = NotReady;
        }

        //Timer visual
        if (SteamManager.Instance.CurrentlyJoinedLobby.TimerStarted) {
            TimerText.text = SteamManager.Instance.CurrentlyJoinedLobby.TimerDisplay.ToString();
            TimerOverlay.offsetMax = new Vector2((float)(-450 * SteamManager.Instance.CurrentlyJoinedLobby.TimerOverlayPercent), 0);
            TimerImage.color = new Color(TimerImage.color.r, TimerImage.color.g, TimerImage.color.b, 1 - (float)SteamManager.Instance.CurrentlyJoinedLobby.TimerOverlayPercent);
        }
        else {
            TimerText.text = "5";
            TimerOverlay.offsetMax = new Vector2(0, 0);
            TimerImage.color = new Color(TimerImage.color.r, TimerImage.color.g, TimerImage.color.b, 0);
        }
    }

    //Sends a message to the lobby data to start the game
    //if we are a client, this just redies up
    public void StartGame() {
        SteamManager.Instance.StartGame();
    }
}
