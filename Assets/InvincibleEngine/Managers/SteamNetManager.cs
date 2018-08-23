//System
using System;
using System.ComponentModel;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

//Unity 
using UnityEngine;
using UnityEngine.SceneManagement;

//Steam
using _3rdParty.Steamworks.Plugins.Steamworks.NET.types.SteamClientPublic;
using _3rdParty.Steamworks.Plugins.Steamworks.NET.autogen;
using _3rdParty.Steamworks.Plugins.Steamworks.NET;

//JSON
using Newtonsoft.Json;

//Project
using HexSerializer;
using System.Globalization;
using InvincibleEngine;
using InvincibleEngine.Managers;
using InvincibleEngine.UnitFramework.Enums;

namespace SteamNet {

    /// <summary>
    /// Primary network controller in charge of: <para/>
    /// 
    ///  1) relaying data between players <para/>
    ///  2) Setting up lobbies, leaving and joining <para/>
    ///  3) Launching matches, which then hands game control over to and <para/>
    ///     passing all information about players to the Match Manager <para/>
    ///     
    /// 
    /// </summary>
    public class SteamNetManager : MonoBehaviour {

        //Network Variables
        [Header("Network Properties")]
        [SerializeField] public ENetworkState NetworkState = ENetworkState.Stopped;
        [SerializeField] private float _NetUpdatesPerSecond = 5;
        [SerializeField] private bool _TrackingEntities = false;
        [SerializeField] public bool DebugLogs = false;
        [SerializeField] public List<ushort> UsedIDs = new List<ushort>();

        //Matchmaking lookup
        [SerializeField] public LobbyData CurrentlyJoinedLobby = new LobbyData();
        [SerializeField] public List<LobbyData> OnlineLobbies = new List<LobbyData>();


        //-----------------------------------
        #region Ease of use static properties
        //-----------------------------------

        //Steamnet Instance
        public static SteamNetManager Instance;

        //Local player
        public static SteamnetPlayer LocalPlayer {
            get { return Instance.CurrentlyJoinedLobby.LobbyMembers[MySteamID]; }
        }

        //Currently joined lobby data
        public static LobbyData CurrentLobbyData {
            get { return Instance.CurrentlyJoinedLobby; }
        }

        public static CSteamID MySteamID {
            get { return SteamUser.GetSteamID(); }
        }

        public float NetUpdatesPerSecond {
            get { return 1 / _NetUpdatesPerSecond; }
            set { _NetUpdatesPerSecond = value; }
        }

        public bool Hosting {
            get { if (NetworkState == ENetworkState.Hosting) { return true; } else { return false; } }
        }

        public bool Connected {
            get { if (NetworkState == ENetworkState.Connected) { return true; } else { return false; } }
        }

        public bool TrackingEntities {
            get {
                return _TrackingEntities;
            }
            set {
                Debug.Log("Entity tracking toggled");
                _TrackingEntities = value;
                OnTrackEntitiesToggle();
            }
        }


        #endregion


        //Steam Callback Variables
        protected Callback<LobbyCreated_t> m_CreateLobby;
        protected Callback<LobbyMatchList_t> m_lobbyList;
        protected Callback<LobbyEnter_t> m_lobbyEnter;
        protected Callback<LobbyDataUpdate_t> m_lobbyInfo;
        protected Callback<GameLobbyJoinRequested_t> m_LobbyJoinRequest;
        protected Callback<LobbyChatMsg_t> m_LobbyChatMsg;
        protected Callback<LobbyChatUpdate_t> m_LobbyChatUpdate;
        protected Callback<P2PSessionRequest_t> m_P2PSessionRequest;

        //Serialization allocation
        public string jdata = "";

        /// <summary>
        /// Preload and ensure singleton
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        protected static void Preload() {
            //Make sure the Managers object exists
            GameObject Managers = GameObject.Find("Managers") ?? new GameObject("Managers");

            // Ensure this singleton initializes at startup
            if (Instance == null) Instance = Managers.GetComponent<SteamNetManager>() ?? Managers.AddComponent<SteamNetManager>();

            // Ensure this singleton does not get destroyed on scene load
            DontDestroyOnLoad(Instance.gameObject);
        }

