﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using InvincibleEngine.Networking;
using Newtonsoft.Json;
using _3rdParty.Steamworks.Plugins.Steamworks.NET;
using _3rdParty.Steamworks.Plugins.Steamworks.NET.autogen;
using _3rdParty.Steamworks.Plugins.Steamworks.NET.types.SteamClientPublic;
using _3rdParty.Steamworks.Plugins.Steamworks.NET.types.SteamTypes;
using _3rdParty.Steamworks.Scripts.Steamworks.NET;

/// <summary>
/// Manager for all network traffic to/from this client
/// </summary>
namespace InvincibleEngine.Managers {
    /// <summary>
    /// Defining class for messages that can be sent over networks
    /// Those prefixed with "L" are lobby only
    /// Those prefixed with "G' are game only
    /// </summary>   
    public class NetMessage {
        //required interface
        public interface INetMessage { }

        //Generic chat dialog message
        public class L_CHT : INetMessage {
            public string message;

            public L_CHT(string message) {
                this.message = message;
            }
        }
        
        public class G_ENT :INetMessage {
            public ushort EntityID;
            public List<object> SyncFields;
        }
    }

    /// <summary>
    /// Custom attributes for field syncing
    /// </summary>
    public class SyncField : Attribute { }
    public class SyncEvent : Attribute { }

    public class NetManager : SteamHelper {

        //Singleton Implementation
        private static NetManager _Singleton = null;
        public static NetManager Singleton { get { if (_Singleton == null) { _Singleton = GameObject.Find("Managers").GetComponent<NetManager>(); } return _Singleton; } private set { } }

        //TODO: Refactor to account for lobbies and games
        //Server State
        [Header("State of network")]
        public NetworkState networkState = NetworkState.Stopped;
        public enum NetworkState {
            Hosting, Connected, Stopped
        }

        //information about the lobby member
        [Serializable]
        public struct LobbyMember {
            public LobbyMember(int team, ulong steamID) {
                Team = team;
                SteamID = steamID;
            }

            public int Team;
            public ulong SteamID;

            public string Name { get { return SteamFriends.GetFriendPersonaName((CSteamID)SteamID); } }           
        }
        
        //Stores chat log of active lobby
        public static class ChatLog {
            public static string chat;
            public static void PostChat(string input) {
                chat += $"\n{input}";
            }         
        }
        
        //ID of game lobby
        [Header("Lobby Data")]
        public CSteamID CurrentLobbyID;
        public List<CSteamID> lobbyIDS;
        public List<LobbyMember> LobbyMembers = new List<LobbyMember>();
        public LobbyMember LocalPlayer;            

        //Update parameters
        public int LobbyUpdatesPerSecond = 1;

        public  void Start() {

            //Restart if overlay is not injected
            SteamAPI.RestartAppIfNecessary((AppId_t)APP_ID);

            //Start Steam API
            if (SteamAPI.Init())
                Debug.Log("Steam API init -- SUCCESS!");
            else
                Debug.Log("Steam API init -- failure ...");

            //Ensure steamworks is properly started and assign callbacks
            if (SteamManager.Initialized) {
                //Assign call returns and callbacks
                m_CreateLobby = Callback<LobbyCreated_t>.Create(OnCreateLobby);
                m_lobbyList = Callback<LobbyMatchList_t>.Create(OnGetLobbiesList);
                m_lobbyEnter = Callback<LobbyEnter_t>.Create(OnLobbyEntered);
                m_lobbyInfo = Callback<LobbyDataUpdate_t>.Create(OnGetLobbyInfo);
                m_LobbyJoinRequest = Callback<GameLobbyJoinRequested_t>.Create(OnJoinLobbyRequest);
                m_LobbyChatMsg = Callback<LobbyChatMsg_t>.Create(OnLobbyChatMsg);


                //Start Coroutines
                StartCoroutine("SteamCall");
                StartCoroutine("SteamLobbyUpdate");
            }
        }


