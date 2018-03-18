using System;
using System.Runtime.InteropServices;
using VektorLibrary.Math;

namespace VektorLibrary.Pathfinding.NavGrid {
    /// <summary>
    /// Represents a partition of a larger NavGrid.
    /// </summary>
    [Serializable] public class NavGridTile {
        // Public: Indices and Dimension
        public readonly int X, Y, L;           // The X,Y indices and side-length of this tile
        
        // Property: Nodes
        public NavGridNode[] Nodes { get; }    // The nodes contained within this tile
        
        /// <summary>
        /// Create a new NavGridTile with the specified parameters.
        /// </summary>
        /// <param name="x">The X index of this tile.</param>
        /// <param name="y">The Y indes of this tile.</param>
        /// <param name="l">The side-length of this tile.</param>
        public NavGridTile(int x, int y, int l) {
            // Sanity Check: X and Y must be non-negative
            if (x < 0 || y < 0) 
                throw new ArgumentException("The X and Y indices of a tile must be non-negative!");
            
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
        
        /// <summary>
        /// Returns the node at the specified indices if it exists.
        /// </summary>
        /// <param name="x">The X index of the node.</param>
        /// <param name="y">The Y index of the node.</param>
        public NavGridNode GetNode(int x, int y) {
            // Throw an exception of the node does not exist in this tile.
            if (!ContainsNode(x, y))
                throw new IndexOutOfRangeException("The specified node does not exist within this tile!");
            
            // Convert grid-space to tile-space
            x -= X * L;
            y -= Y * L;
            
            // Convert x,y to a sequential index and return the node
            return Nodes[L * y + x];
        }
        
        /// <summary>
        /// Checks if a node exists within this tile at the specified grid position.
        /// </summary>
        /// <param name="x">The X index of the node to check.</param>
        /// <param name="y">The Y index of the node to check.</param>
        /// <returns>True if a node exists at the specified indices.</returns>
        public bool ContainsNode(int x, int y) {
            // Convert grid-space point to tile-space
            x -= X * L;
            y -= Y * L;
            var i = (L * y + x);
            
            // Return true if the index exists within this tile
            return i >= 0 && i < Nodes.Length;
        }
        
        /// <summary>
        /// Checks if a specified node exists within this tile.
        /// </summary>
        /// <param name="node"></param>
        /// <returns>True if a node exists at the specified indices.</returns>
        public bool ContainsNode(NavGridNode node) {
            return ContainsNode(node.X, node.Y);
        }
    }
}