        /// <summary>
        /// Starts steam networking
        /// </summary>
        protected bool InitializeSteam() {
            var wasInit = SteamAPI.Init();
            if (wasInit) {
                m_CreateLobby = Callback<LobbyCreated_t>.Create(OnCreateLobby);
                m_lobbyList = Callback<LobbyMatchList_t>.Create(OnGetLobbiesList);
                m_lobbyEnter = Callback<LobbyEnter_t>.Create(OnLobbyEntered);
                m_lobbyInfo = Callback<LobbyDataUpdate_t>.Create(OnGetLobbyInfo);
                m_LobbyJoinRequest = Callback<GameLobbyJoinRequested_t>.Create(OnJoinLobbyRequest);
                m_LobbyChatMsg = Callback<LobbyChatMsg_t>.Create(OnLobbyChatMsg);
                m_LobbyChatUpdate = Callback<LobbyChatUpdate_t>.Create(OnLobbyChatUpdate);
                m_P2PSessionRequest = Callback<P2PSessionRequest_t>.Create(OnP2PRequest);
            }
            return wasInit;
        }

        /// <summary>
        /// Start relative coroutines
        /// </summary>
        public void Start() {

            //Start steam, only run loop if we can start steam
            if (InitializeSteam()) {
                StartCoroutine(NetworkUpdate());
            }

            else {
                Debug.Log("Steamworks could not be started!");
            }
        }

        public void Update() {

            //Disable network view
            if (Input.GetKeyDown(KeyCode.Backslash)) {
                GUIToggle = !GUIToggle;
            }

            //Always check to see if the currently joined lobby has started the game, if so be sure to load in
            //Unless we are not in the lobby, then don't always be loading
            if (Connected & CurrentLobbyData.MatchStarted & SceneManager.GetActiveScene().buildIndex ==0) {
                BeginMatch();
            }

            //Check for packets, like, all the time, bro
            ReadPackets();
        }



        //----------------------------------------------------
        #region  Sync lobby information and other iterated network behavior like checking for state changes and launching game when ready
        //---------------------------------------------------- 

        protected IEnumerator NetworkUpdate() {
            while (true) {
                try {

                    //Grab game state from steam server if connected to server not hosting
                    if (Connected) {

                        //convert from json
                        jdata = SteamMatchmaking.GetLobbyData(CurrentlyJoinedLobby.LobbyID, "0");

                        //set to client
                        CurrentlyJoinedLobby = JsonConvert.DeserializeObject<LobbyData>(jdata);
                    }

                    //Update list of lobbies found on matchmaking
                    SteamMatchmaking.RequestLobbyList();

                    //Run steam callbacks
                    SteamAPI.RunCallbacks();

                    //Ensure that if the host left the lobby we leave
                    if (Connected) {

                        //Check to ensure the host has not left, if he did, leave lobby
                        //There is a chance that the lobby owner ID is 0 on initial start
                        if (SteamMatchmaking.GetLobbyOwner(CurrentlyJoinedLobby.LobbyID) != CurrentlyJoinedLobby.Host) {
                            if ((ulong)SteamMatchmaking.GetLobbyOwner(CurrentlyJoinedLobby.LobbyID) != 0) {
                                LeaveLobby("Host has left the lobby");
                            }
                        }
                    }

                    //Sync lobby state to steam server
                    if (Hosting) {

                        //Set relevent lobby data
                        CurrentlyJoinedLobby.ConnectedPlayers = CurrentlyJoinedLobby.LobbyMembers.Count;

                        //conver to json
                        jdata = JsonConvert.SerializeObject(CurrentlyJoinedLobby);

                        //send to steam
                        SteamMatchmaking.SetLobbyData(CurrentlyJoinedLobby.LobbyID, "0", jdata);
                    }

                    //For both clients and servers, if we are in game get into the game, ensure we are in lobby before loading
                    if (CurrentlyJoinedLobby.LobbyState == EGameState.InGame && SceneManager.GetActiveScene().buildIndex == 0) {
                        SceneManager.LoadScene(CurrentlyJoinedLobby.MapIndex);
                    }
                }

                catch (Exception e) {
                    Debug.Log($"Error in network loop {e.Message}");
                }

                //reiterate based off network updates per second
                yield return new WaitForSeconds(NetUpdatesPerSecond);
            }
        }
        #endregion

