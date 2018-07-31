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
using InvincibleEngine;
using InvincibleEngine.UnitFramework.Components;
using InvincibleEngine.UnitFramework.DataTypes;

using VektorLibrary.EntityFramework.Components;

//Player object for game communication
public class Player {    
    public int Resources;
    public int SpawnSlot;
    public int Team;
}


/// <summary>
/// Controls match behavior, statistics, order dispatch, and any other behavior for the game
/// </summary>
public class MatchManager : MonoBehaviour {

    //Match Grid
    public GridSystem GridSystem = new GridSystem();

    //Match specific variables
    public bool MatchStarted;

    //List of players in the game
    public Dictionary<CSteamID, Player> Players = new Dictionary<CSteamID, Player>();

    // Singleton Instance Accessor
    public static MatchManager Instance { get; private set; }

    //Easy access to the local lobby data
    public LobbyData CurrentLobbyData {
        get { return SteamNetManager.Instance.CurrentlyJoinedLobby; }
    }

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

    /// <summary>
    /// Check for win conditions and other game related states
    /// </summary>
    private void Update() {

    }

    /// <summary>
    /// On start:
    /// 
    /// Generate the grid to be used in the game, clients and hosts do this and it should be identical 
    /// 
    /// </summary>
    private void Start() {

        //Generate Grid
        GridSystem.GenerateGrid();

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
    /// Accepts unit commands from either directly from the player manager of this client
    /// or from the network from other clients, excecutes them if this game is a host match
    /// 
    /// if this is a client game then the command is relayed to the host machine
    /// </summary>
    /// <param name="command"></param>
    public void OnCommand(UnitCommand command, CSteamID playerSource) {


    }


    /// <summary>
    /// Constructs a building, consuming resources to do so
    /// </summary>
    /// <param name="BuildingID">Building asset ID</param>
    /// <param name="Location">cooresponding grid coordinates</param>
    /// <param name="Orientation">rotation in degrees between 0 and 360</param>
    public void ConstructBuilding(StructureBehavior Building, GridPoint point, Quaternion Orientation, CSteamID playerSource) {

        //if we are connected to a server, send a build request
        if (SteamNetManager.Instance.Connected) {

        }

        //if we are hosting, attempt to construct building
        if (SteamNetManager.Instance.Hosting) {

            //if the player can afford it, construct it
            if (CurrentLobbyData.LobbyMembers[playerSource].Economy.OnUseResources(Building.Cost)) {

                //Instantiate object
                GameObject n = Instantiate(Building.gameObject, point.WorldPosition, Orientation);

                //Set ownership to the player that built it
                n.GetComponent<StructureBehavior>().PlayerOwner = playerSource;

            }
        }
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
