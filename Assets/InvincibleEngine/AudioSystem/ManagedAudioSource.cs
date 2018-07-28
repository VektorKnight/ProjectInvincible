using UnityEngine;
using VektorLibrary.EntityFramework.Components;

namespace InvincibleEngine.AudioSystem {
    public class ManagedAudioSource : EntityBehavior {
        // Unity Inspector
        [Header("Source Config")] 
        [SerializeField] private AudioClip _clip;

        [SerializeField] [Range(0f, 1f)] private float _volume = 1.0f;
        [SerializeField] [Range(-3f, 3f)] private float _pitch = 1.0f;
        [SerializeField] private bool _nonSpatial;
        
        // OnEnable
        private void OnEnable() {
            if (_clip == null) return;

            if (_nonSpatial)
                AudioManager.PlayNonSpatialClip(_clip, _volume);
            else
                AudioManager.PlayClipAtPosition(transform.position, _clip, _volume);
        }
    }
}