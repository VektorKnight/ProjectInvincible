using System.Collections.Generic;
using UnityEngine;

namespace VektorLibrary.Utility {
    // Simple Object Pool
    public class ObjectPool : System.IDisposable {

        // Object to Pool
        private GameObject _poolObject;
        
        // Parent transform for pool objects
        private readonly Transform _parent;

        // Properties: Object Counts
        public int MaxObjects { get; }

        public int TotalObjects { get; private set; }
        public int ActiveObjects { get; private set; }

        // Object Pool
        private Queue<GameObject> _openSet;

        // Exceptions
        private static readonly ObjectPoolException DynamicOverloadException = new ObjectPoolException("Dynamic pool has reached the maximum number of objects.");
        private static readonly ObjectPoolException ReturnOverloadException =  new ObjectPoolException("Tried to return an object to a full Object Pool");
        private static readonly ObjectPoolException InvalidReturnException =   new ObjectPoolException("Returned object does not belong to this Object Pool");

        // Constructor
        public ObjectPool (GameObject poolObject, int maxObjects, Transform parent) {
            _poolObject = poolObject;
            MaxObjects = maxObjects;
            _parent = parent;
            
            // Parent transform must be centered at origin (0,0,0) with zero rotation
            _parent.position = Vector3.zero;
            _parent.rotation = Quaternion.identity;

            _openSet = new Queue<GameObject>();
        }

        // Get Object from Pool
        public GameObject GetObject(Vector3 position, Quaternion rotation) {
            // If objects are available, return one
            if (_openSet.Count > 0) {
                var obj = _openSet.Dequeue();
                obj.transform.position = position;
                obj.transform.rotation = rotation;
                obj.SetActive(true);
                ActiveObjects++;
                return obj;
            }
            
            // If no objects are available and we are below the limit, create one
            if (TotalObjects >= MaxObjects) throw DynamicOverloadException;
            {
                TotalObjects++;
                var obj = Object.Instantiate(_poolObject, position, rotation);
                obj.transform.parent = _parent;
                ActiveObjects++;
                return obj;
            }
        }

        // Return Object to Pool
        public void ReturnObject (GameObject obj) {
            if (_openSet.Count + 1 > MaxObjects) {
                throw ReturnOverloadException;
            }
            
            var key = obj.name.Replace("(Clone)", "(Pooled)").Trim();
            if (key == _poolObject.name) {
                obj.SetActive(false);
                _openSet.Enqueue(obj);
                ActiveObjects--;
            }
            else { throw InvalidReturnException; }
        }

        // IDisposable Implementation
        public void Dispose() {
            //Destroy the Open and Closed Sets (Rev. 5/20/17 - 3:35pm)
            while (_openSet.Count > 0) { Object.Destroy(_openSet.Dequeue()); }

            _poolObject = null;
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