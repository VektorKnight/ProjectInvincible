using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SteamNet;
using _3rdParty.Steamworks.Plugins.Steamworks.NET.types.SteamClientPublic;

/// <summary>
/// Inherit to get UI events for displaying things
/// </summary>
public class UIBehavior : MonoBehaviour {


    //------------------------------------
    #region virtual methods
    //------------------------------------
    
    /// <summary>
    /// Match started (loaded into game)
    /// </summary>
    public virtual void OnMatchStart() {
        Debug.Log("UI Event: Match Started");
    }

    /// <summary>
    /// Joined a lobby
    /// </summary>
    public virtual void OnJoinLobby() {
        Debug.Log("UI Event: Lobby Joined");
    }


    /// <summary>
    /// Online Lobby Changed
    /// </summary>
    /// <param name="added"></param>
    /// <param name="changed"></param>
    public virtual void OnOnlineLobbyUpdate(bool added, LobbyData changed, CSteamID id) {
        string n = added ? "added" : "removed";
        Debug.Log($"Lobby {changed.Name} has been {n}");
    }

    #endregion



    //------------------------------------
    #region Subscribe overrides
    //------------------------------------

    //Subscribe 
    private void OnEnable() {
        MatchManager.OnMatchStartEvent += OnMatchStart;
        SteamNetManager.OnEnterLobby += OnJoinLobby;
        SteamNetManager.OnOnlineLobbyUpdate += OnOnlineLobbyUpdate;
    }

    //Unsubscribe
    private void OnDisable() {
        MatchManager.OnMatchStartEvent -= OnMatchStart;
        SteamNetManager.OnEnterLobby -= OnJoinLobby;
        SteamNetManager.OnOnlineLobbyUpdate -= OnOnlineLobbyUpdate;

    }

    #endregion

}
