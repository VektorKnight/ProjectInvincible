using System;
using UnityEngine;

namespace VektorLibrary.Pathfinding.Grid {
    [Serializable] public struct NavGridConfig {
        // Properties: Dimensionality
        public Vector3 Origin;            // The world-space origin of the NavGrid.
        public int Size;                  // The X,Y size of the grid in Unity meters.
        public int UnitsPerNode;          // Number of Unity meters per node (Default is 4:1)
        public int Subdivision;           // The grid subdivided based on this value (must be a power of two).
        public int Dimension => Size / UnitsPerNode;
        public int NodeCount => Dimension * Dimension;

        // Properties: Passability
        public float MaxHeight;           // Maximum height of a node before it is marked as impassable.
        public float MaxSteepness;        // Maximum steepness of a node before it is marked as impassable.
        public LayerMask GroundMask;      // The layer(s) representing the ground/passable geometry.
        public LayerMask ObstacleMask;    // The layer(s) that should be checked for obstructions.
    }
}