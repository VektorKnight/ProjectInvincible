using InvincibleEngine.Components.Generic;
using UnityEngine;
using VektorLibrary.EntityFramework.Components;

namespace InvincibleEngine.WeaponSystem {
    /// <summary>
    /// Base class for all projectiles used by unit weapons.
    /// </summary>
    public abstract class ProjectileBehavior : PooledBehavior {
        // Unity Inspector
        [Header("Projectile Physics")] 
        [SerializeField] protected float CollisionRadius = 0.25f;
        [SerializeField] protected float InitialVelocity = 50f;
        [SerializeField] protected float GravityScale = 1.0f;
        [SerializeField] protected float MaxRange = 500f;
        [SerializeField] protected float Damage = 10f;
        
        [Header("Projectile Aesthetics")]
        [SerializeField] protected ParticleSystem ImpactEffect;
        
        // Private: State
        private bool _initialized;
		
        // Protected: Pooled Object
        protected PooledBehavior PooledObject;
        
        // Protected: Config
        protected LayerMask CollisionMask;
        protected Vector3 CurrentVelocity;
        protected float GravityForce;
        
        // Protected: Targeting
        protected Transform Target;
        
        // Properties: Physics
        public float Velocity => InitialVelocity;
        public float Gravity => GravityScale * -9.81f;
        public float Range => MaxRange;
        
        // Initialization
        public override void OnRegister() {
            // Reference required components
            PooledObject = GetComponent<PooledBehavior>();
            
            // Calculate force of gravity
            GravityForce = GravityScale * -9.81f;
            
            // Call base method
            base.OnRegister();
        }
        
        // Called by a weapon to set the config values for this projectile
        public virtual void Initialize(LayerMask collisionMask, Transform target = null) {
            // Set config values
            CollisionMask = collisionMask | (int)Mathf.Pow(2, 8);
            Target = target;
            
            // Set velocity vector
            CurrentVelocity = transform.forward * InitialVelocity;
            
            // Calculate despawn time based on range
            DespawnTime = MaxRange / InitialVelocity;
            
            // Invoke return to pool function
            if (AutoDespawn) 
                Invoke(nameof(ReturnToPool), DespawnTime);
        }
        
        // Cancel the base behavior of this method
        // The despawn time must be calculated for projectiles
        public override void OnRetrieved() { }
        
        // Onreturned override
        public override void OnReturned() {
            // Reset velocity and target
            Target = null;
            CurrentVelocity = Vector3.zero;
            
            // Call base method
            base.OnReturned();
        }
    }
}