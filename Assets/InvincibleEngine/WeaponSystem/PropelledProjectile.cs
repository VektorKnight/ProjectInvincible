using UnityEngine;
using VektorLibrary.EntityFramework.Singletons;

namespace InvincibleEngine.WeaponSystem {
    public class PropelledProjectile : PhysicalProjectile {
        // Unity Inspector
        [Header("Rocket Engine Settings")] 
        [SerializeField] protected float Acceleration = 15f;
        [SerializeField] protected float MaxVelocity = 75f;
        [SerializeField] protected float SteerSpeed = 90f;
        [SerializeField] protected float TimeToTrack = 1f;

        [Header("Targeting")] 
        [SerializeField] protected bool SeekTarget = true;
        
        // Targeting
        protected Vector3 TargetPosition;
        
        // Initialization
        public override void Initialize(LayerMask collisionMask, Transform target = null) {   
            // Exit if a target isn't set
            if (target == null) {
                Debug.LogWarning("Propelled projectile inialized without a target!\n" +
                                 "Projectile will despawn immediately.");
                ReturnToPool();
                return;
            }
            
            TargetPosition = target.position;
            
            // Call base initializer
            base.Initialize(collisionMask, target);
        }
        
        // OnSimUpdate override
        public override void OnSimUpdate(float fixedDelta, bool isHost) {
           
            // Call base method
            base.OnSimUpdate(fixedDelta, isHost);
            
            // Update target position
            TargetPosition = Target != null ? Target.position : TargetPosition;
            
            // Calculate vector to target
            var targetVector = Vector3.Normalize(TargetPosition - transform.position);

            if (TimeAlive > TimeToTrack) {
                // Accelerate
                CurrentVelocity += transform.forward * Acceleration * fixedDelta;

                // Rotate to match current velocity vector
                var heading = Quaternion.LookRotation(targetVector, transform.up);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, heading, SteerSpeed * fixedDelta);
            }

            // Clamp velocity vector to maximum velocity value
            CurrentVelocity = Vector3.ClampMagnitude(CurrentVelocity, MaxVelocity);
        }
    }
}