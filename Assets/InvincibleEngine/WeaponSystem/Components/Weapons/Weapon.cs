using InvincibleEngine.Managers;
using InvincibleEngine.WeaponSystem.Components.Projectiles;
using InvincibleEngine.WeaponSystem.Interfaces;
using UnityEngine;
using VektorLibrary.Utility;

namespace InvincibleEngine.WeaponSystem.Components.Weapons {
    /// <summary>
    /// Abstract base class for all weapons.
    /// </summary>
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(AudioSource))]
    public abstract class Weapon : MonoBehaviour, IWeapon {
        
        // Unity Inspector
        [Header("Weapon Metadata")] 
        [SerializeField] private string _weaponName;
        [SerializeField] private Sprite _reticleImage;
        
        [Header("Weapon Aiming")]
        [SerializeField] private Vector2 _pitchRange = new Vector2(-20f, 20f);
        [SerializeField] private Vector2 _yawRange = new Vector2(-180f, 180f);
        [SerializeField] private float[] _zoomLevels = {1.0f, 0.75f};

        [Header("Weapon Effects")] 
        [SerializeField] protected ParticleSystem[] ParticleEffects;
        [SerializeField] protected AudioClip[] SoundEffects;
        [SerializeField] protected int FireEffectIndex = 0;
        
        // Unity Inspector
        [Header("Projectile Spawn")] 
        [SerializeField] protected Projectile Projectile;
        [SerializeField] protected Transform Muzzle;
        [SerializeField] protected float Variance;
        
        // Protected: Weapon State
        protected bool Initialized;
        
        // Protected: Required Components
        protected Animator WeaponAnimator;
        protected AudioSource WeaponAudio;
        
        // Public Readonly: Metadata
        public string WeaponName => _weaponName;
        public Sprite ReticleImage => _reticleImage;
        
        // Public Readonly: Ownership
        public ulong OwnerId { get; private set; }
        
        // Public Readonly: Aiming & Transform
        public Vector3 SignedLocalRotation => VektorUtility.WrapAngles(transform.localEulerAngles);
        public Vector3 CameraAimPoint { get; set; }
        public Vector2 PitchRange => _pitchRange;
        public Vector2 YawRange => _yawRange;
        public float[] ZoomLevels => _zoomLevels;
        
        // Public Read/Write: Weapon Animation Speed
        public float AnimationSpeed {
            get { return WeaponAnimator.speed; } 
            set { WeaponAnimator.speed = Mathf.Clamp(value, 0.25f, 2.0f); }
        }
        
        /// <summary>
        /// Initialize the weapon for use.
        /// </summary>
        /// <param name="ownerId">The ID of the weapon owner.</param>
        public virtual void Initialize(ulong ownerId) {
            // Exit if already initialized
            if (Initialized) return;
			
            // Set the owner id
            OwnerId = ownerId;
			
            // Reference the necessary weapon components
            WeaponAnimator = GetComponent<Animator>();
            WeaponAudio = GetComponent<AudioSource>();
			
            // Initialization complete
            Initialized = true;
        }
        
        // Used internally to instantiate a projectile
        protected virtual void SpawnProjectile() {
            // Check if projectile or muzzle are null
            if (Projectile == null || Muzzle == null) {
                Debug.LogWarning($"The projectile and/or muzzle for {name} has not been set up!");
                return;
            }
			
            // Grab a projectile from the multi-pool
            var aimVector = Vector3.Normalize(CameraAimPoint - Muzzle.position);
            var rotation = Quaternion.LookRotation(aimVector + (Variance * Random.insideUnitSphere));
            var projectile = ObjectManager.GetObject(Projectile.gameObject, Muzzle.position, rotation);
            projectile.GetComponent<Projectile>().Initialize(OwnerId);
        }
        
        /// <summary>
        /// Trigger the weapon.
        /// </summary>
        public abstract void TriggerDown();
        
        /// <summary>
        /// Release the trigger.
        /// </summary>
        public abstract void TriggerUp();
        
        /// <summary>
        /// Start the reloading process.
        /// </summary>
        public abstract void Reload();

        public virtual void FireProjectile() {
            // Play the first particle and sound effects if possible
            PlayEffectPair(FireEffectIndex);
            
            // Spawn a projectile
            SpawnProjectile();
        }

        /// <summary>
        /// Plays the sound effect at the given index.
        /// </summary>
        /// <param name="index">The index of the desired sound effect.</param>
        public virtual void PlayParticleEffect(int index) {
            // Check if the given index is within bounds
            if (index >= 0 && index < ParticleEffects.Length) {
                ParticleEffects[index]?.Play();
            }
            else {
                Debug.LogWarning($"{name}: Particle effect at index {index} is not specified or out of range!");
            }
        }

        /// <summary>
        /// Plays the particle effect at the given index.
        /// </summary>
        /// <param name="index">The index of the desired particle effect.</param>
        public virtual void PlaySoundEffect(int index) {
            // Check if the given index is within bounds
            if (index >= 0 && index < SoundEffects.Length) {
                WeaponAudio.PlayOneShot(SoundEffects[index]);
            }
            else {
                Debug.LogWarning($"{name}: Sound effect at index {index} is not specified or out of range!");
            }
        }
        
        /// <summary>
        /// Plays the particle and sound effects at the specified index.
        /// </summary>
        /// <param name="index">Index for the particle and sound effect.</param>
        public virtual void PlayEffectPair(int index) {
            PlayParticleEffect(index);
            PlaySoundEffect(index);
        }

        /// <summary>
        /// Reset the fire state of the weapon.
        /// Resets all state parameters of the weapon.
        /// </summary>
        public virtual void ResetFireState() {
            // Reset the weapon animation speed
            AnimationSpeed = 1.0f;
        }
    }
}