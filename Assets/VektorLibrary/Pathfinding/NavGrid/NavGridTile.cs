using System;
using System.Runtime.InteropServices;
using VektorLibrary.Math;

namespace VektorLibrary.Pathfinding.NavGrid {
    /// <summary>
    /// Represents a partition of a larger NavGrid.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [Serializable] public struct NavGridTile {
        // Public: Indices and Dimension
        public readonly int X, Y, L;           // The X,Y indices and side-length of this tile
        
        // Property: Nodes
        public NavGridNode[] Nodes { get; }    // The nodes contained within this tile
        
        /// <summary>
        /// Create a new NavGridTile with the specified parameters.
        /// </summary>
        /// <param name="x">The X index of this tile within it's NavGrid</param>
        /// <param name="y"></param>
        /// <param name="l"></param>
        public NavGridTile(int x, int y, int l) {
            // Sanity Check: X and Y must be non-negative
            if (x < 0 || y < 0) 
                throw new ArgumentException("The X and Y indices of a tile cannot be negative!");
            
            // Sanity Check: L must be greater than zero and a power of two
            if (l <= 0 || !VektorMath.IsPowerOfTwo(l)) 
                throw new ArgumentException("The side-length of a tile must be greater than zero and a power of two!");
            
            // Initialize values
            X = x;
            Y = y;
            L = l;
            
            // Initialize nodes with size of L^2
            Nodes = new NavGridNode[L*L];
        }
    }
}