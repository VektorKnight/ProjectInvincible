﻿using InvincibleEngine.Managers;
using InvincibleEngine.UnitFramework.Components;
using UnityEngine;
using VektorLibrary.EntityFramework.Components;
using VektorLibrary.EntityFramework.Singletons;

namespace InvincibleEngine.WeaponSystem {
	/// <summary>
	/// Base class for all weapons used by units.
	/// </summary>
	public class WeaponBehavior : EntityBehavior {
		// Unity Inspector
		[Header("Weapon Settings")]
		[SerializeField] protected float FireInterval;
		[SerializeField] protected Vector2 PitchRange;
		[SerializeField] protected float AimSpeed;

		[Header("Projectile Settings")] 
		[SerializeField] protected ProjectileBehavior Projectile;
		[SerializeField] protected float ProjectileVelocity;	// Ignored for linecast and raycast projectiles
		[SerializeField] protected float ProjectileDamage;
		[SerializeField] protected float ProjectileRange;		// Ignored for physical projectiles

		[Header("Required Objects")] 
		[SerializeField] protected Transform Muzzle;
		[SerializeField] protected ParticleSystem MuzzleFlash;
		
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
			
			// Call base method
			base.OnSimUpdate(fixedDelta, isHost);
		}
		
		// Render update callback
		public override void OnRenderUpdate(float renderDelta) {
			if (ParentUnit.CurrentTarget != null) {
				transform.LookAt(ParentUnit.CurrentTarget.transform);
				FireOnce();
			}
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