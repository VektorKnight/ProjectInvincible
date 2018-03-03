using UnityEngine;

namespace InvincibleEngine.WeaponSystem.Components.Projectiles {
    public class SimpleRocket : PhysicalProjectile {
        
        // Unity Inspector
        [Header("Rocket Projectile Traits")] 
        [SerializeField] protected bool ExplodeOnTimeout = true; // Whether or not the rocket will explode when it runs out of life time
        [SerializeField] protected float ThrustVariance = 0.01f; // The amount of variance applied to the thrust vector to simulate instability
        [SerializeField] protected float EngineForce = 500f;     // The constant force applied by the simulated rocket engine
        [SerializeField] protected float MaxVelocity = 750.0f;   // The maximum velocity (m/s) that this rocket can reach
        
        // Private: Random Torque
        protected Vector3 RandomTorque;

        // Fixed Update
        protected override void FixedUpdate() {
            base.FixedUpdate();

            RandomTorque = Random.insideUnitSphere.normalized * ThrustVariance;
            ProjectileBody.velocity += RandomTorque;
        }
    }
}