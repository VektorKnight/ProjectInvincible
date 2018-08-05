using InvincibleEngine.UnitFramework.Components;
using UnityEngine;
using VektorLibrary.EntityFramework.Singletons;

namespace InvincibleEngine.WeaponSystem {
    /// <summary>
    /// Base class for fake projectiles using raycasts.
    /// </summary>
    [RequireComponent(typeof(LineRenderer))]
    public class RaycastProjectile : ProjectileBehavior {
        // Unity Inspector
        [Header("Projectile Aesthetics")]
        [SerializeField] protected Gradient ProjectileGradient;
        [SerializeField] protected float ProjectileWidth;
        
        // Protected: Projectile Aesthetics
        protected LineRenderer ProjectileRenderer;
        protected Material ProjectileMaterial;
        protected float ProjectileTimer;
        
        // Initialization
        public override void OnRegister() {
            // Reference required components
            ProjectileRenderer = GetComponent<LineRenderer>();
            ProjectileRenderer.startWidth = ProjectileWidth;
            ProjectileRenderer.endWidth = ProjectileWidth;
            ProjectileMaterial = ProjectileRenderer.material;
            ProjectileMaterial.SetColor("_Color", ProjectileGradient.Evaluate(0f));
            
            // Call base method
            base.OnRegister();
        }
        
        // Initialize the projectile
        public override void Initialize(float velocity, float gravity, float damage, float range, LayerMask collisionMask) {
            // Call base method
            base.Initialize(velocity, gravity, damage, range, collisionMask);
            
            // Create raycast
            CreateRaycast();
        }
        
        // Per-Frame Update
        public override void OnRenderUpdate (float renderDelta) {
            // Decay the projectile material
            if (ProjectileTimer > 0f) {
                ProjectileTimer -= renderDelta;
            }

            // Set the projectile material color
            var ratio = DespawnTime < 1f ? (100f * ProjectileTimer) / (100f * DespawnTime) : ProjectileTimer / DespawnTime;
            ProjectileMaterial.SetColor("_DiffuseColor", ProjectileGradient.Evaluate(ratio));
            base.OnRenderUpdate(renderDelta);
        }
        
        // Raycast for collisions (simulation)
        protected virtual void CreateRaycast() {
            // Set up for raycasting
            var projectileRay = new Ray(transform.position, transform.forward);
			
            // Raycast for collisions
            RaycastHit rayHit;
            if (Physics.Raycast(projectileRay, out rayHit, Range, CollisionMask)) {
                // Set up projectile aesthetics
                ProjectileTimer = DespawnTime;
                ProjectileMaterial.SetColor("_DiffuseColor", ProjectileGradient.Evaluate(1.0f));
                ProjectileRenderer.SetPosition(0, transform.position);
                ProjectileRenderer.SetPosition(1, rayHit.point);

                // Calculate and apply impact force and damage if applicable
                var unitHit = rayHit.collider.gameObject.GetComponent<UnitBehavior>();
                if (unitHit != null)
                    unitHit.ApplyDamage(Damage);

                // Calculate where to instantiate the impact effect
                var pos = rayHit.point - (transform.forward * 0.125f);
                var rot = Quaternion.FromToRotation(Vector3.forward, -transform.forward);
                ObjectManager.GetObject(ImpactEffect.gameObject, pos, rot);
            }
            else {
                // Set up projectile aesthetics
                ProjectileTimer = DespawnTime;
                ProjectileMaterial.SetColor("_DiffuseColor", ProjectileGradient.Evaluate(1.0f));
                ProjectileRenderer.SetPosition(0, transform.position);
                ProjectileRenderer.SetPosition(1, transform.position + transform.forward * Range);
            }
        }
    }
}