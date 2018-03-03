using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace InvincibleEngine.VektorLibrary.Utility {
    /// <summary>
    /// Static Library for Projects
    /// Copyright 2017 VektorKnight | All Rights Reserved
    /// </summary>
    public static class VektorUtility {
        
        // Wrap Euler Angles
        public static Vector3 WrapAngles(Vector3 angles) {
            if (angles.x > 180) angles.x -= 360;
            if (angles.x < -180) angles.x += 360;
            if (angles.y > 180) angles.y -= 360;
            if (angles.y < -180) angles.y += 360;
            if (angles.z > 180) angles.z -= 360;
            if (angles.z < -180) angles.z += 360;
            return angles;
        }

        // Returns the average of an array of Vector3s
        public static Vector3 GetVectorAverage(IEnumerable<Vector3> vectors, bool normalize = false) {
            var sum = Vector3.zero;

            // Make sure we weren't given an empty array
            var enumerable = vectors as Vector3[] ?? vectors.ToArray();
            if (!enumerable.Any()) return Vector3.zero;
            
            // Sum the vectors
            foreach (var vector in enumerable) {
                sum += vector;
            }

            // Return the average or normalized average
            if (normalize) return (sum / enumerable.Count()).normalized;
            return sum / enumerable.Count();
        }
        
        // Calculate distance between two vectors projected onto a plane
        public static float PlanarDistance(Vector3 p1, Vector3 p2, Vector3 normal) {
            p1 = Vector3.ProjectOnPlane(p1, normal);
            p2 = Vector3.ProjectOnPlane(p2, normal);
            return Vector3.Distance(p1, p2);
        }
        
        // Calculate the squared distance between two vectors projected onto a plane
        public static float SqrPlanarDistance(Vector3 p1, Vector3 p2, Vector3 normal) {
            p1 = Vector3.ProjectOnPlane(p1, normal);
            p2 = Vector3.ProjectOnPlane(p2, normal);
            return Vector3.SqrMagnitude(p2 - p1);
        }
        
        // Reset the transform values
        public static void ResetTransform(this Transform transform) {
            transform.position = Vector3.zero;
            transform.rotation = Quaternion.identity;
            transform.localScale = Vector3.one;
        }
        
        // Wrap a float value between start and end
        public static float WrapFloat(float current, float start, float end) {
            if (current > end) return start;
            if (current < start) return end;
            return current;
        }

        //Returns a directional vector based on the input
        public static Vector3 GetDirectionVector(Transform transform, DirectionVectors direction) {
            switch (direction) {
                case DirectionVectors.GForward: { return Vector3.forward; }
                case DirectionVectors.GRight: { return Vector3.right; }
                case DirectionVectors.GUp: { return Vector3.up; }
                case DirectionVectors.LForward: { return transform.forward; }
                case DirectionVectors.LRight: { return transform.right; }
                case DirectionVectors.LUp: { return transform.up; }
                default: { return Vector3.zero; }
            }
        }
    }

    //Simple Object Pool
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
            
            var key = obj.name.Replace("(Clone)", "").Trim();
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

    //Multi-Object Pool
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

    //Custom Exceptions
    public class ObjectPoolException : System.Exception {   //Exceptions related to the generic ObjectPool class
        public ObjectPoolException() { }
        public ObjectPoolException(string message) : base(message) { }
        public ObjectPoolException(string message, System.Exception inner) : base(message, inner) { }
    }

    //Pooled Objects
    [System.Serializable]
    public struct PoolObject {
        public GameObject Object;
        public int PoolSize;
    }

    //Pooled WeightedObjects (Level Generator Stuff)
    [System.Serializable]
    public struct PooledWeightedObject {
        public PoolObject PoolObject;
        public int Weight;
    }

    //Directional Vectors
    [System.Serializable]
    public enum DirectionVectors {
        GForward,
        GRight,
        GUp,
        LForward,
        LRight,
        LUp
    }


    //Simple float pair struct
    [System.Serializable]
    public struct RangeFloat {
        public float Min;
        public float Max;

        public RangeFloat (float min, float max) {
            Min = min;
            Max = max;
        }
    }
}
