using UnityEngine;

namespace InvincibleEngine.WeaponSystem {
    /// <summary>
    /// Utility class for calculating various compensation and targeting values for ballistic weapons.
    /// </summary>
    public static class Ballistics {
        /// <summary>
        /// Calculates an aim vector for physical projectiles affected by gravity.
        /// </summary>
        public static Vector3 AimVectorArc(Vector3 muzzle, Vector3 target, float velocity, float gravity) {
            // Calculate expected projectile drop based on given values
            var targetDistance = Vector3.Distance(muzzle, target);
            var travelTime = targetDistance / velocity;
            var drop = 0.5f * gravity * travelTime * travelTime;
            
            // Subtract the expected drop to the target's [y] component
            target.y -= drop;
            
            // Calculate the new aim vector accounting for drop
            return Vector3.Normalize(target - muzzle);
        }
    }
}