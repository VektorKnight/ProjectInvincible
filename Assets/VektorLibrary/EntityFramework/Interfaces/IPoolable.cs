using UnityEngine;

namespace InvincibleEngine {
    public interface IPoolable {
        
        // Property: Initialized
        bool Initialized { get; }

        /// <summary>
        /// Called when an object is initialized in pre-populated pool or instantiated for the first time in a dynamic pool.
        /// </summary>
        void Initialize();
        
        /// <summary>
        /// Called when this object is retrieved from a pool.
        /// </summary>
        void OnRetrieved();
        
        /// <summary>
        /// Called when this object is returned to a pool.
        /// </summary>
        void OnReturned();
        
        /// <summary>
        /// Returns this object to it's pool.
        /// </summary>
        void ReturnToPool();
    }
}