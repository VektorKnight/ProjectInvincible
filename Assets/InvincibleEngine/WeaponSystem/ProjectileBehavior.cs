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
        protected float Damage; 
        protected float Range;
        
        // Initialization
        public override void OnRegister() {
            // Reference required components
            PooledObject = GetComponent<PooledBehavior>();
            
            // Call base method
            base.OnRegister();
        }
        
        // Sets this projectile's config
        public virtual void Initialize(float velocity, float damage, float range, LayerMask collisionMask) {
            CollisionMask = collisionMask;
            Velocity = velocity;
            Damage = damage;
            Range = range;
        }
    }
}