using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controls match behavior, statistics, order dispatch, and any other behavior for the game
/// </summary>
public class MatchManager : MonoBehaviour {
    //Match specific variables
    public bool MatchStarted;

    // Singleton Instance Accessor
    public static MatchManager Instance { get; private set; }

    //Preloaded Gameobejcts used by the manager
    public GameObject StructureDropship;

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

        //Spawn a dropship and pass necessary parameters
    }


    #endregion
    //----------------------------------------------------
    #region  Starting/Stopping match
    //----------------------------------------------------

    //Start match if everyone is ready, load into map and set lobby data
    public void StartMatch(int MapID) {

    }

    //End match, return to lobby and dump game data
    public void EndMatch() {

    }
    #endregion

}
