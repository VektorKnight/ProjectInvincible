using System.Collections.Generic;
using UnityEngine;
using InvincibleEngine;
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
        private HashSet<GameObject> _objects;
        private Stack<GameObject> _openSet;

        // Exceptions
        private static readonly ObjectPoolException MissingComponentException = new ObjectPoolException("Specified object must have at least one PooledBehavior attached!");
        private static readonly ObjectPoolException OverloadException = new ObjectPoolException("This pool has reached it's maximum size or all available objects are in use.");
        private static readonly ObjectPoolException InvalidReturnException =   new ObjectPoolException("Returned object does not belong to this Object Pool");

        // Constructor
        public ObjectPool (GameObject poolObject, int maxObjects) {
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
        /// <exception cref="OverloadException">Thrown if this pool has reached it's maximum size.</exception>
        public GameObject GetObject(Vector3 position, Quaternion rotation) {       
            // Throw an exception if the open set is empty and we cannot allocate more
            if (_openSet.Count == 0 && TotalObjects >= MaxObjects) throw OverloadException;          
            
            // Grab an object from the open set if possible and return it
            GameObject gameObject;
            PooledBehavior pooledBehavior;
            if (_openSet.Count > 0) {
                // Grab the object and set it's transform
                gameObject = _openSet.Pop();
                gameObject.transform.SetPositionAndRotation(position, rotation);
                
                // Activate the object, invoke the retrieved
                gameObject.SetActive(true);
                pooledBehavior = gameObject.GetComponent<PooledBehavior>();
                pooledBehavior?.OnRetrieved();
                return gameObject;
            }
            
            // Instantiate a new object, initialize it, and add it to the owned objects set
            gameObject = Object.Instantiate(_pooledObject, position, rotation);
            pooledBehavior = gameObject.GetComponent<PooledBehavior>();
            pooledBehavior?.Start();
            pooledBehavior?.OnRetrieved();
            _objects.Add(gameObject);  
            return gameObject;
        }
        
        /// <summary>
        /// Return an object to this pool.
        /// </summary>
        /// <param name="obj">The object to return.</param>
        /// <exception cref="InvalidReturnException">Thrown if the object does not belong to this pool.</exception>
        public void ReturnObject(GameObject obj) {
            // Throw an exception if the object does not belong to this pool.
            if (!_objects.Contains(obj)) throw InvalidReturnException;
            
            // Invoke the returned callback
            var pooledBehavior = obj.GetComponent<PooledBehavior>();
            pooledBehavior?.OnReturned();
            
            // Disable the object and return it to the open set
            obj.SetActive(false);
            _openSet.Push(obj);
        }

        // IDisposable Implementation
        public void Dispose() {
            // Destroy all owned objects
            foreach (var obj in _objects) obj.GetComponent<PooledBehavior>().Terminate();

            _pooledObject = null;
            _objects = null;
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