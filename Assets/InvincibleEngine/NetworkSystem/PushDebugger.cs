using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Custom debuger to display messages in game
/// </summary>
public class PushDebugger : MonoBehaviour {
    public List<Message> MessageList = new List<Message>();
    public float MessageTimeToLive = 5;

    public static PushDebugger Instance;

    /// <summary>
    /// Preload and ensure singleton
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    protected static void Preload() {
        //Make sure the Managers object exists
        GameObject Managers = GameObject.Find("Managers") ?? new GameObject("Managers");

        // Ensure this singleton initializes at startup
        if (Instance == null) Instance = Managers.GetComponent<PushDebugger>() ?? Managers.AddComponent<PushDebugger>();

        // Ensure this singleton does not get destroyed on scene load
        DontDestroyOnLoad(Instance.gameObject);
    }


    public struct Message {
        public string message;
        public float timeCreated;
    }

    public void PushDebug(string message) {
        Message n = new Message();
        n.message = message;
        n.timeCreated = Time.time;
        MessageList.Add(n);
        
    }
    //go through each message
    //first ones
    private void OnGUI() {
       

    }
}


