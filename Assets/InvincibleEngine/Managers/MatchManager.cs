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

//Internal
using SteamNet;

public class Player {

    public CSteamID PlayerID;

    public int Resources;
    public int SpawnSlot;
    public int Team;

}

/// <summary>
/// Controls match behavior, statistics, order dispatch, and any other behavior for the game
/// </summary>
public class MatchManager : MonoBehaviour {

    //Match specific variables
    public bool MatchStarted;
    
    //List of players in the game
    public Dictionary<CSteamID, Player> Players = new Dictionary<CSteamID, Player>();

    // Singleton Instance Accessor
    public static MatchManager Instance { get; private set; }

    // Preload Method
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Preload() {
        //Make sure the Managers object exists
        GameObject Managers = GameObject.Find("Managers") ?? new GameObject("Managers");

        // Ensure this singleton initializes at startup
        if (Instance == null) Instance = Managers.GetComponent<MatchManager>() ?? Managers.AddComponent<MatchManager>();

        // Ensure this singleton does not get destroyed on scene load
        DontDestroyOnLoad(Instance.gameObject);
    }

    private void Start() {
        
    }

    /// <summary>
    /// Check for win conditions and other game related states
    /// </summary>
    private void Update() {

    }

    //----------------------------------------------------
    #region  Game flow control, spawning players and command centers on Game Start
    //----------------------------------------------------

    //On match start, spawn in command centers
    public void OnMatchStart() {

    }

    #endregion

    //----------------------------------------------------
    #region  Basic Game functionality (spawn building, command relay)
    //----------------------------------------------------

    /// <summary>
    /// Spawns a dropship that will take the structure to the destination and deploy it
    /// </summary>
    /// <param name="BuildingID">Building asset ID</param>
    /// <param name="Location">World location excluding height</param>
    /// <param name="Orientation">rotation in degrees between 0 and 360</param>
    public void ConstructBuilding(ushort BuildingID, Vector2 Location, int Orientation) {

    }


    #endregion

    //----------------------------------------------------
    #region  Starting/Stopping match
    //----------------------------------------------------

    //Start match if everyone is ready, load into map and set lobby data.
    //Spawns player objects for each player joining
    public void StartMatch(LobbyData data) {

    }

    //End match, return to lobby and dump game data
    public void EndMatch() {

    }
    #endregion

}
