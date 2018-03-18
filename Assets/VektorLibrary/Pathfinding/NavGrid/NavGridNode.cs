using System;
using System.Runtime.InteropServices;

namespace VektorLibrary.Pathfinding.NavGrid {
    [StructLayout(LayoutKind.Sequential)]
    [Serializable] public struct NavGridNode {
        // Node Properties
        public readonly int X, Y;           // The X,Y indices of this node within a NavGrid
        public bool Passable;               // Determines whether this node is passable or not
        public float Height;                // The height of the geometry at this node
        public float Slope;                 // The steepness of the geometry at this node

        /// <summary>
        /// Create a new NavGridNode with the specified properties.
        /// </summary>
        /// <param name="x">The X index of this node within the NavGrid.</param>
        /// <param name="y">The Y index of this node within the NavGrid.</param>
        /// <param name="passable">The passability of this node.</param>
        /// <param name="height">The height value for this node.</param>
        /// <param name="slope">The steepness value for this node.</param>
        public NavGridNode(int x, int y, bool passable, float height, float slope) {
            X = x;
            Y = y;
            Passable = passable;
            Height = height;
            Slope = slope;
        }
    }
}