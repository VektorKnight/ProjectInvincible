using InvincibleEngine.WeaponSystem.Enums;
using UnityEngine;

namespace InvincibleEngine.WeaponSystem.Components.Weapons {
    /// <summary>
    /// Simple weapon class for semi and fully automatic weapons utlizing a magazine
    /// and reload mechanic.
    /// </summary>
    [AddComponentMenu("Weapon System/Weapons/Magazine Weapon")]
    public class MagazineWeapon : Weapon {
   
        // Unity Inspector
        [Header("Weapon Attributes")] 
        [SerializeField] private WeaponFireMode _fireMode = WeaponFireMode.SemiAuto;
        [SerializeField] private int _clipSize = 8;
        [SerializeField] private bool _autoReload = true;

        [Header("Weapon Effects")]
        [SerializeField] protected int EmptyClipEffectIndex = 1;
        
        // Public Readonly: Attributes
        public int ClipSize => _clipSize;
        
        // Protected: State
        protected int CurrentClip;
        protected bool CanFire;
        protected bool Reloading;
        
        // Initialization Override
        public override void Initialize(ulong ownerId) {
            // Set the current clip value and CanFire bool
            CurrentClip = ClipSize;
            CanFire = true;
            
            // Call the base method
            base.Initialize(ownerId);
        }
        
        // Pull the weapon's trigger
        public override void TriggerDown() {
            switch (_fireMode) {
                case WeaponFireMode.SemiAuto:
                    if (CanFire)
                        WeaponAnimator.SetTrigger("Fire");
                    else {
                        PlayEffectPair(EmptyClipEffectIndex);
                        if (_autoReload)
                            Reload();
                    }
                    break;
                case WeaponFireMode.FullAuto:
                    if (CanFire) 
                        WeaponAnimator.SetBool("Fire", true);
                    else {
                        PlayEffectPair(EmptyClipEffectIndex);
                        if (_autoReload)
                            Reload();
                    }
                    break;
                default:
                    Debug.LogWarning($"{name}: Fire mode value is invalid. Please check the weapon configuration.");
                    return;
            }
        }
        
        // Release the weapon's trigger
        public override void TriggerUp() {
            if (_fireMode == WeaponFireMode.FullAuto) 
                WeaponAnimator.SetBool("Fire", false);
        }

        public override void Reload() {
            // Exit if the current clip is already full or we are reloading
            if (CurrentClip == ClipSize || Reloading) return;
            
            // Call TriggerUp()
            TriggerUp();
            
            // Set the animator trigger "Reload" and Reloading bool
            WeaponAnimator.SetTrigger("Reload");
            Reloading = true;
        }
        
        // Fire Projectile Override
        public override void FireProjectile() {
            // Exit if we cannot fire
            if (!CanFire) return;
            
            // Set the CanFire bool to false if the current clip is empty and exit
            if (CurrentClip <= 0) {
                CanFire = false;
                return;
            }
            
            // Decrement the current clip and fire a projectile
            CurrentClip--;
            base.FireProjectile();
        }
        
        /// <summary>
        /// Called by an animator controller to actually reload the weapon
        /// </summary>
        public override void ResetFireState() {
            // Reset the weapon state
            CurrentClip = ClipSize;
            Reloading = false;
            CanFire = true;
            
            // Call base method
            base.ResetFireState();
        }
    }
}