        //----------------------------------------------------
        #region  In-game networking UI for debugging
        //---------------------------------------------------- 

        public bool GUIToggle = false;


        /// <summary>
        /// Debug controls for networking
        /// </summary>
        public Rect NetBoxContainer = new Rect(Screen.width - 310, 10, 300, 200);

        GUIStyle NetBoxStyle = new GUIStyle();
        GUIStyle NetTitleLabelStyle = new GUIStyle();
        GUIStyle NetTextStyle = new GUIStyle();

        public void OnGUI() {
            if (GUIToggle) {

                //Assign style parameters
                NetBoxStyle.normal.background = new Texture2D(1, 1);
                NetBoxStyle.normal.textColor = Color.black;
                NetBoxStyle.stretchWidth = true;
                NetBoxStyle.fontSize = 12;
                NetBoxStyle.margin = new RectOffset(5, 5, 5, 5);

                NetTitleLabelStyle.normal.textColor = Color.black;
                NetTitleLabelStyle.fontSize = 18;
                NetTitleLabelStyle.margin = new RectOffset(5, 5, 5, 5);

                NetTextStyle.normal.textColor = Color.black;
                NetTextStyle.fontSize = 12;
                NetTextStyle.margin = new RectOffset(5, 5, 5, 5);

                //Start Area
                GUILayout.BeginArea(NetBoxContainer, NetBoxStyle);

                GUILayout.BeginVertical();

                GUILayout.Label("Network Panel", NetTitleLabelStyle);
                GUILayout.Label($"Status: {NetworkState}", NetTextStyle);

                GUI.backgroundColor = Color.grey;

                GUILayout.EndVertical();

                GUILayout.BeginHorizontal();

                //Start Server
                if (GUILayout.Button(!Hosting ? "Open Lobby" : "Leave Lobby")) {
                    if (Hosting) {
                        LeaveLobby("");
                    }
                    else if (!Hosting) {
                        CreateLobby();
                    }
                }

                GUILayout.EndHorizontal();

                if (Hosting | Connected) {

                    //List of network players
                    GUILayout.BeginVertical();

                    //Display all lobby member names
                    GUILayout.Label("Lobby Members", NetTitleLabelStyle);
                    foreach (KeyValuePair<CSteamID, SteamnetPlayer> n in CurrentlyJoinedLobby.LobbyMembers) {

                        //if they are host show icons
                        GUILayout.Label(!n.Value.IsHost ? n.Value.DisplayName : $"{n.Value.DisplayName} (Host)", NetTextStyle);
                    }


                    //Host tools
                    GUILayout.Label("Host tools", NetTitleLabelStyle);

                    if (TrackingEntities) {
                        GUI.backgroundColor = Color.green;
                    }
                    else {
                        GUI.backgroundColor = Color.red;
                    }

                    if (GUILayout.Button(!TrackingEntities ? "Start Tracking Entities" : "Stop Tracking Entities")) {
                        if (TrackingEntities) {
                            TrackingEntities = false;
                        }
                        else {
                            TrackingEntities = true;
                        }
                    }

                    GUI.backgroundColor = Color.grey;
                    GUILayout.EndVertical();

                    //Entity Tracking Information
                    GUILayout.BeginVertical();
                    
                    GUILayout.EndVertical();
                }

                GUILayout.EndArea();

            }
        }
        #endregion

        //----------------------------------------------------
        #region  Entity Tracking Calculations (sending and recieving)
        //----------------------------------------------------

        /// <summary>
        /// Entity tracking is very expensive, complex, and will consume a lot of processing power
        /// as it stands, objects will have priority values based on how long since they received a network
        /// update request multiplied by a priority constant (IE. buildings are not as important as jets). 
        /// This data is sent asyncronously over the network and retrieved as soon as they can
        /// </summary>


