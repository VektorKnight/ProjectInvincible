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


namespace SteamNet {

    //Conversion of JSON Strings to Steam types
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

    /// <summary>
    /// Types of network messages that can be sent
    /// </summary>
    public class N_ENT {
        //Position, Rotation, Velocity, Angular Velocity
        //TODO: Add delta acceleration and other predicition factors
        public Vector3 P, R, V, A;
        public ushort NetID;
        public ushort ObjectID;

        //TODO:
        //public List<object> SyncFields;

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

    /// <summary>
    /// Data about individual players in a lobby
    /// </summary>
    [Serializable]
    public class SteamnetPlayer {
        public bool IsReady = false;
        public CSteamID SteamID;
        public int team = 0;

        public bool IsHost {
            get {
                return SteamMatchmaking.GetLobbyOwner(SteamNetManager.Instance.CurrentLobbyID) == SteamID;
            }
        }
        public string DisplayName {
            get {
                return SteamFriends.GetFriendPersonaName(SteamID);
            }
        }

        public SteamnetPlayer(bool isReady, CSteamID steamID) {
            IsReady = isReady;
            SteamID = steamID;
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
        //ID
        public CSteamID LobbyID;

        //Host of game
        public CSteamID Host;

        //State of lobby
        public EGameState LobbyState = EGameState.InLobby;

        //Server Name
        public string Name = "";

        //List of all players
        public Dictionary<CSteamID, SteamnetPlayer> LobbyMembers = new Dictionary<CSteamID, SteamnetPlayer>();

        //Max players allowed in the lobby
        public int MaxPlayers = 4;
        public int ConnectedPlayers;

        //Map index, starts at 1
        public int MapIndex = 1;

        //Indicated if this match has started 
        public bool MatchStarted = false;

        //Chat log
        public void PostChat(string message, string source) {
            if (message.Length > 0) { ChatLog += $"<b>{source}</b>: {message}\n"; }
        }
        public string ChatLog = "";

        //Check to see if everyone is ready
        public bool ArePlayersReady() {
            foreach (KeyValuePair<CSteamID, SteamnetPlayer> n in LobbyMembers) {
                if (n.Value.IsReady == false && !n.Value.IsHost) {
                    return false;
                }
            }
            return true;
        }

        //Game start and timer
        private bool _TimerStarted = false;
        public bool TimerStarted {
            get { return _TimerStarted; }
            set { TimerTime = Time.time; _TimerStarted = value; }
        }

        public float TimerTime;

        public int TimerDisplay {
            get {
                return 6 - Mathf.Clamp(Mathf.CeilToInt((Time.time - TimerTime)), 0, 6);
            }
        }

        public double TimerOverlayPercent {
            get {
                return Mathf.Clamp(Mathf.Repeat(Time.time - TimerTime, 1), 0, 5);
            }
        }
    }

    /// <summary>
    /// Barebones information about a lobby found online
    /// </summary>
    [Serializable]
    public class OnlineLobbyInfo {
        public string name = "";
        public CSteamID Id;
    }

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
}
