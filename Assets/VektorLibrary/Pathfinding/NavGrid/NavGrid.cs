using System;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using VektorLibrary.Math;
using Debug = UnityEngine.Debug;

namespace VektorLibrary.Pathfinding.NavGrid {
    /// <summary>
    /// Represents a navigational space using nodes in a regular grid pattern.
    /// </summary>
    [Serializable] public class NavGrid {
        // Private: NavGrid Data
        public NavGridNode[] Nodes { get; private set; }    // The raw collection of nodes within the grid
        public NavGridTile[] Tiles { get; private set; }    // Subdivisions of the larger grid for optimization

        // Property: Config
        public NavGridConfig Config { get; }                // The configuration for this NavGrid

        /// <summary>
        /// Creates a new NavGrid with the specified parameters.
        /// </summary>
        /// <param name="config">The configuration for this NavGrid.</param>
        public NavGrid(NavGridConfig config) {
            // Sanity Check: Dimension, Resolution, and Subdivision must be powers of two
            if (!VektorMath.IsPowerOfTwo(config.Size) || !VektorMath.IsPowerOfTwo(config.UnitsPerNode) || !VektorMath.IsPowerOfTwo(config.Subdivision))
                throw new ArgumentException("Grid dimension and subdivision must be powers of two!");
            
            // Sanity Check: Maximum Height & Steepness must be greater than zero
            if (config.MaxHeight <= 0 || config.MaxSteepness <= 0)
                throw new ArgumentException("Maximum height and steepness must be greater than zero!");
            
            // Set the config values
            Config = config;
            
            // Generate the NavGrid
            GenerateGrid();
        }
        
        // Generate the NavGrid populating all nodes and partitioning them into tiles
        private void GenerateGrid() {
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            // Initialize the Node and Tile arrays
            Nodes = new NavGridNode[Config.Dimension * Config.Dimension];
            Tiles = new NavGridTile[Config.Subdivision * Config.Subdivision];
            
            // Iterate over all nodes checking for passability
            for (var i = 0; i < Nodes.Length; i++) {
                // Calculate current node position
                var x = i % Config.Dimension;
                var y = i / Config.Dimension;
                var origin = GridToWorld(this, x, y);
                
                // Create a raycast to test for obstructions and geometry
                var checkRay = new Ray(origin + Vector3.up * 2f * Config.MaxHeight, Vector3.down);
                RaycastHit groundHit;
                RaycastHit obstacleHit;
                
                // Raycast on both masks to determine passability
                var groundCheck = Physics.Raycast(checkRay, out groundHit, Config.MaxHeight * 2, Config.GroundMask);
                var obstacleCheck = Physics.Raycast(checkRay, out obstacleHit, Config.MaxHeight * 2, Config.ObstacleMask);
                
                // Calculate statistics for the ground if possible
                var groundHeight = groundCheck ? groundHit.point.y + Config.Origin.y : 0f;
                var groundSlope = groundCheck ? 1f - Vector3.Dot(groundHit.normal, Vector3.up) : 0f;
                var passable = !obstacleCheck && (groundHeight < Config.MaxHeight && groundSlope < Config.MaxSteepness);
                
                // Initialize the node with the relevant data
                Nodes[i] = new NavGridNode(x, y, passable, groundHeight, groundSlope);
            }
            stopWatch.Stop();
            Debug.Log($"NavGrid with dimension {Config.Size} took {stopWatch.ElapsedMilliseconds}ms to generate!");
        }

        /// <summary>
        /// Returns the NavGridNode at the specified X,Y indices.
        /// </summary>
        /// <param name="grid">NavGrid reference.</param>
        /// <param name="x">The X index of the node.</param>
        /// <param name="y">The Y index of the node.</param>
        public static NavGridNode GetNode(NavGrid grid, int x, int y) {
            return grid.Nodes[grid.Config.Dimension * y + x];
        }
        
        /// <summary>
        /// Returns the NavGridTile at the specified X,Y indices.
        /// </summary>
        /// <param name="grid">NavGrid reference.</param>
        /// <param name="x">The X index of the tile.</param>
        /// <param name="y">The Y index of the tile.</param>
        public static NavGridTile GetTile(NavGrid grid, int x, int y) {
            return grid.Tiles[grid.Config.Subdivision * y + x];
        }
        
        /// <summary>
        /// Converts a point on the NavGrid to a world position.
        /// </summary>
        /// <param name="grid">NavGrid reference.</param>
        /// <param name="x">The X index of the node.</param>
        /// <param name="y">The Y index of the node.</param>
        public static Vector3 GridToWorld(NavGrid grid, int x, int y) {
            var node = GetNode(grid, x, y);
            return grid.Config.Origin + new Vector3((float) x * grid.Config.UnitsPerNode, node.Height, (float) y * grid.Config.UnitsPerNode);
        }
        
        /// <summary>
        /// Converts a node on the NavGrid to a world position.
        /// </summary>
        /// <param name="grid">NavGrid reference.</param>
        /// <param name="node">The node to convert to a world position.</param>
        public static Vector3 NodeToWorld(NavGrid grid, NavGridNode node) {
            return grid.Config.Origin + new Vector3((float) node.X * grid.Config.UnitsPerNode, node.Height, (float) node.Y * grid.Config.UnitsPerNode);
        }
        
        /// <summary>
        /// Returns the closest NavGridNode containing the specified world-space position.
        /// </summary>
        /// <param name="grid">NavGrid reference.</param>
        /// <param name="pos">The world-space point.</param>
        public static NavGridNode WorldToNode(NavGrid grid, Vector3 pos) {
            var x = Mathf.RoundToInt((pos.x - grid.Config.Origin.x) / grid.Config.UnitsPerNode);
            var y = Mathf.RoundToInt((pos.z - grid.Config.Origin.z) / grid.Config.UnitsPerNode);
            return GetNode(grid, x, y);
        }
        
        /// <summary>
        /// Returns the NavGridTile containing the specified point.
        /// </summary>
        /// <param name="grid">NavGrid reference.</param>
        /// <param name="x">The X index of the node.</param>
        /// <param name="y">The Y index of the node.</param>
        /// <returns></returns>
        public static NavGridTile NodeToTile(NavGrid grid, int x, int y) {
            var tX = Mathf.FloorToInt((float)x * grid.Config.Subdivision / grid.Config.Dimension);
            var tY = Mathf.FloorToInt((float)y * grid.Config.Subdivision / grid.Config.Dimension);
            return GetTile(grid, tX, tY);
        }
        
        /// <summary>
        /// Returns the closest NavGridTile containing the specified point;
        /// </summary>
        /// <param name="grid">NavGrid reference.</param>
        /// <param name="pos">The world-space point.</param>
        public static NavGridTile WorldToTile(NavGrid grid, Vector3 pos) {
            var x = Mathf.FloorToInt((pos.x - grid.Config.Origin.x) * grid.Config.Subdivision / grid.Config.Dimension);
            var y = Mathf.FloorToInt((pos.z - grid.Config.Origin.z) * grid.Config.Subdivision / grid.Config.Dimension);
            return GetTile(grid, x, y);
        }
    }
}