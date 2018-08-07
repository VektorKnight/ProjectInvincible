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
    [RequireComponent(typeof(TrailRenderer))]
    public class PhysicalProjectile : ProjectileBehavior {            
        // Protected: Required Components
        protected TrailRenderer ProjectileTrail;
        
        // Private: Ballistics Simulation
        protected Vector3 PreviousPosition;
        protected RaycastHit HitInfo;
        
        // Initialization
        public override void OnRegister() {	
            ProjectileTrail = GetComponent<TrailRenderer>();
            base.OnRegister();
        }
        
        // Sim Update
        public override void OnSimUpdate(float fixedDelta, bool isHost) {
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
                
                // Attempt to reference UnitBehavior on collider, exit if null
                var unit = HitInfo.collider.GetComponent<UnitBehavior>();
                if (unit == null) {
                    ReturnToPool();
                    return;
                };
                
                // Set the projectile to the collision point and reset velocity
                transform.position = HitInfo.point;
                CurrentVelocity = Vector3.zero;
                
                // Apply damage to the unit
                unit.ApplyDamage(Damage);
                
                // Calculate where to instantiate the impact effect
                var pos = HitInfo.point - (transform.forward * 0.125f);
                var rot = Quaternion.FromToRotation(Vector3.forward, -transform.forward);
                ObjectManager.GetObject(ImpactEffect.gameObject, pos, rot);
                
                // Return to pool
                ReturnToPool();
            }
            // Set previous position
            PreviousPosition = transform.position;
            
            // Call base method
            base.OnSimUpdate(fixedDelta, isHost);
        }
        
        // Projectile initialization
        public override void Initialize (LayerMask collisionMask, Transform target = null) {
            // Reset the trail renderer state
            ProjectileTrail.Clear();
			
            // Update previous position (anti-clip)
            PreviousPosition = transform.position;
            
            // Call base method
            base.Initialize(collisionMask, target);
        }
        
        // Object Pool: Returned to pool
        public override void OnReturned() {
            // Reset the trail renderer state
            ProjectileTrail.Clear();
			
            // Call base method
            base.OnReturned();
        }
    }
}