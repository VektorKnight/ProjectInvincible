using UnityEngine;

namespace InvincibleEngine.WeaponSystem.Components.Projectiles {
    /// <summary>
    /// Base class for mathematically simulated projectile.
    /// Simplified formulas are used in place of the physics engine.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class SimulatedProjectile : Projectile {
        
    }
}