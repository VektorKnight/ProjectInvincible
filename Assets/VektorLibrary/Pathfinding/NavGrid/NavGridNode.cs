using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace VektorLibrary.Pathfinding.NavGrid {
    [StructLayout(LayoutKind.Sequential)]
    [Serializable] public struct NavGridNode {
        // Node Properties
        public readonly Vector2Int GridPosition;    // The grid=space position/index of this node.
        public readonly Vector3 WorldPosition;      // The world=spacve position of this node.
        public bool Passable;                       // Determines whether this node is passable or not
        public float Slope;                         // The steepness of the geometry at this node

        /// <summary>
        /// Create a new NavGridNode with the specified properties.
        /// </summary>
        /// /// <param name="gridPos">The grid-space position of this node.</param>
        /// <param name="worldPos">The world-space position of this node.</param>
        /// <param name="passable">The passability of this node.</param>
        /// <param name="slope">The steepness value for this node.</param>
        public NavGridNode(Vector2Int gridPos, Vector3 worldPos, bool passable, float slope) {
            GridPosition = gridPos;
            WorldPosition = worldPos;
            Passable = passable;
            Slope = slope;
        }
    }
}