using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace VektorLibrary.Pathfinding.NavGrid {
    [Serializable] public class NavGridNode : IComparable<NavGridNode> {
        // Node Metadata
        public readonly Vector2Int GridPosition;    // The grid=space position/index of this node.
        public readonly Vector3 WorldPosition;      // The world=spacve position of this node.
        public bool Passable;                       // Determines whether this node is passable or not
        public float Slope;                         // The steepness of the geometry at this node
        
        // Node Pathfinding Data
        public int GCost;
        public int HCost;
        public int FCost => GCost + HCost;
        public NavGridNode Parent;

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

        public int CompareTo(NavGridNode other) {
            var compare = FCost.CompareTo(other.FCost);
            return compare == 0 ? -HCost.CompareTo(other.HCost) : -compare;
        }
    }
}