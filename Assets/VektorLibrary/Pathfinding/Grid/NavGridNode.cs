using System;
using UnityEngine;

namespace VektorLibrary.Pathfinding.Grid {
    [Serializable] public class NavGridNode {
        // Node Metadata
        public readonly int ID;          // The ID of this node within the NavGrid.
        public readonly Vector2Int Local;    // The grid-space index of this node.
        public readonly Vector3 World;       // The world-space position of this node.
        public Vector3 Normal;               // The normal of the geometry at this node
        public bool Passable;                // The passability of this node.
        public float Slope;                  // Slope value of the geometry at this node.
        

        /// <summary>
        /// Create a new NavGridNode with the specified properties.
        /// </summary>
        /// <param name="x">The X index assigned to this data.</param>
        /// <param name="y">The Y index assigned to this data.</param>
        /// <param name="worldPos">The world-space position of this node.</param>
        /// <param name="normal">The normal of the geometry at this node.</param>
        /// <param name="passable">The passability of this node.</param>
        public NavGridNode(int id, int x, int y, Vector3 worldPos, Vector3 normal, bool passable) {
            ID = id;
            Local = new Vector2Int(x, y);
            World = worldPos;
            Normal = normal;
            Passable = passable;
            Slope = 1f - Vector3.Dot(normal, Vector3.up);
        }      
    }
}