        /// <summary>
        /// Runs Steam API Returns and Callbacks
        /// </summary>
        /// <returns></returns>
        IEnumerator SteamCall() {
            while (true) {
                SteamAPI.RunCallbacks();
                yield return new WaitForSeconds(0.01f);
            }
        }

        /// <summary>
        /// Automatic Updates to lobby
        /// </summary>
        /// <returns></returns>
        IEnumerator SteamLobbyUpdate() {
            while (true) {
                //request the list of lobbies on the server
                SteamMatchmaking.RequestLobbyList();
                
                //if we are a host, always update the lobby metadata
                if (networkState==NetworkState.Hosting) {
                    LobbyMembers.Clear();
                    int numPlayers = SteamMatchmaking.GetNumLobbyMembers(CurrentLobbyID);

                    Debug.Log("\t Number of players currently in lobby : " + numPlayers);
                    for (int i = 0; i < numPlayers; i++) {
                        LobbyMember n = new LobbyMember();
                        n.SteamID = SteamMatchmaking.GetLobbyMemberByIndex(CurrentLobbyID, i).m_SteamID;
                        LobbyMembers.Add(n);
                    }

                    //set lobby data
                    string data = JsonConvert.SerializeObject(LobbyMembers);
                    SteamMatchmaking.SetLobbyData(CurrentLobbyID, "0", data);
                }

                //if we are in a lobby, make sure we have the latest lobby metadata
                if(networkState==NetworkState.Connected) {
                   string data= SteamMatchmaking.GetLobbyData(CurrentLobbyID, "0");
                    if (data.Length > 0) { LobbyMembers = JsonConvert.DeserializeObject<List<LobbyMember>>(data); }
                }

                yield return new WaitForSeconds(1 / LobbyUpdatesPerSecond);
            }
        }

        public void GetPlayerProfileImage(uint ID) {
            SteamFriends.GetSmallFriendAvatar((CSteamID)ID);
        }

        #region Steam Direct Call Backs

        /// <summary>
        /// Create new lobby for others to join.
        /// Cannot create a server if hosting or connected to a lobby already.
        /// If Successful, set network state and lobby ID.
        /// </summary>
        bool b_CreateLobby = false;
        public void CreateLobby() {
            if (b_CreateLobby | networkState == NetworkState.Hosting | networkState == NetworkState.Connected) { Debug.LogWarning("Cannot create lobby when connected to or hosting a lobby"); return; }
            ELobbyType lobbyType = ELobbyType.k_ELobbyTypePublic;
            Debug.Log("Attempting to create lobby");
            SteamMatchmaking.CreateLobby(lobbyType, 8);
            b_CreateLobby = true;
        }
        public override void OnCreateLobby(LobbyCreated_t pCallback) {
            b_CreateLobby = false;
            if (pCallback.m_eResult == EResult.k_EResultOK) {
                Debug.Log("Lobby creation succeded");
                networkState = NetworkState.Hosting;
                CurrentLobbyID = (CSteamID)pCallback.m_ulSteamIDLobby;
                LobbyMembers.Add(new LobbyMember(0, ));
            }
            else {
                Debug.Log($"Lobby creation failed because {pCallback.m_eResult}");
            }
        }

        /// <summary>
        /// Join lobby by selected index
        /// </summary>
        /// <param name="index"></param>
        public void JoinLobby(int index) {
            SteamMatchmaking.JoinLobby(lobbyIDS[index]);
        }

        /// <summary>
        /// Called upon getting list of lobbies found in our game
        /// </summary>
        /// <param name="pCallback"></param>
        public override void OnGetLobbiesList(LobbyMatchList_t pCallback) {
            Debug.Log("Found " + pCallback.m_nLobbiesMatching + " lobbies!");
            lobbyIDS.Clear();
            for (int i = 0; i < pCallback.m_nLobbiesMatching; i++) {
                CSteamID lobbyID = SteamMatchmaking.GetLobbyByIndex(i);
                lobbyIDS.Add(lobbyID);
                SteamMatchmaking.RequestLobbyData(lobbyID);
            }
        }

