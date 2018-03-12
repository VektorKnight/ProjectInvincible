using InvincibleEngine.Managers;
using UnityEngine;

namespace InvincibleEngine.Components.Generic {
    public class PooledObject : MonoBehaviour {
        
        // Unity Inspector
        [Header("Despawn Config")] 
        
        [Tooltip("Uncheck this if this objects behavior handles despawning.")]
        [SerializeField] private bool _autoDespawn = true;
        
        [Tooltip("The time")]
        [SerializeField] private float _despawnTime = 5.0f;
        
        // Initialization
        private void Awake() {
            // Set the object to despawn after the given time if enabled
            if (!_autoDespawn) return;

            Invoke(nameof(Despawn), _despawnTime);
        }
        
        // Initialization: Object Pool
        private void OnEnable() {
            // Set the object to despawn after the given time if enabled
            if (!_autoDespawn) return;

            Invoke(nameof(Despawn), _despawnTime);
        }

        // Despawn the object and return it to the multi-pool
        public void Despawn() {
            // Cancel the automatic invocation if method already called
            CancelInvoke();
			
            // Try to return this object to the multi-pool
            try {
                ObjectManager.MultiObjectPool.ReturnObject(gameObject);
            }
            catch (System.Exception ex) {
                // Catch the exception and warn the user
                Debug.LogWarning($"{name}: Encountered an exception while attempting to return to the multi-pool!\n" + ex.Message);
				
                // Assume this object is somehow invalid and destroy it
                Destroy(gameObject);
            }	
        }
    }
}