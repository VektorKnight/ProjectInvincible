using InvincibleEngine.WeaponSystem.Interfaces;
using UnityEngine;

namespace InvincibleEngine.WeaponSystem.Components.Projectiles {
	/// <summary>
	/// Base class for hitscan/raycast based projectiles.
	/// Best used where realistic ballistics are low priority.
	/// Raycast projectile can also pass through multiple objects.
	/// </summary>
	[RequireComponent(typeof(LineRenderer))]
	[AddComponentMenu("Weapon System/Projectiles/Raycast")]
	public class RaycastProjectile : Projectile {
		
		// Unity Inspector
		[Header("Object Collision Traits")] 
		[SerializeField] protected float SphereCastRadius = 0.125f;
		[SerializeField] protected LayerMask CollisionMask;

		[Header("Object Penetration Config")] 
		[SerializeField] protected bool ObjectPenetration = false;
		[SerializeField] protected int MaxObjects = 3;
		[SerializeField] protected float FalloffProportion = 0.66f;
		
		[Header("Projectile Aesthetics")]
		[SerializeField] protected Gradient ProjectileGradient;
		
		// Protected: Projectile Aesthetics
		protected LineRenderer ProjectileRenderer;
		protected Material ProjectileMaterial;
		protected float ProjectileTime = 0f;
		
		// Initialization
		protected override void Awake() {
			ProjectileRenderer = GetComponent<LineRenderer>();
			ProjectileMaterial = ProjectileRenderer.material;
			ProjectileMaterial.SetColor("_DiffuseColor", ProjectileGradient.Evaluate(0f));
			
			base.Awake();
		}

		// Called when taken from object pool
		protected override void OnEnable () {
			// Create the projectile raycast	
			CreateRaycast();
			
			base.OnEnable();
		}
	
		// Per-Frame Update
		protected virtual void Update () {
			// Decay the projectile material
			if (ProjectileTime > 0f) {
				ProjectileTime -= Time.deltaTime;
			}
			
			// Set the projectile material color
			ProjectileMaterial.SetColor("_DiffuseColor", ProjectileGradient.Evaluate(ProjectileTime/ProjectileLife));
		}
		
		// Raycast for collisions (simulation)
		protected virtual void CreateRaycast() {
			// Set up for raycasting
			var maxDistance = InitialVelocity * ProjectileLife;
			var projectileRay = new Ray(transform.position, transform.forward);
			
			// Raycast for collisions
			RaycastHit rayHit;
			if (Physics.SphereCast(projectileRay, SphereCastRadius, out rayHit,maxDistance, CollisionMask)) {
				// Set up projectile aesthetics
				ProjectileTime = ProjectileLife;
				ProjectileMaterial.SetColor("_DiffuseColor", ProjectileGradient.Evaluate(1.0f));
				ProjectileRenderer.SetPosition(0, transform.position);
				ProjectileRenderer.SetPosition(1, rayHit.point);

				// Calculate and apply impact force and damage if applicable
				var impactForce = Mass * InitialVelocity * transform.forward;
				var otherBody = rayHit.collider.gameObject.GetComponent<Rigidbody>();
				if (otherBody != null) {
					otherBody.AddForceAtPosition(impactForce, rayHit.point, ForceMode.Impulse);
					var otherObject = otherBody.GetComponent<IDestructable>();
					otherObject?.ApplyDamage(Damage, OwnerId);
				}

				// Calculate where to instantiate the impact effect
				var pos = rayHit.point - (transform.forward * 0.125f);
				var rot = Quaternion.FromToRotation(Vector3.forward, -transform.forward);
				SpawnImpactEffect(pos, rot);
			}
			else {
				// Set up projectile aesthetics
				ProjectileTime = ProjectileLife;
				ProjectileMaterial.SetColor("_DiffuseColor", ProjectileGradient.Evaluate(1.0f));
				ProjectileRenderer.SetPosition(0, transform.position);
				ProjectileRenderer.SetPosition(1, transform.position + transform.forward * maxDistance);
			}
		}
	}
}
