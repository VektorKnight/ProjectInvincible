using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using VektorLibrary.Math;
using Debug = UnityEngine.Debug;

namespace VektorLibrary.Pathfinding.NavGrid {
    /// <summary>
    /// Represents a navigational space using nodes in a regular grid pattern.
    /// </summary>
    [Serializable] public class NavGrid : ScriptableObject {
        
        // Public Readonly: Read/Write Lock for Threading
        public ReaderWriterLockSlim ThreadLock { get; } = new ReaderWriterLockSlim();

        // Private: NavGrid Data
        public NavGridTile[] Tiles { get; private set; }    // Subdivisions of the larger grid for optimization

        // Property: Config
        public NavGridConfig Config { get; private set; }   // The configuration for this NavGrid
        
        // Property: Bounds
        public Rect Bounds { get; private set; }            // The world-space (x,z) bounds of the grid used for checking

        /// <summary>
        /// Creates a new NavGrid with the specified parameters.
        /// </summary>
        /// <param name="config">The configuration for this NavGrid.</param>
        public void Initialize(NavGridConfig config) {
            // Sanity Check: Size, Subdivision, and Units Per Node must be multiples of two
            if (config.Size % 2 != 0 || config.Subdivision % 2 != 0 || config.UnitsPerNode <= 0)
                throw new ArgumentException("Grid size and subdivision must be multiples of two!");
            
            // Sanity Check: Maximum Height & Steepness must be greater than zero
            if (config.MaxHeight <= 0 || config.MaxSteepness <= 0)
                throw new ArgumentException("Maximum height and steepness must be greater than zero!");
            
            // Set the config values
            Config = config;
            
            // Initialize the tile array
            Tiles = new NavGridTile[Config.Subdivision * Config.Subdivision];
            
            // Initialize the bounding rect
            Bounds = new Rect(Config.Origin.x, Config.Origin.y, Config.Size - 1, Config.Size - 1);
            
            // Initialize each tile in the array
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            for (var i = 0; i < Tiles.Length; i++) {
                Tiles[i] = new NavGridTile(i % Config.Subdivision, i / Config.Subdivision, Config.Size / Config.Subdivision / Config.UnitsPerNode);
                InitializeTile(Tiles[i]);
            }
            stopWatch.Stop();
            Debug.Log($"NavGrid generation with {Tiles[0].Nodes.Length * Tiles.Length} nodes took {stopWatch.ElapsedMilliseconds}ms!");
        }
        
        // Initializes all nodes within a tile.
        private void InitializeTile(NavGridTile tile) {
            // Iterate over all nodes checking for passability
            for (var i = 0; i < tile.Nodes.Length; i++) {
                // Calculate grid position
                var gridPosition = new Vector2Int((i % tile.L) + tile.X * tile.L,     // X-Index
                                             (i / tile.L) + tile.Y * tile.L);    // Y-Index
                
                // Calculate planar world position (x,z), (y) will be updated leter
                var worldPosition = new Vector3(Config.Origin.x + gridPosition.x * Config.UnitsPerNode,
                                                0f,
                                                Config.Origin.x + gridPosition.y * Config.UnitsPerNode);
                
                // Create a raycast to test for obstructions and geometry
                var checkRay = new Ray(new Vector3(worldPosition.x, Config.MaxHeight * 2f, worldPosition.z), Vector3.down);
                RaycastHit groundHit;
                RaycastHit obstacleHit;
                
                // Raycast on both masks to determine passability
                var groundCheck = Physics.Raycast(checkRay, out groundHit, Config.MaxHeight * 3f, Config.GroundMask);
                var obstacleCheck = Physics.Raycast(checkRay, out obstacleHit, Config.MaxHeight * 3f, Config.ObstacleMask);
                
                // Calculate statistics for the ground if possible
                worldPosition.y = groundCheck ? groundHit.point.y + Config.Origin.y : obstacleCheck ? obstacleHit.point.y : 0f;
                var groundSlope = groundCheck ? 1f - Vector3.Dot(groundHit.normal, Vector3.up) : obstacleCheck ? 1f - Vector3.Dot(obstacleHit.normal, Vector3.up) : 0f;
                var passable = !obstacleCheck && groundCheck && (worldPosition.y < Config.MaxHeight && groundSlope < Config.MaxSteepness);
                
                // Initialize the node with the relevant data
                tile.Nodes[i] = new NavGridNode(gridPosition, worldPosition, passable, groundSlope);
            }
        }

        /// <summary>
        /// Returns the NavGridNode at the specified X,Y indices.
        /// </summary>
        /// <param name="gridPos">The grid-space position.</param>
        public NavGridNode GetNode(Vector2Int gridPos) {
            // Fetch the tile containing specified pair
            var tile = GetTile(gridPos);
            
            // Fetch the node from the tile
            return tile.GetNode(gridPos);
        }
        
        /// <summary>
        /// Returns the NavGridTile containing the specified grid position.
        /// </summary>
        /// <param name="gridPos">The grid-space position.</param>
        public NavGridTile GetTile(Vector2Int gridPos) {
            gridPos.x *= Config.Subdivision / Config.Dimension;
            gridPos.y *= Config.Subdivision / Config.Dimension;
            return Tiles[Config.Subdivision * gridPos.y + gridPos.x];
        }
        
        /// <summary>
        /// Checks if a world-space point lies within the bounds of this NavMesh.
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public bool ContainsPoint(Vector3 point) {
            var pos2D = new Vector2(point.x, point.z);
            return Bounds.Contains(pos2D);
        }
        
        /// <summary>
        /// Converts a point on the NavGrid to a world position.
        /// </summary>
        /// <param name="gridPos">The grid-space position to convert.</param>
        public Vector3 GridToWorld(Vector2Int gridPos) {
            return Config.Origin + new Vector3((float) gridPos.x * Config.UnitsPerNode, 
                       0f, 
                       (float) gridPos.y * Config.UnitsPerNode);
        }
        
        /// <summary>
        /// Converts a world-space psotion to a grid-space position.
        /// </summary>
        /// <param name="pos"></param>
        /// <returns>The grid-space conversion of the point.</returns>
        public Vector2Int WorldToGrid(Vector3 pos) {
            return new Vector2Int(Mathf.RoundToInt((pos.x - Config.Origin.x) / Config.UnitsPerNode),
                Mathf.RoundToInt((pos.z - Config.Origin.z) / Config.UnitsPerNode));
        }
        
        /// <summary>
        /// Returns the closest NavGridNode containing the specified world-space position.
        /// </summary>
        /// <param name="pos">The world-space point.</param>
        public NavGridNode WorldToNode(Vector3 pos) {
            // Make sure the point is within the bounds of the grid
            if (!ContainsPoint(pos)) 
                throw new IndexOutOfRangeException("The specified point lies outside the bounds of this NavGrid!");
            
            return GetNode(WorldToGrid(pos));
        }
        
        /// <summary>
        /// Returns the closest NavGridTile containing the specified point;
        /// </summary>
        /// <param name="pos">The world-space point.</param>
        public NavGridTile WorldToTile(Vector3 pos) {
            // Make sure the point is within the bounds of the grid
            if (!ContainsPoint(pos)) 
                throw new IndexOutOfRangeException("The specified point lies outside the bounds of this NavGrid!");
            
            return GetTile(WorldToGrid(pos));
        }
    }
}