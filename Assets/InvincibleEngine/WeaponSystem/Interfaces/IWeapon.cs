using UnityEngine;

namespace InvincibleEngine.WeaponSystem.Interfaces {
    public interface IWeapon {
        // Public Properties
        ulong OwnerId { get; }
        Vector3 CameraAimPoint { get; set; }

        // Initialization Functions
        void Initialize(ulong ownerId);
        
        // Control Functions
        void TriggerDown();
        void TriggerUp();
        void Reload();
        
        // Animation/Controller Functions
        void FireProjectile();
        void PlayParticleEffect(int index);
        void PlaySoundEffect(int index);
    }
}