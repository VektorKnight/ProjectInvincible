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
    protected Callback<LobbyChatUpdate_t> m_LobbyChatUpdate;


    /// <summary>
    /// returns avatar of user
    /// </summary>
    /// <param name="user">Target user</param>
    /// <returns></returns>
    public Texture2D GetAvatar(ulong user) {
        int FriendAvatar = SteamFriends.GetMediumFriendAvatar((CSteamID)user);
        uint ImageWidth;
        uint ImageHeight;
        bool success = SteamUtils.GetImageSize(FriendAvatar, out ImageWidth, out ImageHeight);

        if (success && ImageWidth > 0 && ImageHeight > 0) {
            byte[] Image = new byte[ImageWidth * ImageHeight * 4];
            Texture2D returnTexture = new Texture2D((int)ImageWidth, (int)ImageHeight, TextureFormat.RGBA32, false, true);
            success = SteamUtils.GetImageRGBA(FriendAvatar, Image, (int)(ImageWidth * ImageHeight * 4));
            if (success) {
                returnTexture.LoadRawTextureData(Image);
                returnTexture.Apply();
            }
            return returnTexture;
        }
        else {
            Debug.Log("Couldn't get avatar.");
            return new Texture2D(0, 0);
        }
    }



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
    protected virtual void OnLobbyChatUpdate(LobbyChatUpdate_t param) {

    }

    // Update is called once per frame
    void Update () {
		
	}
}