        //Called on togggle entities, if we are a client then remove all entities in scene
        public void OnTrackEntitiesToggle() {


        }

        //TODO: implement a system for syncing fields by custom serializer and using a priority system OR keyframing

        public void OnTrackEntities() {

            //do nothing if not tracking entities
            if (!TrackingEntities) {
                return;
            }

            ///if hosting, scrape all necessary data and relay it to all clients
            ///this posts a session request that the other users must accept on frist time send 
            if (Hosting) {

                List<byte> buffer = new List<byte>();



                //after entities are zipped, send to all remote connections
                foreach (KeyValuePair<CSteamID, SteamnetPlayer> n in CurrentlyJoinedLobby.LobbyMembers) {

                    //Dont sent messages to ourself, duh
                    if (n.Key == MySteamID) {
                        continue;
                    }

                    //Debug to console
                    if (DebugLogs) Debug.Log($"Entity tracking packet sent of size {buffer.Count} to user {n.Value.DisplayName}");

                    //Sends the collection of synced entities to all users
                    SteamNetworking.SendP2PPacket(n.Key, buffer.ToArray(), (uint)buffer.Count, EP2PSend.k_EP2PSendReliable);
                }

            }

        }

        //Call to read packets available from network stack
        public void ReadPackets() {

            ///if connected, do the following:
            ///
            /// 1) check to see if any new packets exists to be unpacked
            /// 2) see if there exists an entity that has a matching ID, set its properties
            /// 3) if there exists no entity with that ID, create one base off it's asset ID and set its properties
            if (Connected) {
                uint messageSize = 0;

                while (SteamNetworking.IsP2PPacketAvailable(out messageSize, 0)) {

                    //Debug
                    if (DebugLogs) Debug.Log($"Recieved packed from host of size {messageSize}");

                    //Allocate buffer
                    byte[] buffer = new byte[messageSize];
                    uint packetSize = 0;
                    CSteamID sourcePlayer;

                    //Read Packet
                    //Data is now allocated to buffer, read data and resolve
                    SteamNetworking.ReadP2PPacket(buffer, messageSize, out packetSize, out sourcePlayer, 0);

                    //Resolve Packet
                    OnPacketRecieved(buffer);
                }
            }
        }

        //Called on packet recieved, handles behavior from network
        public void OnPacketRecieved(byte[] buffer) {

            N_ENT e;


            //Gut message
            List<AmbiguousTypeHolder> segments = HexSerialize.Unzip(buffer);

            if (DebugLogs) Debug.Log($"Packet recieved, has {segments.Count} segments.");

            foreach (AmbiguousTypeHolder n in segments) {


                //Cast to entity update
                e = (N_ENT)n.obj;

            }
        }


        //fetches a new ID from list of available IDs
        public ushort GenerateID() {
            ushort pickedID = 0;
            while (UsedIDs.Contains(pickedID)) {
                pickedID++;
            }
            UsedIDs.Add(pickedID);
            return pickedID;
        }

        #endregion

        //----------------------------------------------------
        #region  Data sending peer to peer or to all players, establish connections to host
        //----------------------------------------------------

        //called when user attempts data transmission, accept by default
        public void OnP2PRequest(P2PSessionRequest_t param) {

            Debug.Log("Remote session request recieved, accepting and opening port");
            SteamNetworking.AcceptP2PSessionWithUser(param.m_steamIDRemote);

        }


        /// <summary>
        /// Sends a message to the host if connected to one, or sends a message to all connected clients if hosting a game
        /// doing this generates GC as the buffer and data stream must be filled with bytes and destroyed, this is normal
        /// </summary>
        /// <param name="message"></param>
        public void SendDataToLobby(object[] message) {

            //Prepare buffer of serialized message
            List<byte> buffer = new List<byte>();

            //Iterate through all messages and pack them into buffer
            for(int i=0;i<message.Length; i++) {
                HexSerialize.Zip(buffer, message[i], 10000);
            }

            //If hosting, relay to all clients
            if (Hosting) {

            }

            //If connected, relay to host
            if(Connected) {

            }

        }
        
