using UnityEngine;
using UnityEngine.UI;

namespace InvincibleEngine.HudSystem {
    /// <summary>
    /// Abstract base class for weapon HUD components.
    /// </summary>
    public abstract class WeaponHud {
        
        // Unity Inspector
        [Header("Basic Elements")] 
        [SerializeField] private Image[] _weaponReticles;
        [SerializeField] private Image[] _weaponGraphic;
            
    }
}