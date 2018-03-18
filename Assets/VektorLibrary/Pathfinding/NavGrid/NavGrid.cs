using System;
using System.Diagnostics;
using System.Linq;
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
        public NavGridTile[] Tiles { get; }    // Subdivisions of the larger grid for optimization

        // Property: Config
        public NavGridConfig Config { get; }   // The configuration for this NavGrid

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
            
            // Initialize the tile array
            Tiles = new NavGridTile[Config.Subdivision * Config.Subdivision];
            
            // Initialize each tile in the array
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            for (var i = 0; i < Tiles.Length; i++) {
                Tiles[i] = new NavGridTile(i % Config.Subdivision, i / Config.Subdivision, Config.Size / Config.Subdivision / Config.UnitsPerNode);
                InitializeTile(Tiles[i]);
            }
            stopWatch.Stop();
            Debug.Log($"NavGrid generation took {stopWatch.ElapsedMilliseconds}ms!");
        }
        
        // Initializes all nodes within a tile.
        private void InitializeTile(NavGridTile tile) {
            // Iterate over all nodes checking for passability
            for (var i = 0; i < tile.Nodes.Length; i++) {
                // Calculate current node position
                var x = (i % tile.L) + tile.X * tile.L;
                var y = (i / tile.L) + tile.Y * tile.L;
                var origin = GridToWorld(x, y);
                
                // Create a raycast to test for obstructions and geometry
                var checkRay = new Ray(new Vector3(origin.x, Config.MaxHeight * 2f, origin.z), Vector3.down);
                RaycastHit groundHit;
                RaycastHit obstacleHit;
                
                // Raycast on both masks to determine passability
                var groundCheck = Physics.Raycast(checkRay, out groundHit, Config.MaxHeight * 3f, Config.GroundMask);
                var obstacleCheck = Physics.Raycast(checkRay, out obstacleHit, Config.MaxHeight * 3f, Config.ObstacleMask);
                
                // Calculate statistics for the ground if possible
                var groundHeight = groundCheck ? groundHit.point.y + Config.Origin.y : 0f;
                var groundSlope = groundCheck ? 1f - Vector3.Dot(groundHit.normal, Vector3.up) : 0f;
                var passable = !obstacleCheck; //&& groundCheck && (groundHeight < Config.MaxHeight && groundSlope < Config.MaxSteepness);
                
                // Initialize the node with the relevant data
                tile.Nodes[i] = new NavGridNode(x, y, passable, groundHeight, groundSlope);
            }
        }

        /// <summary>
        /// Returns the NavGridNode at the specified X,Y indices.
        /// </summary>
        /// <param name="x">The X index of the node.</param>
        /// <param name="y">The Y index of the node.</param>
        public NavGridNode GetNode(int x, int y) {
            // Fetch the tile containing specified pair
            var tile = GetTile(x, y);
            
            // Fetch the node from the tile
            return tile.GetNode(x, y);
        }
        
        /// <summary>
        /// Returns the NavGridTile at the specified X,Y indices.
        /// </summary>
        /// <param name="x">The X index of the tile.</param>
        /// <param name="y">The Y index of the tile.</param>
        public NavGridTile GetTile(int x, int y) {
            x *= Config.Subdivision / Config.Dimension;
            y *= Config.Subdivision / Config.Dimension;
            return Tiles[Config.Subdivision * y + x];
        }
        
        /// <summary>
        /// Converts a point on the NavGrid to a world position.
        /// </summary>
        /// <param name="x">The X index of the node.</param>
        /// <param name="y">The Y index of the node.</param>
        public Vector3 GridToWorld(int x, int y) {
            return Config.Origin + new Vector3((float) x * Config.UnitsPerNode, 
                       0f, 
                       (float) y * Config.UnitsPerNode);
        }
        
        /// <summary>
        /// Converts a node on the NavGrid to a world position.
        /// </summary>
        /// <param name="node">The node to convert to a world position.</param>
        public Vector3 NodeToWorld(NavGridNode node) {
            return Config.Origin + new Vector3((float) node.X * Config.UnitsPerNode, 
                                               node.Height, 
                                               (float) node.Y * Config.UnitsPerNode);
        }
        
        /// <summary>
        /// Returns the closest NavGridNode containing the specified world-space position.
        /// </summary>
        /// <param name="pos">The world-space point.</param>
        public NavGridNode WorldToNode(Vector3 pos) {
            var x = Mathf.RoundToInt((pos.x - Config.Origin.x) / Config.UnitsPerNode);
            var y = Mathf.RoundToInt((pos.z - Config.Origin.z) / Config.UnitsPerNode);
            return GetNode(x, y);
        }
        
        /// <summary>
        /// Returns the closest NavGridTile containing the specified point;
        /// </summary>
        /// <param name="pos">The world-space point.</param>
        public NavGridTile WorldToTile(Vector3 pos) {
            var x = Mathf.FloorToInt((pos.x - Config.Origin.x) * Config.Subdivision / Config.Dimension);
            var y = Mathf.FloorToInt((pos.z - Config.Origin.z) * Config.Subdivision / Config.Dimension);
            return GetTile(x, y);
        }
    }
}