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
		[SerializeField] protected float WeaponRange;
		[SerializeField] protected bool AutoAimWeapon = true;
		[SerializeField] protected bool WaitForAlignment = true;

		[Header("Projectile Settings")] 
		[SerializeField] protected ProjectileBehavior Projectile;

		[Header("Weapon Aesthetics")] 
		[SerializeField] protected AudioClip FireSound;
		[SerializeField] protected ParticleSystem MuzzleFlash;

		[Header("Required Objects")] 
		[SerializeField] protected Transform Muzzle;
		
		// Protected: Required References
		protected Renderer WeaponRenderer;
		protected UnitBehavior Parent;
		
		// Protected: State
		protected bool CanFire = true;
		protected bool ContinuousFire;
		protected float FireTimer;
		
		// Protected: Range Checking
		protected float SqrWeaponRange;
		
		// Called by parent UnitBehavior to initialize this weapon
		public virtual void Initialize(UnitBehavior parent) {
			// Ensure this object has been registered
			if (!Registered) Start();
			
			// Reference mesh renderer
			WeaponRenderer = GetComponentInChildren<MeshRenderer>();
			
			// Assign parent reference
			Parent = parent;
			
			// Assign team and glow material properties
			if (WeaponRenderer != null) {
				WeaponRenderer.material.SetColor("_TeamColor", Parent.UnitColor);
				WeaponRenderer.material.SetColor("_EmissionColor", Parent.UnitColor);
			}

			// Calculate optimized range check value
			SqrWeaponRange = WeaponRange * WeaponRange;
		}
		
		// Sim update callback
		public override void OnSimUpdate(float fixedDelta, bool isHost) {
			// Update the fire timer if necessary
			if (!CanFire) {
				if (FireTimer <= 0)
					CanFire = true;
				else
					FireTimer -= fixedDelta;
			}
			
			// Control the aiming of the weapon
			if (Parent.CurrentTarget != null) {
				// Calculate vector and rotation to target
				var targetVector = Ballistics.AimVectorArc(Muzzle.position, Parent.CurrentTarget.transform.position, Projectile.Velocity, Projectile.Gravity);
				var targetRotation = Quaternion.LookRotation(targetVector, transform.up);

				// Rotate weapon towards target
				if (AutoAimWeapon)
					transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, AimSpeed * fixedDelta);

				// Fire weapon if we are aimed at the target and the target is within range
				if (WaitForAlignment) {
					var sqrTargetDistance = Vector3.SqrMagnitude(Parent.CurrentTarget.transform.position - Muzzle.position);
					var aimDotP = Vector3.Dot(targetVector, Muzzle.forward);
					if (aimDotP > 0.999f && sqrTargetDistance <= SqrWeaponRange) TryFire();
				}
				else {
					TryFire();
				}
			}
			else {
				// Calculate aim rotation aligned with unit forward vector
				var targetRotation = Quaternion.LookRotation(Parent.transform.forward, transform.up);
				
				// Slerp weapon rotation towards target vector
				transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, (AimSpeed / 4f) * fixedDelta);
			}
			
			// Call base method
			base.OnSimUpdate(fixedDelta, isHost);
		}
		
		// Called to enable/disable the weapon
		public virtual void SetEnabled(bool state) {
			// TODO: Not yet implemented
		}
		
		// Fires a projectile from this weapon's muzzle
		protected virtual void FireProjectile() {
			// Instantiate and initialize the projectile at the muzzle position
			var projectile = ObjectManager.GetObject(Projectile.gameObject, Muzzle.position, Muzzle.rotation);
			projectile.GetComponent<ProjectileBehavior>().Initialize(Parent.ScanLayers, Parent.CurrentTarget.transform);
			
			// Try to play the fire sound effect
			if (FireSound != null)
				AudioManager.PlayClipAtPosition(transform.position, FireSound, 0.75f);
			
			// Play the muzzle flash particle system
			if (MuzzleFlash != null)
				MuzzleFlash.Play();
		}

		// Called to try and fire the weapon immediately
		public virtual bool TryFire() {
			// Exit if we cannot fire
			if (!CanFire) return false;
			
			// Fire a projectile
			FireProjectile();
			
			// Reset the timer
			FireTimer = FireInterval;
			CanFire = false;
			
			// Return true if the weapon fired
			return true;
		}
		
		// Called to start/end continuous fire
		public virtual void SetContinuousFire(bool state) {
			// TODO: Not yet implemented, fight me
		}
	}
}
