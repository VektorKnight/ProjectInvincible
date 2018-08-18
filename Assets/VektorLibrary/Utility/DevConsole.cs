﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using InvincibleEngine.DataTypes;
using UnityEngine;
using UnityEngine.UI;

namespace VektorLibrary.Utility {
    /// <summary>
    /// Controller for the development console.
    /// </summary>
    public class DevConsole : MonoBehaviour {
        // Singleton instance
        public static DevConsole Instance { get; private set; }

        // Unity Inspector
        [Header("Input Config")] 
        [SerializeField] private KeyCode _toggleKey = KeyCode.BackQuote;
        [SerializeField] private KeyCode _commandKey = KeyCode.Return;
        
        [Header("Required Objects")] 
        [SerializeField] private Text _consoleText;
        [SerializeField] private InputField _commandInput;
        
        // Private: Required References
        private Canvas _canvas;
        private Scrollbar _scrollBar;
        
        // Private: String Builder
        private StringBuilder _stringBuilder;
        
        // Private Static: Message Queue / Command Registry
        private static readonly Queue<string> MessageQueue = new Queue<string>();
        private static readonly Dictionary<string, Action> CommandRegistry = new Dictionary<string, Action>();
        
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Preload() {
            // Load the prefab from the common objects folder
            var prefab = Resources.Load<Canvas>("Objects/Common/DevConsole");
            
            // Destroy any existing instances
            if (Instance != null) Destroy(Instance.gameObject);
            
            // Instantiate and assign the instance
            var instance = Instantiate(prefab, Vector3.zero, Quaternion.identity);
            Instance = instance.GetComponentInChildren<DevConsole>();

            // Ensure this singleton does not get destroyed on scene load
            DontDestroyOnLoad(Instance.transform.root);
            
            // Initialize the instance
            Instance.Initialize();
        }
        
        // Initialization
        private void Initialize() {
            // Reference required components
            _canvas = Instance.GetComponentInParent<Canvas>();
            _scrollBar = GetComponentInChildren<Scrollbar>();
            
            // Initialize the string builder
            _stringBuilder = new StringBuilder();
            
            // Enable console by default in development builds
            _canvas.enabled = false;
        }
       
        // Log a message to the console
        public static void Log(string caller, string message, string nameColor = "lightblue") {
            try {
                MessageQueue.Enqueue($"<color={nameColor}>[{caller}]</color> {message}\n");
                
                if (Instance._scrollBar != null)
                    Instance._scrollBar.value = 0f;
                
                #if UNITY_EDITOR
                Debug.Log(message);
                #endif
            }
            catch (Exception ex) {
                Debug.LogException(ex);
            }
        }
        
        // Log a warning message to the console
        public static void LogWarning(string caller, string message) {
            try {
                MessageQueue.Enqueue($"<color=#f9ba1bff>[{caller}]</color> {message}\n");
                Instance._canvas.enabled = true;
                
                if (Instance._scrollBar != null)
                    Instance._scrollBar.value = 0f;
                
                #if UNITY_EDITOR
                Debug.LogWarning(message);
                #endif
            }
            catch (Exception ex) {
                Debug.LogException(ex);
            }
        }
        
        // Log an error message to the console
        public static void LogError(string caller, string message) {
            try {
                MessageQueue.Enqueue($"<color=#ff6666ff>[{caller}]</color> {message}\n");
                Instance._canvas.enabled = true;
                
                if (Instance._scrollBar != null)
                    Instance._scrollBar.value = 0f;
                
                #if UNITY_EDITOR
                Debug.LogError(message);
                #endif
            }
            catch (Exception ex) {
                Debug.LogException(ex);
            }
        }
        
        // Registers a command
        public static void RegisterCommand(string keyword, Action action) {
            CommandRegistry.Add(keyword.ToLower(), action);
        }
        
        // Try to parse a command string
        public static void ParseCommand(string entry) {
            // Split the command string and remove empty entries
            var rawData = entry.Split(' ');
            rawData = rawData.Where(x => !string.IsNullOrEmpty(x)).ToArray();
            rawData[0] = rawData[0].ToLower();
            
            // Check for special "help" command
            if (rawData[0] == "help") {
                foreach (var command in CommandRegistry) {
                    Log("Help", $"<b>{command.Key}</b> invokes <b>{command.Value.Method.Name}</b>", "lime");
                }
                return;
            }
                
            
            // Exit if the keyword is not in the registry
            if (!CommandRegistry.ContainsKey(rawData[0])) {
                // Inform the user that the given command is malformed or invalid
                LogWarning("Commands", $"Invalid or malformed command: <b>{rawData[0]}</b>");
                return;
            }
            
            // Invoke the action paired to the specified keyword
            Log("Commands", $"Executing command: <b>{rawData[0]}</b>", "lime");
            CommandRegistry[rawData[0]].Invoke();
        }    
        
        // Unity Update
        private void Update() {
            // Handle toggling via key
            if (Input.GetKeyDown(_toggleKey)) {
                _canvas.enabled = !_canvas.enabled;
            }
            
            // Handle command input
            if (Input.GetKeyDown(_commandKey)) {
                ParseCommand(_commandInput.text);
                _commandInput.text = "";
            }
            
            // Loop through the message queue and build the output string
            for (var i = 0; i < MessageQueue.Count; i++) {
                _stringBuilder.Append(MessageQueue.Dequeue());
            }
            _consoleText.text += _stringBuilder.ToString();
            _stringBuilder.Clear();
        }
    }
}