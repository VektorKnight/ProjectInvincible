using InvincibleEngine.UnitFramework.Components;
using UnityEngine;
using UnityEngine.Profiling;
using VektorLibrary.EntityFramework.Singletons;
using _3rdParty.Steamworks.Plugins.Steamworks.NET.autogen;

namespace InvincibleEngine.WeaponSystem {
    /// <summary>
    /// Base class for physically simulated projectiles.
    /// Best used where realistic ballistics are desirable.
    /// </summary>
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(ParticleSystem))]
    public class PhysicalProjectile : ProjectileBehavior {            
        // Protected: Required Components
        protected MeshRenderer ProjectileRenderer;
        protected ParticleSystem[] ParticleEffects;
        
        // Private: Ballistics Simulation
        protected Vector3 PreviousPosition;
        protected RaycastHit HitInfo;
        protected bool IsDead;
        
        // Initialization
        public override void OnRegister() {	
            // Reference required components
            ProjectileRenderer = GetComponent<MeshRenderer>();
            ParticleEffects = GetComponentsInChildren<ParticleSystem>();
            base.OnRegister();
        }
        
        // Sim Update
        public override void OnSimUpdate(float fixedDelta, bool isHost) {
            // Exit if this projectile is dead (has hit something)
            if (IsDead) return;
            
            // Apply acceleration due to gravity
            CurrentVelocity += Vector3.up * GravityForce * fixedDelta;
            
            // Apply current velocity to position
            transform.position += CurrentVelocity * fixedDelta;
            
            // Calculate delta position for this tick
            var deltaPosition = transform.position - PreviousPosition; 
            var deltaMagnitude = deltaPosition.magnitude;

            // Check for collisions
            if (Physics.SphereCast(PreviousPosition, CollisionRadius, deltaPosition, out HitInfo, deltaMagnitude, CollisionMask)) {
                // Exit if the collider is somehow null or we hit a trigger volume
                if (HitInfo.collider == null || HitInfo.collider.isTrigger) return;
                
                // Attempt to reference a behavior implementing IDamageable
                var damageable = HitInfo.collider.GetComponent<IDamageable>();
                
                // Set the projectile to the collision point and reset velocity
                transform.position = HitInfo.point;
                CurrentVelocity = Vector3.zero;
                
                // Apply damage to the object
                damageable?.ApplyDamage(Damage);
                
                // Calculate where to instantiate the impact effect
                var pos = HitInfo.point - (transform.forward * 0.125f);
                var rot = Quaternion.FromToRotation(Vector3.forward, -transform.forward);
                ObjectManager.GetObject(ImpactEffect.gameObject, pos, rot);
                
                // Kill projectile on impact
                KillProjectile();
            }
            // Set previous position
            PreviousPosition = transform.position;
            
            // Call base method
            base.OnSimUpdate(fixedDelta, isHost);
        }
        
        // Projectile initialization
        public override void Initialize (LayerMask collisionMask, Transform target = null) {
            // Update previous position (anti-clip)
            PreviousPosition = transform.position;
            
            // Call base method
            base.Initialize(collisionMask, target);
        }
        
        // Object Pool: Retrieved from pool
        public override void OnRetrieved() {
            // Reset mesh renderer state and dead flag
            ProjectileRenderer.enabled = true;
            IsDead = false;
        }

        // Object Pool: Returned to pool
        public override void OnReturned() {		
            // Reset mesh renderer state
            ProjectileRenderer.enabled = true;
            transform.position = Vector3.zero;
            
            // Call base method
            base.OnReturned();
        }
        
        // Kills this projectile (called on impact)
        protected virtual void KillProjectile() {
            // Exit if already dead
            if (IsDead) return;
            
            // Move to world origin
            transform.position = Vector3.zero;
            
            // Disable mesh renderer and set despawn timer
            ProjectileRenderer.enabled = false;
            CancelInvoke();
            Invoke(nameof(ReturnToPool), ParticleEffects[0].main.duration);
            
            // Set dead flag
            IsDead = true;
        }
    }
}