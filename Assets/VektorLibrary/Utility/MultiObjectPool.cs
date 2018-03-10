using System.Collections.Generic;
using UnityEngine;

namespace VektorLibrary.Utility {
    // Multi-Object Pool
    public class MultiObjectPool : System.IDisposable {

        // Dictionary: ObjectPool
        public Dictionary<string, ObjectPool> MultiPool { get; private set; }

        // Exceptions
        private static readonly ObjectPoolException InvalidGetException =    new ObjectPoolException("Requested object type does not exist in the multi-pool");
        private static readonly ObjectPoolException InvalidReturnException = new ObjectPoolException("Returned object type does not exist in the multi-pool");

        // Constructor
        public MultiObjectPool () {
            MultiPool = new Dictionary<string, ObjectPool>();
        }

        /// <summary>
        /// Create and add a new object pool to this multi-pool.
        /// </summary>
        /// <param name="poolObject">The prefab to be pooled.</param>
        /// <param name="maxSize">The maximum number of objects allowed in the pool.</param>
        /// <param name="parent">The object to serve as a parent transform.</param>
        public void NewObjectPool(GameObject poolObject, int maxSize, Transform parent) {
            // Make sure the specified pool doesn't already exist
            if (MultiPool.ContainsKey(poolObject.name)) {
                Debug.Log($"Object pool for {poolObject.name} already exists.");
            }
            else {
                // Make sure Unity's naming is trimmed to avoid errors
                var key = poolObject.name.Replace("(Clone)", "").Trim();
                MultiPool.Add(key, new ObjectPool(poolObject, maxSize, parent));
            }
        }
        
        /// <summary>
        /// Check if a pool for the specified object name exists in this multi-pool
        /// </summary>
        /// <param name="objectName">The name of the prefab to check for.</param>
        /// <returns></returns>
        public bool ContainsPool(string objectName) {
            return MultiPool.ContainsKey(objectName);
        }

        // Get Object from Multi-Pool
        public GameObject GetObject(GameObject obj, Vector3 position, Quaternion rotation) {
            if (MultiPool.ContainsKey(obj.name)) {
                return MultiPool[obj.name].GetObject(position, rotation);
            }
            
            throw InvalidGetException;
        }

        // Return Object to Multi-Pool
        public void ReturnObject(GameObject obj) {
            var key = obj.name.Replace("(Clone)", "").Trim();
            if (MultiPool.ContainsKey(key)) {
                MultiPool[key].ReturnObject(obj);
            }
            else { throw InvalidReturnException; }
        }
        
        // Get the total number of active objects
        public int GetActiveCount() {
            var count = 0;
            foreach (var kvp in MultiPool) count += kvp.Value.ActiveObjects;
            return count;
        }
        
        // Get the total number of objects
        public int GetTotalCount() {
            var count = 0;
            foreach (var kvp in MultiPool) count += kvp.Value.TotalObjects;
            return count;
        }

        // IDisposable Implementation
        public void Dispose () {
            // Destroy the Multi-Pool
            foreach (var kvp in MultiPool) { kvp.Value.Dispose(); }
            MultiPool = null;
        }
    }
}