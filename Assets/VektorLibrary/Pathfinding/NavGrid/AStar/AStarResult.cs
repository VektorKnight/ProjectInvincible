using UnityEngine;

namespace VektorLibrary.Pathfinding.NavGrid.AStar {
    /// <summary>
    /// Represents a completed A* path result.
    /// </summary>
    public struct AStarResult {
        public bool Success;       // Whether or not the path was successful
        public Vector3[] Nodes;    // The nodes composing the path

        public AStarResult(bool success, Vector3[] nodes) {
            Success = success;
            Nodes = nodes;
        }
    }
}