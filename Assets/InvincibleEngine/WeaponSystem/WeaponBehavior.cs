using InvincibleEngine.AudioSystem;
using InvincibleEngine.Managers;
using InvincibleEngine.UnitFramework.Components;
using UnityEngine;
using VektorLibrary.EntityFramework.Components;
using VektorLibrary.EntityFramework.Singletons;

namespace InvincibleEngine.WeaponSystem {
	/// <summary>
	/// Base class for all weapons used by units.
	/// Aim speed is in degrees/second
	/// </summary>
	public class WeaponBehavior : EntityBehavior {
		// Unity Inspector
		[Header("Weapon Settings")] 
		[SerializeField] protected float FireInterval = 1f;
		[SerializeField] protected Vector2 PitchRange = new Vector2();
		[SerializeField] protected float AimSpeed = 90f;

		[Header("Projectile Settings")] 
		[SerializeField] protected ProjectileBehavior Projectile;
		[SerializeField] protected float ProjectileVelocity;	// Ignored for linecast and raycast projectiles
		[SerializeField] protected float ProjectileDamage;
		[SerializeField] protected float ProjectileRange;		// Ignored for physical projectiles

		[Header("Weapon Aesthetics")] 
		[SerializeField] protected AudioClip FireSound;
		[SerializeField] protected ParticleSystem MuzzleFlash;

		[Header("Required Objects")] 
		[SerializeField] protected Transform Muzzle;
		
		// Protected: Required References
		protected UnitBehavior ParentUnit;
		
		// Protected: State
		protected bool CanFire = true;
		protected bool ContinuousFire;
		protected float FireTimer;
		
		// OnRegister Callback
		public override void OnRegister() {
			// Reference required components
			ParentUnit = GetComponentInParent<UnitBehavior>();
			
			// Call base method
			base.OnRegister();
		}
		
		// OnSimUpdate callback
		public override void OnSimUpdate(float fixedDelta, bool isHost) {
			// Update the fire timer if necessary
			if (!CanFire) {
				if (FireTimer <= 0)
					CanFire = true;
				else
					FireTimer -= fixedDelta;
			}
			
			// Control the aiming of the weapon
			if (ParentUnit.CurrentTarget != null) {
				// Calculate vector and rotation to target
				var targetVector = (ParentUnit.CurrentTarget.transform.position - Muzzle.position).normalized;
				var targetRotation = Quaternion.LookRotation(targetVector, transform.up);

				// Rotate weapon towards target
				transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, AimSpeed * fixedDelta);

				// Fire weapon if we are aimed at the target
				if (Vector3.Dot(targetVector, Muzzle.forward) > 0.999f)
					FireOnce();
			}
			else {
				// Calculate aim rotation aligned with unit forward vector
				//var targetRotation = Quaternion.LookRotation(ParentUnit.transform.forward, transform.up);
				
				// Slerp weapon rotation towards target vector
				//transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, AimSpeed * fixedDelta);
			}
			
			// Call base method
			base.OnSimUpdate(fixedDelta, isHost);
		}
		
		// Called to enable/disable the weapon
		public void SetEnabled(bool state) {
			
		}
		
		// Fires a projectile from this weapon's muzzle
		protected virtual void FireProjectile() {
			
		}

		// Called to try and fire the weapon immediately
		public bool FireOnce() {
			// Exit if we cannot fire
			if (!CanFire) return false;
			
			// Instantiate and initialize the projectile at the muzzle position
			var projectile = ObjectManager.GetObject(Projectile.gameObject, Muzzle.position, Muzzle.rotation);
			projectile.GetComponent<ProjectileBehavior>().Initialize(ProjectileVelocity, ProjectileDamage, ProjectileRange, ParentUnit.ScanLayers);
			
			// Try to play the fire sound effect
			if (FireSound != null)
				AudioManager.PlayClipAtPosition(transform.position, FireSound, 0.75f);
			
			// Play the muzzle flash particle system
			if (MuzzleFlash != null)
				MuzzleFlash.Play();
			
			// Reset the timer
			FireTimer = FireInterval;
			CanFire = false;
			
			// Return true if the weapon fired
			return true;
		}
		
		// Called to start/end continuous fire
		public void SetContinuousFire(bool state) {
			
		}
	}
}
