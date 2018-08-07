using UnityEngine;
using UnityEngine.UI;

namespace InvincibleEngine.Utility {
    /// <summary>
    /// Controller for the development console.
    /// </summary>
    public class DevConsole : MonoBehaviour {
        // Singleton instance
        public static DevConsole Instance { get; private set; }

        // Unity Inspector
        [Header("Input Config")] 
        [SerializeField] private KeyCode _toggleKey = KeyCode.BackQuote;
        
        [Header("Required Objects")] 
        [SerializeField] private Text _consoleText;
        [SerializeField] private InputField _commandInput;
        
        // Required References
        private Canvas _canvas;
        
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
            Instance._canvas = Instance.GetComponentInParent<Canvas>();
            
            // Enable console by default in development builds
            _canvas.enabled = Debug.isDebugBuild;
        }
       
        // Log a message to the console
        public static void Log(string caller, string message, string nameColor = "lightblue") {
            Instance._consoleText.text += $"<color={nameColor}>[{caller}]</color> {message}\n";
        }
        
        // Log a warning message to the console
        public static void LogWarning(string caller, string message) {
            Instance._consoleText.text += $"<color=#f9ba1bff>[{caller}]</color> {message}\n";
        }
        
        // Log an error message to the console
        public static void LogError(string caller, string message) {
            Instance._consoleText.text += $"<color=#ff6666ff>[{caller}]</color> {message}\n";
        }
        
        // Unity Update
        private void Update() {
            if (Input.GetKeyDown(_toggleKey)) {
                _canvas.enabled = !_canvas.enabled;
            }
        }
    }
}