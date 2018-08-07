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
using InvincibleEngine.Managers;

/// <summary>
/// Controls match behavior, statistics, order dispatch, and any other behavior for the game
/// </summary>
public class MatchManager : MonoBehaviour {

    //Match manager properties
    [SerializeField] public GridSystem GridSystem = new GridSystem();
   
    // Singleton Instance Accessor
    public static MatchManager Instance { get; private set; }
    

    ///Force the game to start in the lobby scene, as we move toward an online
    ///match based game it is simply too hard to put checks everywhere that bypass
    ///the expected state of the game and list of players. Maps from the build settings
    ///can be selected in the lobby to load and test
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void ForceLobbyLoad() {

        //If scene index is not zero (lobby), load lobby
        if(SceneManager.GetActiveScene().buildIndex!=0 & !Application.isEditor) {
            SceneManager.LoadScene(0);
        }        
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

      
        //If we load into a scene that is not the lobby, start the game with the current network lobby data and players
        if (SceneManager.GetActiveScene().buildIndex != 0) {
            MatchManager.Instance.OnMatchStart(SteamNetManager.Instance.Hosting);
        }

    }

    //----------------------------------------------------
    #region  Game flow control, spawning players and command centers on Game Start
    //----------------------------------------------------

    /// <summary>
    /// On match start, this will fire before anything else in the game loads
    /// This should spawn in command centers for each player
    /// </summary>
    public void OnMatchStart(bool isHost) {

        //Generate Grid
        GridSystem.GenerateGrid();

        //Locate all spawn points
        GameObject[] spawnPoints = GameObject.FindGameObjectsWithTag("SpawnPoint");
        int spawnIndex = 0;

        //Only if host
        if (isHost) {

            //Spawn command centers for each player in the match
            foreach (KeyValuePair<CSteamID, SteamnetPlayer> n in SteamNetManager.CurrentLobbyData.LobbyMembers) {

                //For each player, spawn them (for now) into a spawn point round robin, assign the building to them
                ConstructBuilding(AssetManager.CommandCenter, GridSystem.WorldToGridPoint(spawnPoints[spawnIndex].transform.position), Quaternion.identity, n.Key);

                //Move to next spawn point
                spawnIndex++;
            }
        }
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
            if (SteamNetManager.CurrentLobbyData.LobbyMembers[playerSource].Economy.OnUseResources(Building.Cost)) {

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
