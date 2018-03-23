using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The Game manager is in charge of issuing commands, scraping data for network, loading levels, and most other game functions
/// </summary>
public class MatchManager : MonoBehaviour {

    //Scruct for stats about games
    public struct GameStats {
        public int Winner;
        public int[] Scores;
    }

    //Stats about the game that just occured
    public GameStats PreviousGameStats = new GameStats();

    // Singleton Instance Accessor
    public static MatchManager Instance { get; private set; }
    
    // Preload Method
    [RuntimeInitializeOnLoadMethod]
    private static void Preload() {
        //Make sure the Managers object exists
        GameObject Managers = GameObject.Find("Managers") ?? new GameObject("Managers");       

        // Ensure this singleton initializes at startup
        if (Instance == null) Instance = Managers.AddComponent<MatchManager>();

        // Ensure this singleton does not get destroyed on scene load
        DontDestroyOnLoad(Instance.gameObject);
    }

    /// <summary>
    /// Ends game, returns to lobby scene with after game report
    /// </summary>
    public void EndGame() {

    }
}
