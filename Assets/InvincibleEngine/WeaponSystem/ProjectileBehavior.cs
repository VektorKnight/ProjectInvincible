using InvincibleEngine.Components.Generic;
using UnityEngine;
using VektorLibrary.EntityFramework.Components;

namespace InvincibleEngine.WeaponSystem {
    /// <summary>
    /// Base class for all projectiles used by unit weapons.
    /// </summary>
    public class ProjectileBehavior : PooledBehavior {
        // Unity Inspector
        [Header("Basic Projectile Config")] 
        [SerializeField] protected ParticleSystem ImpactEffect;
        
        // Private: State
        private bool _initialized;
		
        // Protected: Pooled Object
        protected PooledBehavior PooledObject;
        
        // Protected: Config
        protected LayerMask CollisionMask;
        protected float Velocity;
        protected float Gravity;
        protected float Damage; 
        protected float Range;
        
        // Initialization
        public override void OnRegister() {
            // Reference required components
            PooledObject = GetComponent<PooledBehavior>();
            
            // Call base method
            base.OnRegister();
        }
        
        // Cancel the base behavior of this method
        // The despawn time must be calculated for projectiles
        public override void OnRetrieved() { }

        // Called by a weapon to set the config values for this projectile
        public virtual void Initialize(float velocity, float gravity, float damage, float range, LayerMask collisionMask) {
            CollisionMask = collisionMask;
            Velocity = velocity;
            Gravity = gravity;
            Damage = damage;
            Range = range;
            
            // Calculate despawn time based on range
            DespawnTime = Range / Velocity;
            
            // Invoke return to pool function
            if (AutoDespawn) 
                Invoke(nameof(ReturnToPool), DespawnTime);
        }
    }
}