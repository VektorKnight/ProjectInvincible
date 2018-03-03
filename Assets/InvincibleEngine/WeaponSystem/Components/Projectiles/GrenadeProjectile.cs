using System.Collections.Generic;
using InvincibleEngine.WeaponSystem.Interfaces;
using UnityEngine;

namespace InvincibleEngine.WeaponSystem.Components.Projectiles {
    /// <summary>
    /// Specialized class for bouncing grenade projectiles
    /// </summary>
    [AddComponentMenu("Weapon System/Projectiles/Grenade Projectile")]
    public class GrenadeProjectile : PhysicalProjectile {

        [Header("Grenade Projectile Traits")] 
        [SerializeField] protected LayerMask ExplosionMask; 
        [SerializeField] protected float ExplosionRadius = 10.0f;
        [SerializeField] protected float ExplosionForce = 40000f;
        [SerializeField] protected float MinBounceAngle = 100f;
        [SerializeField] protected float PrimedFuseTime = 1.0f;
        [SerializeField] protected AnimationCurve FalloffCurve;
        
        // Private: Grenade Fuse
        protected bool IsPrimed;
        protected bool IsExploding;
        
        // OnEnable Override
        protected override void OnEnable() {
            // Reset the IsExploding flag
            IsExploding = false;
            
            // Call base method
            base.OnEnable();
        }

        // Physics Update Override
        protected override void FixedUpdate() {
            // Modified anti-clip code
            if (!EnableAntiClip) return;
            var movementThisStep = ProjectileBody.position - PreviousPosition; 
            var movementSqrMagnitude = movementThisStep.sqrMagnitude;
 
            if (movementSqrMagnitude > SqrMinimumExtent) { 
                var movementMagnitude = Mathf.Sqrt(movementSqrMagnitude);
                RaycastHit hitInfo; 
 
                // check for obstructions we might have missed 
                if (Physics.Raycast(PreviousPosition, movementThisStep, out hitInfo, movementMagnitude, LayerMask.value)) {
                    if (!hitInfo.collider)
                        return;
 
                    if (hitInfo.collider.isTrigger) 
                        hitInfo.collider.SendMessage("OnTriggerEnter", ProjectileCollider);
 
                    if (!hitInfo.collider.isTrigger)
                        ProjectileBody.MovePosition(hitInfo.point - (movementThisStep / movementMagnitude) * PartialExtent);
                }
            } 
            PreviousPosition = ProjectileBody.position;
        }
        
        // Trigger Callback Override
        protected override void OnTriggerEnter(Collider other) {
            return;
        }

        // Collision Callback
        protected virtual void OnCollisionEnter(Collision collision) {
            // Calculate the impact angle
            var normal = collision.contacts[0].normal;
            var angle = Vector3.Angle(normal, ProjectileBody.velocity) * 2f;
            
            // Explode if direct hit on player
            if (collision.gameObject.CompareTag("Player")) {
                CreateExplosion();
                return;
            }
            
            // Explode if the angle is less than the minimum bounce angle
            if (angle < MinBounceAngle) {
                CreateExplosion();
                return;
            }
            
            // Allow the grenade to bounce and prime the grenade
            if (!IsPrimed) {
                Invoke(nameof(CreateExplosion), PrimedFuseTime);
                IsPrimed = true;
                return;
            }
            
            // Grenade will explode if already primed
            CreateExplosion();
        }
        
        // Explode Function
        protected virtual void CreateExplosion() {
            // Exit if already exploding, set the flag if not
            if (IsExploding) return;
            IsExploding = true;
            
            // Find all valid objects within the explosion radius
            var rawObjects = Physics.OverlapSphere(transform.position, ExplosionRadius, ExplosionMask);
            var sortedObjects = new List<GameObject>();
            
            // Sort out any duplicate entries (compound colliders)
            foreach (var obj in rawObjects) {
                // Check for duplicate entry
                var gameObj = obj.gameObject;
                if (sortedObjects.Contains(gameObj)) continue;
                
                // Add the object to the list
                sortedObjects.Add(gameObj);
            }
            
            // Apply an explosion force and attempt to apply damage
            foreach (var obj in sortedObjects) {
                // Grab the necessary component references
                var objBody = obj.GetComponent<Rigidbody>();
                var objDest = obj.GetComponent<IDestructable>();
                
                // Apply an explosion force if objBody != null
                if (objBody == null) continue;
                objBody.AddExplosionForce(ExplosionForce, transform.position, ExplosionRadius);
                
                // Apply proportional damage if objDest != null
                if (objDest == null) continue;
                var distance = Vector3.Distance(transform.position, obj.transform.position);
                var damage = FalloffCurve.Evaluate(1f - (distance / ExplosionRadius)) * Damage;
                objDest.ApplyDamage(damage, OwnerId);
            }
            
            // Calculate where to instantiate the impact effect and destroy this object
            var pos = transform.TransformPoint(Vector3.forward * -0.125f);
            var rot = Quaternion.FromToRotation(Vector3.forward, -ProjectileBody.transform.forward);
            SpawnImpactEffect(pos, rot);
            
            // Return to pool
            CancelInvoke();
            IsPrimed = false;
            ReturnToPool();
        }
    }
}