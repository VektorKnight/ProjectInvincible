using SteamNet;
using UnityEngine;
using UnityEngine.UI;
using VektorLibrary.Utility;

namespace InvincibleEngine.UI {
    /// <summary>
    /// Manages the economy display in the in-game UI.
    /// </summary>
    public class EconomyDisplay : MonoBehaviour {
        // Unity Inspector
        [Header("Required Components")]
        [SerializeField] private Text _resourcesText;
        [SerializeField] private Text _energyText;
        
        // Private: State
        private bool _initialized;
        
        // Initialization
        private void Start() {
            // Sanity check on component references
            var nullError = _resourcesText == null && _energyText == null;

            if (nullError) {
                Debug.LogError("EconomyDisplay: Improper component setup on {name}!\n" +
                                                      "UI object(s) will not be updated");
                return;
            }

            _initialized = true;
        }
        
        // Unity Update
        private void Update() {
            // Exit if not initialized
            if (!_initialized) return;
            
            // Grab economy values
            var resources = SteamNetManager.LocalPlayer.Economy.Resources;
            var energy = SteamNetManager.LocalPlayer.Economy.Energy;
            
            // Update UI display values
            _resourcesText.text = $"{resources:n0}";
            _energyText.text = $"{energy:n0}";
        }
    }
}