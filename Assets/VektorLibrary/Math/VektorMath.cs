using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace VektorLibrary.Math {
    public static class VektorMath {
        // Wrap a float value between start and end
        public static float WrapFloat(float current, float start, float end) {
            if (current > end) return start;
            return current < start ? end : current;
        }
        
        // Returns the average of an array of Vector3s
        public static Vector3 VectorAverage(Vector3[] vectors, bool normalize = false) {
            var sum = Vector3.zero;

            // Make sure we weren't given an empty array
            if (vectors.Length == 0) return Vector3.zero;
            
            // Sum the vectors
            for (var i = 0; i < vectors.Length; i++) {
                sum += vectors[i];
            }

            // Return the average or normalized average
            return normalize ? (sum / vectors.Length).normalized : sum / vectors.Length;
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

        /// <summary>
        /// Check if a given value is a power of two.
        /// </summary>
        /// <param name="value">The value to check.</param>
        /// <returns>True if power of 2, false otherwise.</returns>
        public static bool IsPowerOfTwo(int value) {
            return (value != 0) && ((value & (value - 1)) == 0);
        }
        
        
    }
}