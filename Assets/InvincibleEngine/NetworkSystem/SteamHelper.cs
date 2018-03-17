using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using InvincibleEngine.Networking;
using _3rdParty.Steamworks.Plugins.Steamworks.NET;
using _3rdParty.Steamworks.Plugins.Steamworks.NET.autogen;
using _3rdParty.Steamworks.Plugins.Steamworks.NET.types.SteamClientPublic;
using _3rdParty.Steamworks.Plugins.Steamworks.NET.types.SteamTypes;
using _3rdParty.Steamworks.Scripts.Steamworks.NET;

public class SteamHelper : MonoBehaviour {

    //Steam parameters
    public const int APP_ID = 805810;

    //Steam callbacks
    protected Callback<LobbyCreated_t> m_CreateLobby;
    protected Callback<LobbyMatchList_t> m_lobbyList;
    protected Callback<LobbyEnter_t> m_lobbyEnter;
    protected Callback<LobbyDataUpdate_t> m_lobbyInfo;
    protected Callback<GameLobbyJoinRequested_t> m_LobbyJoinRequest;
    protected Callback<LobbyChatMsg_t> m_LobbyChatMsg;
    

   

    public virtual void OnLobbyChatMsg(LobbyChatMsg_t param) {

    }

    public virtual void OnJoinLobbyRequest(GameLobbyJoinRequested_t param) {

    }

    public virtual void OnGetLobbyInfo(LobbyDataUpdate_t param) {

    }

    public virtual void OnLobbyEntered(LobbyEnter_t param) {

    }

    public virtual void OnGetLobbiesList(LobbyMatchList_t param) {

    }

    public virtual void OnCreateLobby(LobbyCreated_t param) {

    }

    // Update is called once per frame
    void Update () {
		
	}
}
