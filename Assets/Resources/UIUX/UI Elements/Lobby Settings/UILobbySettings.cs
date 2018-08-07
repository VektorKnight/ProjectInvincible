using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using SteamNet;
using InvincibleEngine.Managers;

//Steam
using _3rdParty.Steamworks.Plugins.Steamworks.NET.types.SteamClientPublic;
using _3rdParty.Steamworks.Plugins.Steamworks.NET.autogen;
using _3rdParty.Steamworks.Plugins.Steamworks.NET;

public class UILobbySettings : MonoBehaviour {

    //Timer and buttons
    public Text GameButtonText;
    public Button GameButton;
    public Image GameButtonImage, TimerImage;
    public RectTransform TimerOverlay;
    public Dropdown MapPicker;
  
    //Colors
    public Color32 NotReady, Ready;

    private void Start() {

        //On start load all the maps to select from
        List<Dropdown.OptionData> mapData = new List<Dropdown.OptionData>();

        foreach (MapData n in AssetManager.LoadedMaps) {
            mapData.Add(new Dropdown.OptionData(n.MapName, n.Splash));
        }

        MapPicker.AddOptions(mapData);

    }

    public void Update() {

        //--------------------------------------------
        #region Change Button style
        //--------------------------------------------

        try {
           
            if (SteamNetManager.Instance.Hosting) {
                GameButtonText.text = "Start Game".ToUpper();

                GameButton.interactable = (SteamNetManager.Instance.CurrentlyJoinedLobby.ArePlayersReady() ? true : false);
                GameButtonImage.color = (SteamNetManager.Instance.CurrentlyJoinedLobby.ArePlayersReady() ? Ready : NotReady);
            }
            if (SteamNetManager.Instance.Connected) {
                GameButtonText.text = "Ready".ToUpper();

                GameButtonImage.color = (SteamNetManager.Instance.CurrentlyJoinedLobby.LobbyMembers[SteamUser.GetSteamID()].IsReady ? Ready : NotReady);
            }
            if (SteamNetManager.Instance.NetworkState == ENetworkState.Stopped) {
                GameButtonImage.color = NotReady;
            }
        }
        catch {
            Debug.Log("UI Settings error");
        }

        #endregion

        //--------------------------------------------
        #region Disable/enable map picker and change it based on lobby
        //--------------------------------------------

        //If hosting, enable lobby picker
        if (SteamNetManager.Instance.Hosting) {
            MapPicker.interactable = true;
        }
        
        //If client, disable the picker and set it's value directly from lobby data
        if(SteamNetManager.Instance.Connected) {
            MapPicker.interactable = false;
            MapPicker.value = SteamNetManager.CurrentLobbyData.MapIndex;
        }

        #endregion
    }

    /// <summary>
    /// Called when the host picks a map
    /// </summary>
    public void OnMapSelect() {

        //Only able to do this if a host and owner of lobby
        if (SteamNetManager.Instance.Hosting) {
            SteamNetManager.CurrentLobbyData.MapIndex = MapPicker.value;
        }
    }

    //Sends a message to the lobby data to start the game
    //if we are a client, this just redies up
    public void StartGame() {
        SteamNetManager.Instance.StartGame();
    }
}
