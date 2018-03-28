using UnityEngine;

namespace VektorLibrary.Pathfinding.AStar {
    public static class Heuristics {
        
        /// <summary>
        /// The type of heuristic to use.
        /// </summary>
        public enum Heuristic {
            Manhattan,
            Octile,
            Euclidean
        }
        
        /// <summary>
        /// Returns the Manhattan distance between two nodes.
        /// </summary>
        /// <param name="a">The first node.</param>
        /// <param name="b">The second node.</param>
        public static int Manhattan(Vector2Int a, Vector2Int b) {
            var dX = Mathf.Abs(a.x - b.x);
            var dY = Mathf.Abs(a.y - b.y);
            return dX + dY;
        }
        
        /// <summary>
        /// Returns the Octile distance between two nodes.
        /// Values multiplied by 10 for integer math precision.
        /// </summary>
        /// <param name="a">The first node.</param>
        /// <param name="b">The second node.</param>
        public static int Octile(Vector2Int a, Vector2Int b) {
            var dX = 10 * Mathf.Abs(a.x - b.x);
            var dY = 10 * Mathf.Abs(a.y - b.y);
            return dX < dY ? 14 * (dX + dY) : 14 * (dY + dX);
        }
        
        /// <summary>
        /// Returns the Euclidean distance between two nodes.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        public static float Euclidean(Vector2Int a, Vector2Int b) {
            return Vector2Int.Distance(a, b);
        }
    }
}