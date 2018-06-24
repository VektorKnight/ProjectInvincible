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
    #region  Replicator and object control
    //----------------------------------------------------

    //Checks to see when we should be replicating
    private void CheckReplication() {

    }

    //Call to run replication protocol, only on hosts, do not call once per frame.
    private void ReplicateEntities() {

    }

    //Remove all entities in scene that have an entity component to prepare for being a client
    private void ClearEntities() {

    }

    //Called by new entities on hosts to register themselves for replication
    public void RegisterEntity() {

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
