using InvincibleEngine.WeaponSystem;
using UnityEngine;
using VektorLibrary.EntityFramework.Components;

namespace InvincibleEngine.UnitFramework.Components {
    /// <summary>
    /// Controls an energy shield object spawned in by a unit/structure.
    /// </summary>
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(SphereCollider))]
    public class EnergyShield : EntityBehavior, IDamageable {
        // Unity Inspector
        [SerializeField] private ParticleSystem _burstEffect;
        
        // Private: Shield Config Values
        private float _maxHealth;    
        private float _rechargeRate;  
        private float _rechargeDelay;  
        
        // Private: Shield Runtime Values
        private bool _initialized;
        private float _delayTimer;
        private bool _shieldDown;
        
        // Private: Required References
        private MeshRenderer _shieldRenderer;
        private SphereCollider _shieldCollider;
        private ParticleSystem _burstEffectInstance;
        
        // Properties: Shield Stats
        public float CurrentHealth { get; private set; }

        // Initialization
        public void Initialize(float radius, float health, float rechargeRate, float rechargeDelay, int teamLayer) {
            // Register this object if it isn't already registered
            if (!Registered) EntityManager.RegisterBehavior(this);
            
            // Reference required components
            _shieldRenderer = GetComponent<MeshRenderer>();
            _shieldCollider = GetComponent<SphereCollider>();
            
            // Instantiate the burst effect as a shild of this object if set
            if (_burstEffect != null) {
                _burstEffectInstance = Instantiate(_burstEffect, transform);
                _burstEffectInstance.transform.localPosition = Vector3.zero;
                _burstEffectInstance.transform.localRotation = Quaternion.identity;
            }
            
            // Set the layer of the shield to the specified team layer
            gameObject.layer = teamLayer;
            
            // Set the scale of the shield to the radius / 2
            transform.localScale = Vector3.one * radius;
            
            // Set config values
            _maxHealth = health;
            _rechargeRate = rechargeRate;
            _rechargeDelay = rechargeDelay;
            
            // Set initialized flag
            _initialized = true;
        }
        
        // Sim Update Callback
        public override void OnSimUpdate(float fixedDelta, bool isHost) {
            // Exit if not initialized or the shield is down
            if (!_initialized || _shieldDown) return;
            
            // Decrement the delay timers if necessary
            if (_delayTimer > 0f) _delayTimer-= fixedDelta;
			
            // Regen the shield if the timer has expired
            if (_delayTimer <= 0f) 
                CurrentHealth += _rechargeRate * Time.deltaTime;
            
            // Clamp shield health
            CurrentHealth = Mathf.Clamp(CurrentHealth, 0f, _maxHealth);
            
            // Check for critical damage
            if (CurrentHealth <= 0f)
                OnShieldBurst();
            
            // Call base method
            base.OnSimUpdate(fixedDelta, isHost);
        }

        // Called to apply damage to the shield
        public void ApplyDamage(float damage) {
            // Exit if not initialized
            if (!_initialized) return;
            
            // Apply damage to shield health
            CurrentHealth -= damage;
            
            // Set delay timer
            _delayTimer = _rechargeDelay;
        }
        
        // Called when the shield is dealt critical damage
        private void OnShieldBurst() {
            // Exit if the shield is already down
            if (_shieldDown) return;
            
            // Disable the renderer and collider
            _shieldRenderer.enabled = false;
            _shieldCollider.enabled = false;
            
            // Play burst effect if possible
            if (_burstEffectInstance != null)
                _burstEffectInstance.Play();
            
            // Invoke the shield recharge method
            Invoke(nameof(OnShieldRecharge), _rechargeDelay);
            
            // Set shield down flag
            _shieldDown = true;
        }
        
        // Called when the shield recharges from a downed state
        private void OnShieldRecharge() {
            // Reset the shield down flag
            _shieldDown = false;
            
            // Re-enable the renderer and collider
            _shieldRenderer.enabled = true;
            _shieldCollider.enabled = true;
            
            // Reset health value
            CurrentHealth = _maxHealth;
        }
    }
}