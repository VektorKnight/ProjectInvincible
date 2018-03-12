using System.Collections.Generic;
using InvincibleEngine.Components.Generic;
using UnityEngine;
using VektorLibrary.EntityFramework.Components;

namespace VektorLibrary.Utility {
    // Simple Object Pool
    public class ObjectPool : System.IDisposable {
        // Object to Pool
        private GameObject _pooledObject;

        // Properties: Object Counts
        public int MaxObjects { get; }
        public int TotalObjects => _objects.Count;
        public int ActiveObjects => _objects.Count - _openSet.Count;

        // Object Pool
        private readonly HashSet<GameObject> _objects;
        private Stack<GameObject> _openSet;

        // Exceptions
        private static readonly ObjectPoolException MissingComponentException = new ObjectPoolException("Specified object must have at least one PooledBehavior attached!");
        private static readonly ObjectPoolException DynamicOverloadException = new ObjectPoolException("Dynamic pool has reached the maximum number of objects.");
        private static readonly ObjectPoolException InvalidReturnException =   new ObjectPoolException("Returned object does not belong to this Object Pool");

        // Constructor
        public ObjectPool (GameObject poolObject, int maxObjects, bool prePopulate = false) {
            // Throw an exception if the specified object does not have a PooledBehavior attached
            if (poolObject.GetComponent<PooledBehavior>() == null) throw MissingComponentException;
            
            _pooledObject = poolObject;
            MaxObjects = maxObjects;
            
            _objects = new HashSet<GameObject>();
            _openSet = new Stack<GameObject>(maxObjects);
        }

        /// <summary>
        /// Retrieve an object from this pool.
        /// </summary>
        /// <param name="position">Desired world position of the object.</param>
        /// <param name="rotation">Desired world rotation of the object.</param>
        /// <returns></returns>
        /// <exception cref="DynamicOverloadException">Thrown if this pool has reached it's maximum size.</exception>
        public GameObject GetObject(Vector3 position, Quaternion rotation) {
            // If objects are available, return one
            if (_openSet.Count > 0) {
                // Grab the object and set it's transform
                var obj = _openSet.Pop();
                obj.transform.SetPositionAndRotation(position, rotation);
                
                // Activate and return the object.
                obj.SetActive(true);
                return obj;
            }
            
            // If no objects are available and we are below the limit, create one
            if (TotalObjects >= MaxObjects) throw DynamicOverloadException;
            {
                var obj = Object.Instantiate(_pooledObject, position, rotation);
                _objects.Add(obj);
                return obj;
            }
        }
        
        /// <summary>
        /// Return an object to this pool.
        /// </summary>
        /// <param name="obj">The object to return.</param>
        /// <exception cref="InvalidReturnException">Thrown if the object does not belong to this pool.</exception>
        public void ReturnObject(GameObject obj) {
            // Throw an exception if the object does not belong to this pool.
            if (!_objects.Contains(obj)) throw InvalidReturnException;
            
            // Disable the object and return it to the open set
            obj.SetActive(false);
            _openSet.Push(obj);
        }

        // IDisposable Implementation
        public void Dispose() {
            //Destroy the Open and Closed Sets (Rev. 5/20/17 - 3:35pm)
            while (_openSet.Count > 0) { Object.Destroy(_openSet.Pop()); }

            _pooledObject = null;
            _openSet = null;
        }
    }
    
    // Custom Exceptions
    public class ObjectPoolException : System.Exception {   //Exceptions related to the generic ObjectPool class
        public ObjectPoolException() { }
        public ObjectPoolException(string message) : base(message) { }
        public ObjectPoolException(string message, System.Exception inner) : base(message, inner) { }
    }
}