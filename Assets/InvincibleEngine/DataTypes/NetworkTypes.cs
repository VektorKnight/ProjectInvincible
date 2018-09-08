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
using SteamNet;
using HexSerializer;
using System.Globalization;
using InvincibleEngine;
using InvincibleEngine.Managers;
using InvincibleEngine.UnitFramework.Enums;


namespace SteamNet {

    //----------------------------------------------------
    #region  Enumerators
    //----------------------------------------------------
    /// <summary>
    /// State of network
    /// </summary>
    public enum ENetworkState {
        Stopped, Hosting, Connected
    }

    /// <summary>
    /// State of the match
    /// </summary>
    public enum EGameState {
        InLobby, InGame
    }
    #endregion

    //----------------------------------------------------
    #region  Quick network type intercom function
    //----------------------------------------------------

    /// <summary>
    /// Converts strings to CSteamIDs
    /// </summary>
    public class SteamConverter : TypeConverter {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) {

            if (sourceType == typeof(string)) {
                return true;
            }
            else return false;
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value) {
            return new CSteamID(ulong.Parse((string)value));
        }
    }

    #endregion

    //----------------------------------------------------
    #region  Network packet types
    //----------------------------------------------------

    /// <summary>
    /// Types of network messages that can be sent
    /// </summary>
    public class N_ENT {
        public Vector3 P, R, V, A;
        public ushort NetID;
        public ushort ObjectID;
        public ulong Owner;

    }

    /// <summary>
    /// Chat
    /// </summary>
    public class N_CHT {
        public string message;
    }

    /// <summary>
    /// Team Change Request
    /// </summary>
    public class N_TMC {
        public int team;

        public N_TMC(int team) {
            this.team = team;
        }
    }

    /// <summary>
    /// Ready Signal
    /// </summary>
    public class N_RDY { }

    #endregion

    //----------------------------------------------------
    #region  General Game Command Types
    //----------------------------------------------------

    //Construct Building request
    public class CBuildStructure {

        //Constructor
        public CBuildStructure(ushort assetID, Vector3 location) {
            AssetID = assetID;
            Location = location;
        }

        //Fields
        public ushort AssetID;
        public Vector3 Location;      
    }

    //General ping communication to teammates
    public class CPing {

        //Constructor
        public CPing(Vector3 location) {
            Location = location;
        }

        //Fields
        public Vector3 Location;

    }


    #endregion

    //----------------------------------------------------
    #region Lobby and player data
    //----------------------------------------------------

    /// <summary>
    /// Data about individual players in a lobby
    /// </summary>
    [Serializable]
    public class SteamnetPlayer {

        //Constructor
        public SteamnetPlayer(CSteamID steamID) {
            SteamID = steamID;
        }

        //Player properties
        [SerializeField] public bool IsReady = false;
        [SerializeField] public CSteamID SteamID;
        [SerializeField] public ETeam Team = 0;
        [SerializeField] public Economy Economy = new Economy();

        //Player useful information
        public bool IsHost {
            get {
                ulong a, b;
                a = (ulong)SteamNetManager.Instance.CurrentlyJoinedLobby.Host;
                b = (ulong)SteamID;
                
                return a==b;
            }
        }
        public string DisplayName {
            get {
                return SteamFriends.GetFriendPersonaName(SteamID);
            }
        }

    }

    /// <summary>
    /// Data about the lobby, can only be changed by the host
    /// 
    /// For new projects, populate with any variables you want synced, DO NOT touch 
    /// the variables found in the base file
    /// </summary>
    [Serializable]
    public class LobbyData {

        //Constructor
        public LobbyData(CSteamID lobbyID) {
            LobbyID = lobbyID;
        }

        public LobbyData() {

        }

        //Lobby Properties
        [SerializeField] public CSteamID LobbyID;
        [SerializeField] public CSteamID Host;
        [SerializeField] public string Name = "New Lobby";
        [SerializeField] public int MaxPlayers = 4;
        [SerializeField] public int ConnectedPlayers;
        [SerializeField] public EGameState LobbyState = EGameState.InLobby;
        [SerializeField] public Dictionary<CSteamID, SteamnetPlayer> LobbyMembers = new Dictionary<CSteamID, SteamnetPlayer>();
        [SerializeField] public string ChatLog = "";
        [SerializeField] public int GameStartingIn = 5;
        [SerializeField] public int StartingResources = 10000;

        //Match Properties
        [SerializeField] public int MapIndex = 1;
        [SerializeField] public bool MatchStarted = false;


        //-----------------------------------
        #region Lobby functions
        //-----------------------------------

        /// <summary>
        /// Call to add player
        /// </summary>
        /// <param name="playerData"></param>
        public void AddNewPlayer(SteamnetPlayer playerData) {
            LobbyMembers.Add(playerData.SteamID, playerData);
        }

        /// <summary>
        /// Call to remove player
        /// </summary>
        /// <param name="player"></param>
        public void RemovePlayer(CSteamID player) {
            LobbyMembers.Remove(player);
        }

        /// <summary>
        /// Post chat to lobby, do not call directly, this is used by the network manager 
        /// </summary>
        /// <param name="message"> Raw message</param>
        /// <param name="source"> Player name string</param>
        public void PostChat(string message, string source) {
            if (message.Length > 0) { ChatLog += $"<b>{source}</b>: {message}\n"; }
        }

        /// <summary>
        /// Checks to see if all players are ready, only used on hosts to determine start game
        /// </summary>
        /// <returns></returns>
        public bool ArePlayersReady() {
            foreach (KeyValuePair<CSteamID, SteamnetPlayer> n in LobbyMembers) {
                if (n.Value.IsReady == false && !n.Value.IsHost) {
                    return false;
                }
            }
            return true;
        }
        #endregion
    }

    /// <summary>
    /// Barebones information about a lobby found online
    /// </summary>
    [Serializable]
    public class OnlineLobbyInfo {
        public string name = "";
        public CSteamID Id;
    }

    #endregion

    //----------------------------------------------------
    #region  Network communication types
    //----------------------------------------------------

    /// <summary>
    /// Holds type information that came from a serialized data steam
    /// </summary>
    public class AmbiguousTypeHolder {
        public AmbiguousTypeHolder(object _obj, Type _type) {
            obj = _obj;
            type = _type;
        }
        public object obj;
        public Type type;
    }

    /// <summary>
    /// Holds metadata about fields and their index
    /// </summary>
    public class SyncField {
        public byte Dirty = 0;
        public byte Index;
        public object Data;
    }
    #endregion
}


