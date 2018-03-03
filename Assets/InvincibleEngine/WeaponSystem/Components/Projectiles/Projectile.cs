using InvincibleEngine.Components.Generic;
using InvincibleEngine.Managers;
using UnityEngine;

namespace InvincibleEngine.WeaponSystem.Components.Projectiles {
	/// <summary>
	/// Abstract base class for all projectile types.
	/// </summary>
	[RequireComponent(typeof(PooledObject))]
	public abstract class Projectile : MonoBehaviour {
		
		// Unity Inspector
		[Header("Basic Projectile Config")] 
		[SerializeField] protected float ProjectileLife = 5.0f;
		[SerializeField] protected ParticleSystem ImpactEffect;

		[Header("Projectile Traits")] 
		[SerializeField] protected float Damage;
		[SerializeField] protected float Mass = 0.125f;
		[SerializeField] protected float InitialVelocity = 0.0f;
		
		// Private: State
		protected bool Initialized;
		
		// Protected: Pooled Object
		protected PooledObject PooledObject;
		
		// Public Properties: Traits
		public ulong OwnerId { get; private set; }
		
		// Initialization: Awake
		protected virtual void Awake() {
			PooledObject = GetComponent<PooledObject>();
		}

		// Initialization: Object Pool (Get)
		protected virtual void OnEnable () {
			// Make sure ImpactEffect is not null and warn the user if it is
			if (ImpactEffect == null) {
				Debug.LogWarning($"Impact effect for projectile {name} has not been set up!\n" +
				                 "Please correct this through the prefab inspector.");
			}
			
			// Set the projectile to despawn on time out
			Invoke(nameof(ReturnToPool), ProjectileLife);
		}
		
		// Initialization: Traits
		public virtual void Initialize(ulong ownerId) {
			// Exit if already initialized
			if (Initialized) return;
			
			OwnerId = ownerId;
			
			// Initialization complete
			Initialized = true;
		}
		
		// Spawn the Impact Effect
		protected virtual void SpawnImpactEffect(Vector3 position, Quaternion rotation) {
			if (ImpactEffect == null) return;

			GlobalObjectManager.GetObject(ImpactEffect.gameObject, position, rotation);
		}
		
		// Despawn the projectile instance
		protected virtual void ReturnToPool() {
			// Cancel the automatic invocation if method already called
			CancelInvoke();
			
			// Reset initialization flag
			Initialized = false;
			
			// Call the despawn method
			PooledObject.Despawn();
		}
	}
}
