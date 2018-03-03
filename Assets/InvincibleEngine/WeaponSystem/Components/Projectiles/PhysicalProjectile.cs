using InvincibleEngine.WeaponSystem.Interfaces;
using UnityEngine;

namespace InvincibleEngine.WeaponSystem.Components.Projectiles {
	/// <summary>
	/// Base class for physically simulated projectiles.
	/// Best used where realistic ballistics are desirable.
	/// **
	/// Portions of anti-clip code based on original work by
	/// Daniel Brauer.
	/// **
	/// </summary>
	[RequireComponent(typeof(Rigidbody))]
	[RequireComponent(typeof(SphereCollider))]
	[RequireComponent(typeof(TrailRenderer))]
	[AddComponentMenu("Weapon System/Projectiles/Physical Projectile")]
	public class PhysicalProjectile : Projectile {
		
		// Unity Inspector	
		[Header("Anti-Clip Config")] 
		[SerializeField] protected bool EnableAntiClip = true;
		[SerializeField] protected LayerMask LayerMask = -1; 
		[SerializeField] protected float SkinWidth = 0.1f; 
		
		// Private: Anti Clipping
		protected float MinimumExtent;
		protected float PartialExtent;
		protected float SqrMinimumExtent;
		protected Vector3 PreviousPosition; 
		
		// Protected: Components
		protected Collider ProjectileCollider;
		protected Rigidbody ProjectileBody;
		protected TrailRenderer ProjectileTrail;
		
		// Initialization
		protected override void Awake() {	
			ProjectileCollider = GetComponent<Collider>();
			ProjectileBody = GetComponent<Rigidbody>();
			ProjectileTrail = GetComponent<TrailRenderer>();
			
			MinimumExtent = Mathf.Min(Mathf.Min(ProjectileCollider.bounds.extents.x, ProjectileCollider.bounds.extents.y), ProjectileCollider.bounds.extents.z); 
			PartialExtent = MinimumExtent * (1.0f - SkinWidth); 
			SqrMinimumExtent = MinimumExtent * MinimumExtent;
			
			base.Awake();
		}

		// Object Pool: Taken from pool
		protected override void OnEnable () {
			// Reset the trail renderer state
			ProjectileTrail.Clear();
			
			// Update previous position (anti-clip)
			PreviousPosition = ProjectileBody.position;
			
			// Set new velocity
			ProjectileBody.velocity = InitialVelocity * transform.forward;

			base.OnEnable();
		}
		
		// Object Pool: Returned to pool
		protected override void ReturnToPool() {
			// Reset the trail renderer state
			ProjectileTrail.Clear();
			
			// Reset rigidbody
			ProjectileBody.velocity = Vector3.zero;
			
			base.ReturnToPool();
		}

		// Physics Update
		protected virtual void FixedUpdate() { 
			// Exit if anti-clip is disabled
			if (!EnableAntiClip) return;
			
			// Calculate the delta position for this frame
			var deltaPosition = ProjectileBody.position - PreviousPosition; 
			var sqrDeltaPosition = deltaPosition.sqrMagnitude;
 			
			// Check if the delta is greater than the minimum extent
			if (sqrDeltaPosition > SqrMinimumExtent) { 
				var movementMagnitude = Mathf.Sqrt(sqrDeltaPosition);
				RaycastHit hitInfo; 
 
				// Check for missed collisions
				if (Physics.Raycast(PreviousPosition, deltaPosition, out hitInfo, movementMagnitude, LayerMask.value)) {
					if (!hitInfo.collider) return;
 
					if (hitInfo.collider.isTrigger) 
						hitInfo.collider.SendMessage("OnTriggerEnter", ProjectileCollider);
 
					if (!hitInfo.collider.isTrigger)
						ProjectileBody.MovePosition(hitInfo.point - (deltaPosition / movementMagnitude) * PartialExtent);
						ProjectileBody.velocity = Vector3.zero;
				}
			} 
 			
			// Update the previous position for the next frame
			PreviousPosition = ProjectileBody.position; 
		}
		
		// Trigger Callback
		protected virtual void OnTriggerEnter(Collider other) {
			// Calculate and apply impact force and damage
			var impactForce = (ProjectileBody.mass * InitialVelocity * transform.forward);
			var impactPoint = other.gameObject.GetComponent<Collider>().ClosestPointOnBounds(transform.position);
			var otherBody = other.GetComponent<Rigidbody>();
			
			if (otherBody != null) {
				otherBody.AddForceAtPosition(impactForce, impactPoint, ForceMode.Impulse);
				var otherObject = otherBody.GetComponent<IDestructable>();
				otherObject?.ApplyDamage(Damage, OwnerId);
			}
			
			// Calculate where to instantiate the impact effect and destroy this object
			var pos = transform.TransformPoint(Vector3.forward * -0.125f);
			var rot = Quaternion.FromToRotation(Vector3.forward, -ProjectileBody.transform.forward);
			SpawnImpactEffect(pos, rot);
			
			// Despawn the projectile (return to pool)
			ReturnToPool();
		}
	}
}
