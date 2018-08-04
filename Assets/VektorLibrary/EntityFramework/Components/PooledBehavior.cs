using InvincibleEngine;
using UnityEngine;
using VektorLibrary.EntityFramework.Interfaces;
using VektorLibrary.EntityFramework.Singletons;

namespace VektorLibrary.EntityFramework.Components {
    public class PooledBehavior : EntityBehavior, IPoolable {
        // Unity Inspector
        [Header("Pooled Object Settings")] 
        [SerializeField] protected bool AutoDespawn;
        [SerializeField] protected float DespawnTime;     
        
        // Called when this object is allocated from a pool
        public virtual void OnRetrieved() {
            if (AutoDespawn) Invoke(nameof(ReturnToPool), DespawnTime);
        }
        
        // Called when this object is returned to a pool
        public virtual void OnReturned() { }
        
        // Called to try and return this object to the pool
        public virtual void ReturnToPool() {
            // Cancel any invocations
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