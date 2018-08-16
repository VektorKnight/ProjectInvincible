using System.Collections.Generic;
using System.Linq;
using UnityEditor;
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

        public static Color Inverse(this Color color) {
            return new Color(1f - color.r, 1f - color.g, 1f - color.b, color.a);
        }

        public static Vector3[] CenteredGrid(Vector3 origin, int count, float scale = 1f) {
            // Make sure count is always even
            count += count % 2;
            
            // Calculate side length as count / 2
            var sideLength = Mathf.CeilToInt(Mathf.Sqrt(count));
            
            // Initialize grid array
            var grid = new Vector3[count];

            for (var i = 0; i < count; i++) {
                var x = i % sideLength;
                var y = i / sideLength;
                grid[i] = new Vector3(origin.x + (x * scale), origin.y, origin.z + (y * scale));
            }

            return grid;
        }

        public static bool MouseInView() {
            #if UNITY_EDITOR
            if (Input.mousePosition.x == 0 || Input.mousePosition.y == 0 || Input.mousePosition.x >= Handles.GetMainGameViewSize().x - 1 || Input.mousePosition.y >= Handles.GetMainGameViewSize().y - 1) {
                return false;
            }
            #else
            if (Input.mousePosition.x == 0 || Input.mousePosition.y == 0 || Input.mousePosition.x >= Screen.width - 1 || Input.mousePosition.y >= Screen.height - 1) {
            return false;
            }
            #endif
            return true;
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
        
        public static Rect ScreenSpaceRect(this RectTransform transform)
        {
            var size = Vector2.Scale(transform.rect.size, transform.lossyScale);
            var rect = new Rect(transform.position.x, Screen.height - transform.position.y, size.x, size.y);
            rect.x -= (transform.pivot.x * size.x);
            rect.y -= ((1.0f - transform.pivot.y) * size.y);
            return rect;
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
