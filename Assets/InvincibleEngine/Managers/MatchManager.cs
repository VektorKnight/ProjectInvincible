//System
using System;
using System.ComponentModel;
using System.Collections;
using System.Windows.Input;
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
using InvincibleEngine.CameraSystem;
using VektorLibrary.Utility;

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

        //If scene index is not zero (lobby)           and  not in edtior       or    nothing I can't get input before scene loads
        if (SceneManager.GetActiveScene().buildIndex != 0 & (Application.isEditor)) {
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

    }

    /*
    private void OnDrawGizmos() {
        foreach (KeyValuePair<Vector2Int, GridPoint> n in GridSystem.GridPoints) {

            if (n.Value.IsOpen()) {
                Gizmos.color = Color.green;
            }
            else {
                Gizmos.color = Color.red;

            }
            Gizmos.DrawWireCube(n.Value.WorldPosition, Vector3.one);
        }
    }
    */

    //----------------------------------------------------
    #region  Game flow control, spawning players and command centers on Game Start
    //----------------------------------------------------

    /// <summary>
    /// Called by unity when the level loads
    /// </summary>
    /// <param name="level"></param>
    private void OnLevelWasLoaded(int level) {

        //Signal the match to start if we're not in the lobby
        if (SceneManager.GetActiveScene().buildIndex != 0) {

            //Signal OnMatchStart to the match manager
            MatchManager.Instance.OnMatchStart(SteamNetManager.Instance.Hosting);
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
    /// Returns true if we have the money/room to construct something, locally determined - host authoritated
    /// </summary>
    /// <returns></returns>
    public bool CanConstructBuilding(StructureBehavior Building, GridPoint point, CSteamID player) {

        //Check all conditions
        bool a = SteamNetManager.CurrentLobbyData.LobbyMembers[player].Economy.SuffucientResources(Building.Cost);
        bool b = Instance.GridSystem.WorldToGridPoint(InvincibleCamera.MouseData.WorldPosition).IsOpen();
        bool c = GridSystem.WorldToGridPoints(point.WorldPosition, Building.Bounds.x, Building.Bounds.y).AreOpen();

        return (a & b & c) ? true : false;
        
    }

    /// <summary>
    /// Constructs a building, consuming resources to do so
    /// </summary>
    /// <param name="BuildingID">Building asset ID</param>
    /// <param name="Location">cooresponding grid coordinates</param>
    /// <param name="Orientation">rotation in degrees between 0 and 360</param>
    public bool ConstructBuilding(StructureBehavior Building, GridPoint point, Quaternion Orientation, CSteamID playerSource, bool bypassRequirements) {

        //if we are connected to a server, send a build request
        if (SteamNetManager.Instance.Connected) {

        }

        //if we are hosting, attempt to construct building
        if (SteamNetManager.Instance.Hosting && (bypassRequirements||CanConstructBuilding(Building, point, playerSource))) {

            Debug.Log("Spawning building, checking to see if player has enough econ for it");
            //if the player can afford it, construct it
            if ( SteamNetManager.CurrentLobbyData.LobbyMembers[playerSource].Economy.OnUseResources(Building.Cost)) {
                Debug.Log("Player can afford building, spawning it");

                //Instantiate object
                GameObject n = Instantiate(Building.gameObject, point.WorldPosition, Orientation);

                //Set ownership to the player that built it
                n.GetComponent<StructureBehavior>().PlayerOwner = playerSource;
                
                //
                GridSystem.OnOccupyGrid(GridSystem.WorldToGridPoints(point.WorldPosition, Building.Bounds.x, Building.Bounds.y));

                return true;

            }
        }

        //return false if nothing worked
        return false;
       
    }


    #endregion

    //----------------------------------------------------
    #region  Starting/Stopping match
    //----------------------------------------------------

    /// <summary>
    /// On match start, this will fire before anything else in the game loads
    /// This should spawn in command centers for each player, give economy, etc.
    /// </summary>
    public void OnMatchStart(bool isHost) {

        // Generate Grid
        GridSystem.GenerateGrid();

        // Locate all spawn points
        GameObject[] spawnPoints = GameObject.FindGameObjectsWithTag("SpawnPoint");
        int spawnIndex = 0;
        
        // Try to spawn in the camera system prefab
        try {
            // Destroy any existing camera systems
            var existingCameras = FindObjectsOfType<InvincibleCamera>();
            foreach (var cam in existingCameras) {
                DevConsole.LogWarning("MatchManager", "Found existing camera system in scene on match start!\n" +
                                                      "The existing camera system will be destroyed.");
                Destroy(cam.gameObject);
            }
            
            // Load the Camera System prefab from the resources folder and try to spawn it
            var cameraSystem = Resources.Load<InvincibleCamera>("Objects/Common/OverheadCamera");
            Instantiate(cameraSystem, spawnPoints[0].transform.position, Quaternion.identity);
        }
        catch (Exception e) {
            DevConsole.LogError("MatchManager", $"Error spawning in camera system prefab!\n" +
                                                e.Message);
        }

        // If hosting, Spawn command centers for each player in the match and assign them their starting resources
        if (isHost) {

            foreach (KeyValuePair<CSteamID, SteamnetPlayer> n in SteamNetManager.CurrentLobbyData.LobbyMembers) {
                DevConsole.Log("MatchManager", $"Spawning Command Center and setting initial economy values for <b>{n.Value.DisplayName}</b>");
                //Give each player starting resources
                n.Value.Economy.Resources = SteamNetManager.CurrentLobbyData.StartingResources;

                //For each player, spawn them (for now) a command center into a spawn point round robin, assign the building to them
                ConstructBuilding(AssetManager.CommandCenter, GridSystem.WorldToGridPoint(spawnPoints[spawnIndex].transform.position), Quaternion.identity, n.Key, true);

                //Move to next spawn point
                spawnIndex++;
            }
        }


    }
    //End match, return to lobby and dump game data
    public void EndMatch() {

    }
    #endregion

}
