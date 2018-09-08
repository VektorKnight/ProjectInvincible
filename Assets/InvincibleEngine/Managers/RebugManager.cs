using System;
using System.Windows;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Wtf is a rebug manager???
// ya fuckin' wanker
class RebugManager : MonoBehaviour {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN

    //Console Fields
    Windows.ConsoleWindow console = new Windows.ConsoleWindow();
    Windows.ConsoleInput input = new Windows.ConsoleInput();

    //Singleton pattern
    public static RebugManager Instance;

    /// <summary>
    /// Preload and ensure singleton
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    protected static void Preload() {
        //Make sure the Managers object exists
        GameObject Managers = GameObject.Find("Managers") ?? new GameObject("Managers");

        // Ensure this singleton initializes at startup
        if (Instance == null) Instance = Managers.GetComponent<RebugManager>() ?? Managers.AddComponent<RebugManager>();

        // Ensure this singleton does not get destroyed on scene load
        DontDestroyOnLoad(Instance.gameObject);
    }

    string strInput;

    //
    // Create console window, register callbacks
    //
    void OnEnable() {
        DontDestroyOnLoad(gameObject);

        console.Initialize();
        console.SetTitle("Invincible Console");

        Application.logMessageReceived += HandleLog;

        Debug.Log("Console Started");
    }

    private void OnDisable() {
        Application.logMessageReceived -= HandleLog;
    }

    //
    // Debug.Log* callback
    //
    void HandleLog(string message, string stackTrace, LogType type) {
        if (type == LogType.Warning)
            System.Console.ForegroundColor = ConsoleColor.Yellow;
        else if (type == LogType.Error)
            System.Console.ForegroundColor = ConsoleColor.Red;
        else
            System.Console.ForegroundColor = ConsoleColor.White;

        System.Console.WriteLine(message);

        // If we were typing something re-add it.
        input.RedrawInputLine();
    }

    //
    // Update the input every frame
    // This gets new key input and calls the OnInputText callback
    //
    void Update() {
        input.Update();
    }

    //
    // It's important to call console.ShutDown in OnDestroy
    // because compiling will error out in the editor if you don't
    // because we redirected output. This sets it back to normal.
    //
    void OnDestroy() {
        console.Shutdown();
    }

#endif
}