        #endregion

        //----------------------------------------------------
        #region  Lobby Creation, joining, leaving, disconnection, connection
        //----------------------------------------------------

        //Tracks active lobby request
        bool LobbyRequest = false;

        //Try and create lobby
        public void CreateLobby() {

            //check to see if we are networked currently
            if (Hosting | Connected | LobbyRequest) {
                Debug.Log("Cannot create lobby if hosting, connected, or there is a pending lobby create");
                return;
            }

            //otherwise try and get steam to create the server
            SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypePublic, 8);
            LobbyRequest = true;
        }

        //Called from response on lobby creation
        public void OnCreateLobby(LobbyCreated_t param) {

            //reset lobby request
            LobbyRequest = false;

            //if create lobby suceeded 
            if (param.m_eResult == EResult.k_EResultOK) {

                //Instantiate current lobby
                CurrentlyJoinedLobby = new LobbyData();

                //Set proper values
                Debug.Log("Lobby creation succeded");
                NetworkState = ENetworkState.Hosting;

                CurrentlyJoinedLobby.Name = $"{SteamFriends.GetPersonaName()}'s game";
                CurrentlyJoinedLobby.Host = MySteamID;
                CurrentlyJoinedLobby.LobbyID = (CSteamID)param.m_ulSteamIDLobby;

                //create lobby member for the current user
                CurrentlyJoinedLobby.AddNewPlayer(new SteamnetPlayer(MySteamID));
               

            }

            else {
                Debug.Log($"Lobby creation failed because {param.m_eResult}");
            }
        }

        //Call to leave the current lobby
        public void LeaveLobby(string reason) {

            //display message
            if (reason.Length > 0) {
                UIMessage.GlobalMessage.DisplayMessage(reason);
            }

            //Leave the lobby
            SteamMatchmaking.LeaveLobby(CurrentLobbyData.LobbyID);

            //Reset lobby data
            CurrentlyJoinedLobby = new LobbyData();

            //Reset Network State
            NetworkState = ENetworkState.Stopped;
        }

        //Join lobby by parameter
        public void JoinLobby(CSteamID LobbyID) {
            //check to see if we are networked currentlys
            if (Hosting | Connected | LobbyRequest) {
                Debug.Log("Cannot join new lobby if hosting, connected, or there is a pending lobby request");
                return;
            }

            //otherwise join lobby like normal
            SteamMatchmaking.JoinLobby(LobbyID);
        }

        //Called on lobby member state change, used by hosts when a user joins/leaves
        private void OnLobbyChatUpdate(LobbyChatUpdate_t param) {

            //Check users entering or leaving
            switch ((EChatMemberStateChange)param.m_ulSteamIDUserChanged) {


                //User disconnected or left
                case (EChatMemberStateChange.k_EChatMemberStateChangeDisconnected | EChatMemberStateChange.k_EChatMemberStateChangeLeft): {

                        //if in game, stop game
                        if (CurrentLobbyData.MatchStarted) {
                            EndMatch($"Player {SteamFriends.GetFriendPersonaName((CSteamID)param.m_ulSteamIDUserChanged)}");
                        }

                        //If the host leaves, leave the lobby
                        if ((CSteamID)param.m_ulSteamIDUserChanged == CurrentLobbyData.Host) {

                        }

                        //Only hosts do anything here
                        if (Hosting) {

                            //Player disconneted, remove them from the list of players
                            CurrentLobbyData.RemovePlayer((CSteamID)param.m_ulSteamIDUserChanged);

                        }
                        break;
                    }

                //User entered the lobby
                case EChatMemberStateChange.k_EChatMemberStateChangeEntered: {

                        //Only hosts can add members
                        if (Hosting) {

                            //If we are in a match, do NOT let the new player in
                            //TODO: let them in, but only let them spectate?
                            if (CurrentLobbyData.MatchStarted) {
                                break;
                            }

                            //Create user in match
                            CurrentLobbyData.AddNewPlayer(new SteamnetPlayer((CSteamID)param.m_ulSteamIDUserChanged));

                        }

                        break;
                    }
            }

        }


