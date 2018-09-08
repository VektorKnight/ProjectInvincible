using _3rdParty.Steamworks.Plugins.Steamworks.NET.types.SteamClientPublic;
using SteamNet;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIFriendPanel : UIBehavior {

    //Object references
    [SerializeField] public Text LobbyName, LobbyState;
    [SerializeField] public RawImage PlayerIcon;
    [SerializeField] public CSteamID LobbyID;

    public LobbyData lobbyData;

    public void SetData(LobbyData data, CSteamID id) {
        lobbyData = data;
        LobbyName.text = data.Name;
        LobbyID = id;
    }
    
    //Button calls
    public void JoinLobby() {
        SteamNetManager.Instance.JoinLobby(LobbyID);
    }
    
}
