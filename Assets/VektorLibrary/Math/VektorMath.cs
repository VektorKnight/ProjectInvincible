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
        public static Vector3 GetVectorAverage(IEnumerable<Vector3> vectors, bool normalize = false) {
            var sum = Vector3.zero;

            // Make sure we weren't given an empty array
            var enumerable = vectors as Vector3[] ?? vectors.ToArray();
            if (!enumerable.Any()) return Vector3.zero;
            
            // Sum the vectors
            for (var i = 0; i < enumerable.Length; i++) {
                sum += enumerable[i];
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
    }
}