        //Called when we enter a lobby
        private void OnLobbyEntered(LobbyEnter_t param) {

            //reset lobby request
            LobbyRequest = false;

            //if the lobby is joined sucessfully
            if (param.m_EChatRoomEnterResponse == 1) {

                //if we are hosting a user just joined, adding the user is handled below
                if (NetworkState == ENetworkState.Hosting) {
                    Debug.Log($"User {param.m_ulSteamIDLobby} joined the lobby");
                }

                //if we are not hosting we just joined a lobby
                else {
                    Debug.Log("Successfully joined lobby");
                    CurrentLobbyData.LobbyID = (CSteamID)param.m_ulSteamIDLobby;
                    NetworkState = ENetworkState.Connected;
                }
            }

            //if the lobby join failed
            else {
                Debug.Log($"Failed to join lobby, {param.m_EChatRoomEnterResponse}");
            }
        }

        //Called when we want to join a friend's game, leave our current lobby and join theirs 
        private void OnJoinLobbyRequest(GameLobbyJoinRequested_t param) {

            //leave any lobbies we are a part of
            LeaveLobby("Joining different lobby");

            //Join remote lobby
            SteamMatchmaking.JoinLobby(param.m_steamIDLobby);
        }
        #endregion

        //----------------------------------------------------
        #region  Lobby updates and chat messages
        //----------------------------------------------------

        //Sends chat message to all users if host, or host if client
        public void BroadcastChatMessage(object message) {

            //Serialize object for send
            byte[] buffer = HexSerialize.ToByte(message);

            //Send to lobby
            SteamMatchmaking.SendLobbyChatMsg(CurrentLobbyData.LobbyID, buffer, buffer.Length);
        }

        //Lobby Chat message
        private void OnLobbyChatMsg(LobbyChatMsg_t param) {

            //Declare variables
            byte[] buffer = new byte[4096];
            CSteamID playerSource;
            EChatEntryType entryType;

            //Pull message
            int messageSize = SteamMatchmaking.GetLobbyChatEntry(CurrentLobbyData.LobbyID, (int)param.m_iChatID, out playerSource, buffer, 4096, out entryType);
            Debug.Log(messageSize);
            byte[] trim = buffer.SubArray(0, messageSize);


            //Unzip
            List<AmbiguousTypeHolder> messages = HexSerialize.Unzip(trim);

            //iterate through messages and act upon each
            foreach (AmbiguousTypeHolder n in messages) {

                //Chat message
                if (n.type == typeof(N_CHT)) {
                    N_CHT m = (N_CHT)n.obj;

                    CurrentlyJoinedLobby.PostChat(m.message, SteamFriends.GetFriendPersonaName((CSteamID)param.m_ulSteamIDUser));
                }

                //Player Team Change Request
                if (n.type == typeof(N_TMC)) {
                    N_TMC m = (N_TMC)n.obj;

                    if (Hosting) {
                        CurrentlyJoinedLobby.LobbyMembers[(CSteamID)param.m_ulSteamIDUser].Team = (ETeam)m.team;
                    }
                }

                //Only comes from clients, signals that they are ready
                if (n.type == typeof(N_RDY)) {
                    if (Hosting) {
                        CurrentlyJoinedLobby.LobbyMembers[(CSteamID)param.m_ulSteamIDUser].IsReady = (CurrentlyJoinedLobby.LobbyMembers[(CSteamID)param.m_ulSteamIDUser].IsReady ? false : true);

                    }
                }
            }
        }

        #endregion

        //----------------------------------------------------
        #region  Lobby List and lobby information as well as starting game and timer
        //----------------------------------------------------

        //Timer Routine
        private Coroutine TimerRoutine;

