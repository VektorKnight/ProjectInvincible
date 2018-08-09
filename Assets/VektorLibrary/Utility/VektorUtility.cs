using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace VektorLibrary.Utility {
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
        
        public static Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Vector3 angles) {
            return Quaternion.Euler(angles) * (point - pivot) + pivot;
        }
        
        // Shuffles a list
        public static void Shuffle<T>(this IList<T> list) {
            var n = list.Count;
            var rnd = new System.Random();
            while (n > 1) {
                var k = (rnd.Next(0, n) % n);
                n--;
                var value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
        
        // Reset the transform values
        public static void ResetTransform(this Transform transform) {
            transform.position = Vector3.zero;
            transform.rotation = Quaternion.identity;
            transform.localScale = Vector3.one;
        }

        // Returns a directional vector based on the input
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

    // Pooled Objects
    [System.Serializable]
    public struct PoolObject {
        public GameObject Object;
        public int PoolSize;
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
