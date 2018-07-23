using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace VektorLibrary.Utility {
    /// <summary>
    /// Utility class for drawing various debug readouts to the game screen.
    /// </summary>
    public class DebugReadout : MonoBehaviour {
        
        // Singleton Instance & Accessor
        private static DebugReadout _singleton;
        public static DebugReadout Instance => _singleton ?? new GameObject("DebugReadout").AddComponent<DebugReadout>();
        
        // Unity Inspector
        [Header("Debug Readout Config")] 
        [SerializeField] private KeyCode _toggleKey = KeyCode.F2;
        [SerializeField] private Text _debugText;
        
        // Private: Debug Fields
        private readonly Dictionary<string, string> _debugFields = new Dictionary<string, string>();
        
        // Private: FPS Counter
        private FpsCounter _fpsCounter;
        
        // Private: State
        private bool _enabled;
        
        // Property: State
        public static bool Enabled => Instance._enabled;

        // Initialization
        private void Awake() {
            // Enforce Singleton Instance
            if (_singleton == null) { _singleton = this; }
            else if (_singleton != this) { Destroy(gameObject); }
            
            // Ensure this object is not destroyed on scene load
            DontDestroyOnLoad(gameObject);
            
            // Display the readout by default if dev build or editor
            if (Debug.isDebugBuild) _enabled = true;
            
            // Set up some default readouts
            AddField("Vektor Games");
            var version = Debug.isDebugBuild ? "DEVELOPMENT" : "DEPLOY";
            UpdateField("Vektor Games", $"Project Invincible [{version}]");
            
            AddField("Display");
            UpdateField("Display", $"{Screen.width}x{Screen.height}");
            
            AddField("FPS");
            _fpsCounter = new FpsCounter();
            
        }
        
        // Toggle display the of readout
        public static void ToggleReadout() {
            if (Instance._enabled) {
                Instance._debugText.enabled = false;
                Instance._enabled = false;
            }
            else {
                Instance._debugText.enabled = true;
                Instance._enabled = true;
            }
        }
        
        // Toggle the display of the readout based on a bool
        public static void ToggleReadout(bool state) {
            if (state) {
                Instance._debugText.enabled = true;
                Instance._enabled = true;
            }
            else {
                Instance._debugText.enabled = false;
                Instance._enabled = false;
            }
        }
        
        // Add a debug field to the readout
        public static void AddField(string key) {
            // Exit if the specified key already exists
            if (Instance._debugFields.ContainsKey(key)) return;
            
            // Add the key to the dictionary with the given value
            Instance._debugFields.Add(key, "null");
        }
        
        // Remove a debug field from the readout
        public static void RemoveField(string key) {
            // Exit if the specified key does not exist
            if (!Instance._debugFields.ContainsKey(key)) return;
            
            // Remove the key from the dictionary
            Instance._debugFields.Remove(key);
        }
        
        // Update an existing debug field
        public static void UpdateField(string key, string value) {
            // Create a new field if the specified field doesn't exist
            if (!Instance._debugFields.ContainsKey(key))
                Instance._debugFields.Add(key, value);
            
            // Update the specified field with the new value
            Instance._debugFields[key] = value;
        }
        
        // Unity Update
        private void Update() {
            // Check for key press
            if (Input.GetKeyDown(Instance._toggleKey)) ToggleReadout();
            
            // Exit if the readout is disabled
            if (!_enabled) return;
            
            // Update FPS Counter
            UpdateField("FPS", $"{Instance._fpsCounter.UpdateValues()} (Δ{Time.deltaTime * 1000f:n1}ms)");
            
            // Iterate through the debug fields and add them to the readout
            var displayText = new StringBuilder();
            foreach (var field in Instance._debugFields) {
                displayText.Append($"{field.Key}: {field.Value}\n");
            }
            
            // Set the readout text
            Instance._debugText.text = displayText.ToString();
        }
    }
}