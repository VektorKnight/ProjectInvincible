using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using InvincibleEngine.Networking;
using Newtonsoft.Json;
using InvincibleEngine.Managers;
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

        ///Lobby data change request from client to host
        public class L_RDY : INetMessage {
          

        }

        /// <summary>
        /// Team Change request from client to host
        /// </summary>
        public class L_TCH {
            public int team;
        }


        //lobby closed, everyone leaves
        public class L_CLS : INetMessage {

        }

        //Entity update
        public class G_ENT : INetMessage {
            public ushort EntityID;
            public List<object> SyncFields;
        }
    }

    /// <summary>
    /// Data for lobby members in the game, created only by servers
    /// </summary>
    [Serializable]
    public class LobbyMember {
        public LobbyMember(int team, ulong steamID) {
            Team = team;
            SteamID = steamID;
        }

        public bool Ready = false;
        public int Team;
        public ulong SteamID;

        public string Name { get { return SteamFriends.GetFriendPersonaName((CSteamID)SteamID); } }
    }

    /// <summary>
    /// Game chat log from any and all sources
    /// </summary>
    [Serializable]
    public static class ChatLog {
        public static string chat;
        public static void PostChat(string input) {
            chat += $"\n{input}";
        }
    }

    /// <summary>
    /// Game launch options
    /// </summary>
    public class GameOptions {
        public int map = 0;
        public bool GameStarted = false;
    }

    /// <summary>
    /// Custom attributes for field syncing
    /// </summary>
    public class SyncField : Attribute { bool ditry; }
    public class SyncEvent : Attribute { }

    /// <summary>
    /// Network manager, in charge of all network behavior in the game
    /// </summary>
    public class NetManager : SteamHelper {

        //Singleton Implementation
        private static NetManager _Singleton = null;
        public static NetManager Singleton { get { if (_Singleton == null) { _Singleton = GameObject.Find("Managers").GetComponent<NetManager>(); } return _Singleton; } private set { } }

        //TODO: Refactor to account for lobbies and games
        //Server State
        [Header("State of network")]
        public ENetworkState NetworkState = ENetworkState.Stopped;
        public enum ENetworkState {
            Hosting, Connected, Stopped
        }

        [Header("State of game")]
        public EGameState GameState;
        public enum EGameState {
            Started, Ended, Paused
        }

        //Teams
        public Color[] Teams;

        //game options
        public GameOptions GameOptions;

        //Lobby Data
        [Header("Lobby Data")]
        public CSteamID CurrentLobbyID;
        public List<CSteamID> lobbyIDS;
        [SerializeField]
        public List<LobbyMember> LobbyMembers = new List<LobbyMember>();

        //returns local player
        public LobbyMember LocalPlayer { get { return LobbyMembers.Find(o => o.SteamID == (ulong)SteamUser.GetSteamID()); } }

        public float LaunchGameCountdown = 5;

        //Update parameters
        public int LobbyUpdatesPerSecond = 1;

        //Exposed network data

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
                if (NetworkState == ENetworkState.Hosting) {
                   /* //clear lobby list
                    LobbyMembers.Clear();

                    //get number of players in lobby
                    int numPlayers = SteamMatchmaking.GetNumLobbyMembers(CurrentLobbyID);

                    Debug.Log("\t Number of players currently in lobby : " + numPlayers);
                    for (int i = 0; i < numPlayers; i++) {
                        CSteamID n = SteamMatchmaking.GetLobbyMemberByIndex(CurrentLobbyID, i);

                        LobbyMembers.Add(new LobbyMember(1, (ulong)n));
                    }
                    */
                    //sort members in list by team
                    LobbyMembers.OrderBy(o => o.Team);


                    //set lobby data
                    string U_LobbyMemberList = JsonConvert.SerializeObject(LobbyMembers);
                    string U_GameOptions = JsonConvert.SerializeObject(GameOptions);

                    SteamMatchmaking.SetLobbyData(CurrentLobbyID, "0", U_LobbyMemberList);
                    SteamMatchmaking.SetLobbyData(CurrentLobbyID, "1", U_GameOptions);

                }


                //if we are in a lobby, make sure we have the latest lobby metadata
                if (NetworkState==ENetworkState.Connected) {
                    string data_0= SteamMatchmaking.GetLobbyData(CurrentLobbyID, "0");
                    string data_1= SteamMatchmaking.GetLobbyData(CurrentLobbyID, "1");

                    Debug.Log(data_0 +" :: " + data_1);

                    if (data_0.Length > 0 && data_1.Length>0) {
                        LobbyMembers = JsonConvert.DeserializeObject<List<LobbyMember>>(data_0);
                        GameOptions = JsonConvert.DeserializeObject<GameOptions>(data_1);
                    }
                }

                yield return new WaitForSeconds(1 / LobbyUpdatesPerSecond);
            }
        }

        /// <summary>
        /// Returns lobby member from ID
        /// </summary>
        /// <param name="Id">ID of player in question</param>
        /// <returns></returns>
        public LobbyMember GetMemberFromID(CSteamID Id) {
            return LobbyMembers.Find(o => o.SteamID == (ulong)Id);
        }

        /// <summary>
        /// returns avatar of user
        /// </summary>
        /// <param name="user">Target user</param>
        /// <returns></returns>
        public Texture2D GetSmallAvatar(ulong user) {
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

        #region Steam Direct Call Backs

        /// <summary>
        /// Create new lobby for others to join.
        /// Cannot create a server if hosting or connected to a lobby already.
        /// If Successful, set network state and lobby ID.
        /// </summary>
        bool b_CreateLobby = false;
        public void CreateLobby() {
            if (b_CreateLobby | NetworkState == ENetworkState.Hosting | NetworkState == ENetworkState.Connected) { Debug.LogWarning("Cannot create lobby when connected to or hosting a lobby"); return; }
            ELobbyType lobbyType = ELobbyType.k_ELobbyTypePublic;
            Debug.Log("Attempting to create lobby");
            SteamMatchmaking.CreateLobby(lobbyType, 8);
            b_CreateLobby = true;
        }
        public override void OnCreateLobby(LobbyCreated_t pCallback) {

            //We are no longer trying to make a lobby
            b_CreateLobby = false;

            //if create lobby suceeded 
            if (pCallback.m_eResult == EResult.k_EResultOK) {

                //Set proper values
                Debug.Log("Lobby creation succeded");
                NetworkState = ENetworkState.Hosting;
                CurrentLobbyID = (CSteamID)pCallback.m_ulSteamIDLobby;

                //create lobby member for the current user
                LobbyMembers.Add(new LobbyMember(0, (ulong)SteamUser.GetSteamID()));
            }
            else {
                Debug.Log($"Lobby creation failed because {pCallback.m_eResult}");
            }
        }

        /// <summary>
        /// Leaves active lobby
        /// </summary>
        public void LeaveLobby() {

            //if we are the host, tell everyone else to leave the lobby
            if(NetworkState== ENetworkState.Hosting) {
                SendLobbyChatMsg(new NetMessage.L_CLS());               
            }

            //Leave lobby
            SteamMatchmaking.LeaveLobby(CurrentLobbyID);

            //clear lobby members
            LobbyMembers.Clear();
            NetworkState = ENetworkState.Stopped;
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

            //iterate through all lobbies to 
            for (int i = 0; i < lobbyIDS.Count; i++) {
                if (lobbyIDS[i].m_SteamID == pCallback.m_ulSteamIDLobby) {
                    Debug.Log("Lobby " + i + " :: " + SteamMatchmaking.GetLobbyData((CSteamID)lobbyIDS[i].m_SteamID, "0"));
                    return;
                }
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
        /// Lobby Chatroom state has changed, usually when users leave or join
        /// </summary>
        /// <param name="param"></param>
        protected override void OnLobbyChatUpdate(LobbyChatUpdate_t param) {

            //Only act upon this if we are hosting
            if (NetworkState == ENetworkState.Hosting) {
                CSteamID user = (CSteamID)param.m_ulSteamIDUserChanged;

                //if a user just left remove them from the list
                if ((EChatMemberStateChange)param.m_rgfChatMemberStateChange == EChatMemberStateChange.k_EChatMemberStateChangeDisconnected) {
                    LobbyMembers.Remove(LobbyMembers.Find(o => o.SteamID == param.m_ulSteamIDUserChanged));
                    
                }
                if ((EChatMemberStateChange)param.m_rgfChatMemberStateChange == EChatMemberStateChange.k_EChatMemberStateChangeEntered) {
                    LobbyMembers.Add(new LobbyMember(0, user.m_SteamID));
                }

            }
        }

        /// <summary>
        /// Called when a lobby is joined or user joins our lobby, called on lobby 
        /// creation as well.
        /// </summary>
        /// <param name="pCallback"></param>
        public override void OnLobbyEntered(LobbyEnter_t pCallback) {

            //if the lobby is joined sucessfully
            if (pCallback.m_EChatRoomEnterResponse == 1) {

                //if we are hosting a user just joined
                if (NetworkState == ENetworkState.Hosting) {
                    
                }

                //if we are not hosting we just joined a lobby
                else {
                    Debug.Log("Successfully joined lobby");
                    CurrentLobbyID = (CSteamID)pCallback.m_ulSteamIDLobby;
                    NetworkState = ENetworkState.Connected;
                }
            }

            //if the lobby join failed
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
            if (NetworkState == ENetworkState.Connected | NetworkState == ENetworkState.Hosting) {
                SteamMatchmaking.LeaveLobby(CurrentLobbyID);
            }

            //join new lobby
            SteamMatchmaking.JoinLobby(pCallback.m_steamIDLobby);
            NetworkState = ENetworkState.Connected;
        }

        /// <summary>
        /// Chat messages are simply data pinged to all players, deserialize in the standard
        /// hexnet serializer format for info.
        /// 
        /// Contains primary resolver for lobby message activity
        /// </summary>
        /// <param name="pCallback"></param>
        public void SendLobbyChatMsg(NetMessage.INetMessage message) {
            List<byte> buffer = new List<byte>();
            buffer.Zip(message, 4096);
            SteamMatchmaking.SendLobbyChatMsg(CurrentLobbyID, buffer.ToArray(), buffer.Count);
        }

        /// <summary>
        /// Called if lobby chat message is recieved
        /// </summary>
        /// <param name="pCallback"></param>
        public override void OnLobbyChatMsg(LobbyChatMsg_t pCallback) {
            //TODO: if message is our own ignore

            //get message content
            byte[] a_buffer = new byte[4096];
            List<byte> buffer = new List<byte>();
            EChatEntryType chatEntryType;
            CSteamID csource;
            int messageSize;
            messageSize = SteamMatchmaking.GetLobbyChatEntry(CurrentLobbyID, (int)pCallback.m_iChatID, out csource, a_buffer, 4096, out chatEntryType);
            buffer = a_buffer.ToList().GetRange(0, messageSize);
            LobbyMember msource = LobbyMembers.Find(o => o.SteamID == (ulong)csource);

            //Resolver for all types of lobby messages
            foreach (AmbiguousTypeHolder n in HexSerialize.Unzip(buffer)) {

                //Chat message recieved, display it
                if (n.type == typeof(NetMessage.L_CHT)) {
                    NetMessage.L_CHT src = (NetMessage.L_CHT)n.obj;
                    ChatLog.PostChat( $"{SteamFriends.GetFriendPersonaName((CSteamID)pCallback.m_ulSteamIDUser)}: {src.message}");
                }

                //Leave lobby call, 
                if(n.type==typeof(NetMessage.L_CLS)) {

                    //if the message came from the host only
                    if(NetworkState==ENetworkState.Connected && (CSteamID)pCallback.m_ulSteamIDUser == SteamMatchmaking.GetLobbyOwner(CurrentLobbyID)) {
                        LeaveLobby();
                    }
                }

                //Ready message, can only come from clients to host
                if(n.type==typeof(NetMessage.L_RDY)) {
                    Debug.Log($"Player {msource.Ready} has toggled ready");
                    if(NetworkState== ENetworkState.Hosting) {
                        LobbyMembers.Find(o => o.SteamID == (ulong)csource).Ready = true;
                        
                    }
                }
            }
        }
        #endregion

        /// <summary>
        /// Primary resolver for all network messages, different on clients and servers
        /// </summary>
        /// <param name="message"></param>
        public void OnNetMessage(NetMessage.INetMessage message) {
            if(NetworkState== ENetworkState.Connected) {
               
            }
        }

        /// <summary>
        /// Starts the game with the current game settings, initiates a timer that can be aborted
        /// </summary>
        public void LaunchGame() {

            //Do not start if we are not the host and a player isnt ready
            if(NetworkState!= ENetworkState.Hosting) {

                return;
            }
            foreach(LobbyMember n in LobbyMembers) {

                //dont check if we, the host, are ready
                if(n.SteamID==(ulong)SteamUser.GetSteamID()) {
                    continue;
                }
                if(!n.Ready) {

                    return;
                }
            }

            Debug.Log("Able to start game, starting timer");

            //else start game
            StartCoroutine(LaunchGameTimer());
        }
        IEnumerator LaunchGameTimer() {        
            while(true) {
                LaunchGameCountdown -= 0.1f;
                LaunchGameCountdown = Mathf.Clamp(LaunchGameCountdown,0,5);                
                yield return new WaitForSecondsRealtime(0.1f); ;
            }
        }
        /// <summary>
        /// Aborts the start game,
        /// </summary>
        public void LaunchGameAbort() {
            Debug.Log("Aborting game start");
            LaunchGameCountdown = 5;
            StopCoroutine(LaunchGameTimer());
        }

        /// <summary>
        /// Make connections with all players and set proper parameters
        /// </summary>
        public void OnGameStart() {

        }

        /// <summary>
        /// Ensure Steam shuts down before close
        /// </summary>
        private void OnApplicationQuit() {
            SteamAPI.Shutdown();
        }
    }
}

    
