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
using InvincibleEngine.UnitFramework.Enums;

using VektorLibrary.EntityFramework.Components;
using InvincibleEngine.Managers;
using InvincibleEngine.CameraSystem;
using VektorLibrary.Collections;
using VektorLibrary.Utility;

/// <summary>
/// Controls match behavior, statistics, order dispatch, and any other behavior for the game
/// </summary>
public class MatchManager : MonoBehaviour {

    // All gameplay units in scene
    public HashedArray<UnitBehavior> UnitList = new HashedArray<UnitBehavior>(1024);

    public Dictionary<ushort, UnitBehavior> AllUnits = new Dictionary<ushort, UnitBehavior>();

    //Match manager properties
    [SerializeField] public GridSystem GridSystem = new GridSystem();

    // Singleton Instance Accessor
    public static MatchManager Instance { get; private set; }


    //------------------------------------
    #region Events
    //------------------------------------

    public delegate void _DOnMatchStart();
    public static event _DOnMatchStart OnMatchStartEvent;


    #endregion


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
        bool a = SteamNetManager.CurrentLobbyData.LobbyMembers[player].Economy.SuffucientResources(Building.BuildCost);
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
    public bool ConstructBuilding(StructureBehavior Building, GridPoint point, Vector3 Orientation, CSteamID playerSource, bool bypassRequirements) {

        //if we are connected to a server, send a build request
        if (SteamNetManager.Instance.Connected) {

        }

        //if we are hosting, attempt to construct building
        if (SteamNetManager.Instance.Hosting && (bypassRequirements || CanConstructBuilding(Building, point, playerSource))) {

            Debug.Log("Spawning building, checking to see if player has enough econ for it");

            //if the player can afford it, construct it
            if (SteamNetManager.CurrentLobbyData.LobbyMembers[playerSource].Economy.OnUseResources(Building.BuildCost)) {

                Debug.Log("Player can afford building, spawning it");

                SpawnUnit(SteamNetManager.Instance.GetNetworkID(),
                    Building.AssetID, 
                    point.WorldPosition,
                    Orientation, 
                    playerSource);

                return true;
            }
        }

        //return false if nothing worked
        return false;

    }

   

    /// <summary>
    /// call to finally spawn unit, all instantiations for networked units MUST be done here
    /// </summary>
<<<<<<< HEAD
    public void SpawnUnit(ushort netID, ushort assetID, Vector3 position, Vector3 rotation, CSteamID owner) {
=======
    public UnitBehavior SpawnUnit(ushort netID, ushort assetID, Vector3 position, Vector3 rotation, PlayerTeam team, CSteamID owner) {
>>>>>>> abba111f889a94044a08b3dd381d69f8324d6103

        //Spawn physical object
        var newUnit = Instantiate(AssetManager.LoadAssetByID(assetID), position, Quaternion.Euler(rotation));

        //Set values
        newUnit.PlayerOwner = owner;
        newUnit.SetTeam(SteamNetManager.CurrentLobbyData.LobbyMembers[owner].Team);
        newUnit.NetID = netID;

        //Add unit to list
        AllUnits.Add(netID, newUnit);
        UnitList.Add(newUnit);

        // Return unit reference
        return newUnit;
    }

    /// <summary>
    /// Destroys a unit by it's network ID.
    /// </summary>
    public void DestroyUnit(ushort netID) {
        UnitList.Remove(AllUnits[netID]);
        Destroy(AllUnits[netID].gameObject);
        AllUnits.Remove(netID);
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

        //Call events
        OnMatchStartEvent?.Invoke();

        //Reset all values for new match
        AllUnits.Clear();

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
                Debug.LogWarning("MatchManager: Found existing camera system in scene on match start!\n" +
                                                      "The existing camera system will be destroyed.");
                Destroy(cam.gameObject);
            }

            // Load the Camera System prefab from the resources folder and try to spawn it
            var cameraSystem = AssetManager.LoadAsset<InvincibleCamera>("Objects/Common/OverheadCamera");
            Instantiate(cameraSystem, spawnPoints[0].transform.position, Quaternion.identity);
        }
        catch (Exception e) {
            Debug.LogError("MatchManager: Error spawning in camera system prefab!\n" +
                                                e.Message);
        }

        // If hosting, Spawn command centers for each player in the match and assign them their starting resources
        if (isHost) {

            foreach (var n in SteamNetManager.CurrentLobbyData.LobbyMembers) {
                Debug.Log($"Spawning Command Center and setting initial economy values for <b>{n.Value.DisplayName}</b>");
                //Give each player starting resources
                n.Value.Economy.Resources = SteamNetManager.CurrentLobbyData.StartingResources;

                //For each player, spawn them (for now) a command center into a spawn point round robin, assign the building to them
                SpawnUnit(SteamNetManager.Instance.GetNetworkID(),
                    AssetManager.CommandCenter.AssetID,
                    GridSystem.WorldToGridPoint(spawnPoints[spawnIndex].transform.position).WorldPosition,
                    Vector3.zero,
                    n.Key);

                //Move to next spawn point
                spawnIndex++;
            }
        }


    }
    //End match, return to lobby and dump game data
    public void EndMatch() {

    }
    #endregion

    //----------------------------------------------------
    #region Network packet resolution
    //----------------------------------------------------

    /// <summary>
    /// Takes a collection of network updates and assigns to units
    /// if no unit exists with the 
    /// </summary>
    /// <param name="messages"></param>
    public void OnNetworkMessage(IEnumerable<AmbiguousTypeHolder> messages) {

        //go through each message and resolve it
        foreach (AmbiguousTypeHolder n in messages) {

            //Entity update            
            if (n.type == typeof(N_ENT)) {
                N_ENT u = (N_ENT)n.obj;

                //Check to see if the entity exists
                if (AllUnits.ContainsKey(u.NetID)) {
                    AllUnits[u.NetID].transform.position = u.P;
                    AllUnits[u.NetID].transform.eulerAngles = u.R;
                }

                //if not, spawn this unit
                else {
<<<<<<< HEAD
                    SpawnUnit(u.NetID, u.ObjectID, u.P, u.R, (CSteamID)u.Owner);
=======
                    SpawnUnit(u.NetID, u.ObjectID, u.P, u.R, PlayerTeam.Blue, (CSteamID)u.Owner);
>>>>>>> abba111f889a94044a08b3dd381d69f8324d6103
                }
            }
        }
    }

    #endregion

}
