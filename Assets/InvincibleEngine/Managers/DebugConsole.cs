using System;
using System.Windows;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Creates and manages a command-line window for the engine.
/// </summary>
internal class DebugConsole : MonoBehaviour {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN

    // Console Fields
    private readonly Windows.ConsoleWindow _consoleWindow = new Windows.ConsoleWindow();
    private readonly Windows.ConsoleInput _consoleInput = new Windows.ConsoleInput();

    // Singleton pattern
    public static DebugConsole Instance;

    /// <summary>
    /// Preload and ensure singleton
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    protected static void Preload() {
        //Make sure the Managers object exists
        var managers = GameObject.Find("Managers") ?? new GameObject("Managers");

        // Ensure this singleton initializes at startup
        if (Instance == null) Instance = managers.GetComponent<DebugConsole>() ?? managers.AddComponent<DebugConsole>();

        // Ensure this singleton does not get destroyed on scene load
        DontDestroyOnLoad(Instance.gameObject);
    }

    // Unity Monobehavior Callbacks
    #region Unity Monobehavior Callbacks
    // Unity OnEnable Callback
    private void OnEnable() {
        DontDestroyOnLoad(gameObject);

        _consoleWindow.Initialize();
        _consoleWindow.SetTitle("Invincible Console");

        Application.logMessageReceived += HandleLog;

        Debug.Log("Debug Console Enabled!");
    }

    // Unity Update Callback
    private void Update() {
        _consoleInput.Update();
    }

    // Unity OnDisable Callback
    private void OnDisable() {
        Application.logMessageReceived -= HandleLog;
    }

    // Unity OnDestroy Callback
    private void OnDestroy() {
        _consoleWindow.Shutdown();
    }
    #endregion

    // Message Handling Functions
    #region Message Handlers
    // MessageReceived Event Handler (Unity Redirect)
    private void HandleLog(string message, string stackTrace, LogType type) {
        // Set based on the log type
        var logTrace = false;

        // Handle primary message types
        switch (type) {
            case LogType.Warning:
                Console.ForegroundColor = ConsoleColor.Yellow;
                break;
            case LogType.Error:
                Console.ForegroundColor = ConsoleColor.Red;
                logTrace = true;
                break;
            case LogType.Exception:
                Console.ForegroundColor = ConsoleColor.Magenta;
                logTrace = true;
                break;
            default:
                Console.ForegroundColor = ConsoleColor.White;
                break;
        }

        // Log messages to the console
        if (logTrace) {
            Console.WriteLine(message);
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine(stackTrace);
        }
        else {
            Console.WriteLine(message);
        }

        // Make sure we don't lose user input
        _consoleInput.RedrawInputLine();
    }

    /// <summary>
    /// Logs a message directly to the console window without using the Unity Debug interface.
    /// Much faster as a stacktrace is not included.
    /// </summary>
    /// <param name="message">The message to log.</param>
    /// <param name="type">The type of message (determines color).</param>
    public void LogDirect(string message, LogType type = LogType.Log) {
        // Handle primary message types
        switch (type) {
            case LogType.Warning:
                Console.ForegroundColor = ConsoleColor.Yellow;
                break;
            case LogType.Error:
                Console.ForegroundColor = ConsoleColor.Red;
                break;
            case LogType.Exception:
                Console.ForegroundColor = ConsoleColor.Magenta;
                break;
            default:
                Console.ForegroundColor = ConsoleColor.White;
                break;
        }

        // Log messages to the console
        Console.WriteLine(message);

        // Make sure we don't lose user input
        _consoleInput.RedrawInputLine();
    }
    #endregion
#endif
}