        //Attempt to start timer
        public void StartGame() {

            //if we are hosting, authoritate this
            if (Hosting) {

                //if we are host, just stop timer
                if (CurrentLobbyData.GameStartingIn != 5) {
                    ResetTimer();
                    return;
                }

                //Make sure everyone is ready
                if (CurrentLobbyData.ArePlayersReady()) {
                    TimerRoutine = StartCoroutine(StartTimer());
                }
            }

            //if we are connected to a game this will toggle our ready status
            if (Connected) {
                SteamNetManager.Instance.BroadcastChatMessage(new N_RDY());
            }
        }

        //Reset timer on ready status change
        public void ResetTimer() {

            //Post chat
            CurrentLobbyData.PostChat("Countdown Stopped", "");

            //End coroutine
            StopCoroutine(TimerRoutine);

            //Reset timer value
            CurrentLobbyData.GameStartingIn = 5;
        }

        /// <summary>
        /// Call to start a timer that counts down to game start
        /// </summary>
        /// <returns></returns>
        IEnumerator StartTimer() {
            while (true) {

                //Post Chat
                CurrentLobbyData.PostChat($" Starting in {CurrentLobbyData.GameStartingIn}", "Lobby");

                //increment game timer
                CurrentLobbyData.GameStartingIn--;

                //If not all players are ready, stop timer and reset
                if (!CurrentLobbyData.ArePlayersReady()) {
                    ResetTimer();
                    yield return null;
                }

                //If the timer hits 0, signal game start
                if (Hosting && CurrentLobbyData.GameStartingIn < 0) {

                    //If the timer gets to 0, signal game start
                    BeginMatch();
                }

                yield return new WaitForSeconds(1);
            }
        }


        /// <summary>
        /// Call to start the match, do not call directly. Will extract the current lobby data
        /// being used to launch the correct map, at which point the match manager will take over
        /// all match control until the game ends and we are sent to the lobby.
        /// 
        /// At any point after this if the host leaves the game will end, but clients can reconnect potentially
        /// </summary>
        public void BeginMatch() {

            //if hosting, do a lot more than just load into the scene
            if (Hosting) {

                //Set relative lobby data
                CurrentLobbyData.MatchStarted = true;
            }

            //in the end, both load scene, account for index 0 being lobby, so increment by 1
            if (Connected | Hosting) {
                
                //Load desired level
                SceneManager.LoadScene(CurrentLobbyData.MapIndex + 1);
            }
        }

        /// <summary>
        /// Call to end the game
        /// this can be because a win condition was met or a user disconnected
        /// but it is not up to this function to decide this
        /// </summary>
        public void EndMatch(string reason) {

            Debug.Log($"Game ended: {reason}");

            if(Hosting) {

                //Set relative lobby data
                CurrentLobbyData.MatchStarted = false;

            }

            if (Connected | Hosting) {
             
                //Return to lobby
                SceneManager.LoadScene(0);
            }

        }

        // Called from getting information about a lobby
        private void OnGetLobbyInfo(LobbyDataUpdate_t param) {

        }

        //fetches all lobbies available
        private void OnGetLobbiesList(LobbyMatchList_t param) {
            uint lobbiesFound = param.m_nLobbiesMatching;


            //clear list
            OnlineLobbies.Clear();

            //Rejuvinate lobby list
            for (int i = 0; i < lobbiesFound; i++) {

                //get lobby ID
                CSteamID lobbyId = SteamMatchmaking.GetLobbyByIndex(i);

                //get lobby information
                string key = SteamMatchmaking.GetLobbyData(lobbyId, "0");

                //convert using json                
                LobbyData m = JsonConvert.DeserializeObject<LobbyData>(key);
                if (m == null) {
                    continue;
                }
                m.LobbyID = lobbyId;

                //Add lobby to list
                OnlineLobbies.Add(m);

            }
        }

        //returns lobby member by ID
        public SteamnetPlayer GetMemberFromID(CSteamID Id) {
            return CurrentlyJoinedLobby.LobbyMembers[Id];
        }
        #endregion

        private void OnApplicationQuit() {
            SteamMatchmaking.LeaveLobby(CurrentLobbyData.LobbyID);
        }
    }
}
