using System;
using UnityEngine;

namespace VektorLibrary.Pathfinding.AStar {
    /// <summary>
    /// Represents a completed A* path result.
    /// </summary>
    public struct AStarResult {
        public bool Success;       // Whether or not the path was successful
        public Vector3[] Nodes;    // The nodes composing the path
        public Action<AStarResult> Callback;    // The callback to invoke once the path request is completed

        public AStarResult(bool success, Vector3[] nodes, Action<AStarResult> callback) {
            Success = success;
            Nodes = nodes;
            Callback = callback;
        }
    }
}