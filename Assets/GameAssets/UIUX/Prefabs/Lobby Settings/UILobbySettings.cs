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

    public Text GameButtonText;
    public Button GameButton;
    public Image GameButtonImage;

    public Color32 NotReady, Ready;

    public void Update() {
        if(SteamManager.Instance.Hosting) {
            GameButtonText.text = "Start Game".ToUpper();

            GameButton.interactable = (SteamManager.Instance.CurrentlyJoinedLobby.ArePlayersReady() ? true : false);          
        }
        if(SteamManager.Instance.Connected) {
            GameButtonText.text = "Ready".ToUpper();

            GameButtonImage.color = (SteamManager.Instance.CurrentlyJoinedLobby.LobbyMembers[SteamUser.GetSteamID()].IsReady ? Ready : NotReady);
        }
    }

    //Sends a message to the lobby data to start the game
    //if we are a client, this just redies up
    public void StartGame() {

        //If connected, signal ready flag
        if(SteamManager.Instance.Connected) {
            SteamManager.Instance.BroadcastChatMessage(new N_RDY());
        }

        //If hosting, sigal game start on local manager
        if(SteamManager.Instance.Hosting) {

        }
    }
}