        /// <summary>
        /// Called from getting information about a lobby
        /// </summary>
        /// <param name="pCallback"></param>
        public override void OnGetLobbyInfo(LobbyDataUpdate_t pCallback) {
            for (int i = 0; i < lobbyIDS.Count; i++) {
                if (lobbyIDS[i].m_SteamID == pCallback.m_ulSteamIDLobby) {
                    Debug.Log("Lobby " + i + " :: " + SteamMatchmaking.GetLobbyData((CSteamID)lobbyIDS[i].m_SteamID, "name"));
                    return;
                }
            }
        }

        /// <summary>
        /// Called when a lobby is joined or user joins our lobby, called on lobby 
        /// creation as well.
        /// </summary>
        /// <param name="pCallback"></param>
        public override void OnLobbyEntered(LobbyEnter_t pCallback) {
            if (pCallback.m_EChatRoomEnterResponse == 1) {
                Debug.Log("Successfully joined lobby");
                CurrentLobbyID = (CSteamID)pCallback.m_ulSteamIDLobby;
                networkState = NetworkState.Connected;
            }
            else {
                Debug.Log($"Failed to join lobby, {pCallback.m_EChatRoomEnterResponse}");
            }
        }


        /// <summary>
        /// Called when we were invited to join a lobby, for now just join it
        /// </summary>
        /// <param name="pCallback"></param>
        public override void OnJoinLobbyRequest(GameLobbyJoinRequested_t pCallback) {
            //if we are in a lobby or own a lobby be sure to disconnect from current lobby first
            if (networkState == NetworkState.Connected | networkState == NetworkState.Hosting) {
                SteamMatchmaking.LeaveLobby(CurrentLobbyID);
            }

            //join new lobby
            SteamMatchmaking.JoinLobby(pCallback.m_steamIDLobby);
            networkState = NetworkState.Connected;
        }

        /// <summary>
        /// Chat messages are simply data pinged to all players, deserialize in the standard
        /// vektor serializer format for info.
        /// 
        /// Contains primary resolver for lobby message activity
        /// </summary>
        /// <param name="pCallback"></param>
        public void SendLobbyChatMsg(NetMessage.INetMessage message) {
            List<byte> buffer = new List<byte>();
            buffer.Zip(message, 4096);
            SteamMatchmaking.SendLobbyChatMsg(CurrentLobbyID, buffer.ToArray(), buffer.Count);
        }
        public override void OnLobbyChatMsg(LobbyChatMsg_t pCallback) {
            //TODO: if message is our own ignore

            //get message content
            byte[] a_buffer = new byte[4096];
            List<byte> buffer = new List<byte>();
            EChatEntryType chatEntryType;
            CSteamID source;
            int messageSize;
            messageSize = SteamMatchmaking.GetLobbyChatEntry(CurrentLobbyID, (int)pCallback.m_iChatID, out source, a_buffer, 4096, out chatEntryType);
            buffer = a_buffer.ToList().GetRange(0, messageSize);
            foreach (AmbiguousTypeHolder n in HexSerialize.Unzip(buffer)) {
                if (n.type == typeof(NetMessage.L_CHT)) {
                    NetMessage.L_CHT src = (NetMessage.L_CHT)n.obj;
                    ChatLog.PostChat( $"{SteamFriends.GetFriendPersonaName((CSteamID)pCallback.m_ulSteamIDUser)}: {src.message}");
                }
            }
        }
        #endregion

        /// <summary>
        /// Ensure Steam shuts down before close
        /// </summary>
        private void OnApplicationQuit() {
            SteamAPI.Shutdown();
        }
    }
}

    