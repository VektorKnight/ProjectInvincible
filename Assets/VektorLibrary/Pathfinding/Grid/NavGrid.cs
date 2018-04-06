using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Newtonsoft.Json;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace VektorLibrary.Pathfinding.Grid {
    /// <summary>
    /// Represents a navigational space using nodes in a regular grid pattern.
    /// </summary>
    [Serializable] public class NavGrid {
        
        // Property: Read/Write Lock for Threading
        [JsonIgnore]
        public ReaderWriterLock Lock { get; }
        
        // Property: Config
        public NavGridConfig Config { get; }   // The configuration for this NavGrid
        
        // Property: Bounds
        public Rect WorldBounds { get; }       // The world-space (x,z) bounds of the grid used for checking
        
        // Private: Nodes & Data
        public NavGridNode[] Nodes { get; }

        public NavGridTile[] Tiles { get; }

        // Helpers: Get Node (ID / (X,Y) / V2I)
        public NavGridNode Node(int id)
            => Exists(id % Config.Dimension, id / Config.Dimension) ? Nodes[id] : null;
        public NavGridNode Node(int x, int y) 
            => Exists(x, y) ? Nodes[Config.Dimension * y + x] : null;
        public NavGridNode Node(Vector2Int p) => Node(p.x, p.y);

        // Helpers: Exists
        public bool Exists(int x, int y) 
            => Config.Dimension * y + x < Nodes.Length && (x >= 0 && y >=0);
        public bool Exists(Vector2Int p) => Exists(p.x, p.y);
        
        // Helpers: Passable
        public bool Passable(int x, int y)
            => Exists(x, y) && Node(x, y).Passable;
        public bool Passable(Vector2Int p)
            => Passable(p.x, p.y);

        /// <summary>
        /// Creates a new NavGrid with the specified parameters.
        /// </summary>
        /// <param name="config">The configuration for this NavGrid.</param>
        public NavGrid (NavGridConfig config) {
            // Sanity Check: Size, Subdivision, and Units Per Node must be multiples of two
            if (config.Size % 2 != 0 || config.Subdivision % 2 != 0 || config.UnitsPerNode <= 0)
                throw new ArgumentException("Grid size and subdivision must be multiples of two!");
            
            // Sanity Check: Maximum Height & Steepness must be greater than zero
            if (config.MaxHeight <= 0 || config.MaxSteepness <= 0)
                throw new ArgumentException("Maximum height and steepness must be greater than zero!");
            
            // Set the config values
            Lock = new ReaderWriterLock();
            Config = config;
            
            // Initialize the node, tile, and data collections
            Nodes = new NavGridNode[Config.NodeCount];
            Tiles = new NavGridTile[Config.Subdivision * Config.Subdivision];
            
            // Initialize the bounding rect
            WorldBounds = new Rect(Config.Origin.x, Config.Origin.y, Config.Size - 1, Config.Size - 1);
            
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
                // Calculate node indices belonging to this tile
                var nodeX = (i % tile.L) + tile.X * tile.L;
                var nodeY = (i / tile.L) + tile.Y * tile.L;
                var nodeI = Config.Dimension * nodeY + nodeX;
                tile.Nodes[i] = nodeI;

                
                // Calculate planar world position (x,z), (y) will be updated leter
                var worldPosition = new Vector3(Config.Origin.x + nodeX * Config.UnitsPerNode,
                                                0f,
                                                Config.Origin.x + nodeY * Config.UnitsPerNode);
                
                // Create a raycast to test for obstructions and geometry
                var checkRay = new Ray(new Vector3(worldPosition.x, Config.MaxHeight * 2f, worldPosition.z), Vector3.down);
                RaycastHit groundHit;
                RaycastHit obstacleHit;
                
                // Raycast on both masks to determine passability
                var groundCheck = Physics.Raycast(checkRay, out groundHit, Config.MaxHeight * 3f, Config.GroundMask);
                var obstacleCheck = Physics.Raycast(checkRay, out obstacleHit, Config.MaxHeight * 3f, Config.ObstacleMask);
                
                // Calculate statistics for the ground if possible
                worldPosition.y = groundCheck ? groundHit.point.y + Config.Origin.y : obstacleCheck ? obstacleHit.point.y : 0f;
                var normal = groundCheck ? groundHit.normal : Vector3.up;
                var passable = !obstacleCheck && groundCheck && (worldPosition.y < Config.MaxHeight);
                
                // Initialize the node with the relevant data
                Nodes[nodeI] = new NavGridNode(nodeI, nodeX, nodeY, worldPosition, normal, passable);
            }
        }

        /// <summary>
        /// Returns the NavGridTile containing the specified grid position.
        /// </summary>
        /// <param name="gridPos">The grid-space position.</param>
        public NavGridTile GetTile(Vector2Int gridPos) {
            var tileX = (gridPos.x * Config.Subdivision) / Config.Dimension;
            var tileY = (gridPos.y *Config.Subdivision) / Config.Dimension;
            return Tiles[Config.Subdivision * tileY + tileX];
        }
        
        /// <summary>
        /// Returns any valid neighbors of the specified node.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public List<NavGridNode> GetNeighbors(int x, int y) {
            var neighbours = new List<NavGridNode>();

            // Top Left
            if (Exists(x - 1, y + 1) && Passable(x - 1, y + 1))
                neighbours.Add(Node(x - 1, y + 1));
            
            // Top Center
            if (Exists(x, y + 1) && Passable(x, y + 1))
                neighbours.Add(Node(x, y + 1));
            
            // Top Right
            if (Exists(x + 1, y + 1) && Passable(x + 1, y + 1))
                neighbours.Add(Node(x + 1, y + 1));
            
            // Right Center
            if (Exists(x + 1, y) && Passable(x + 1, y))
                neighbours.Add(Node(x + 1, y));
            
            // Bottom Right
            if (Exists(x + 1, y - 1) && Passable(x + 1, y - 1))
                neighbours.Add(Node(x + 1, y - 1));
            
            // Bottom Center
            if (Exists(x, y - 1) && Passable(x, y - 1))
                neighbours.Add(Node(x, y - 1));
            
            // Bottom Left
            if (Exists(x - 1, y - 1) && Passable(x - 1, y - 1))
                neighbours.Add(Node(x - 1, y - 1));
            
            // Left Center
            if (Exists(x - 1, y) && Passable(x - 1, y))
                neighbours.Add(Node(x - 1, y));
            
            return neighbours;
        }

        public List<NavGridNode> GetNeighbors(Vector2Int point) {
            return GetNeighbors(point.x, point.y);
        }
        
        /// <summary>
        /// Checks if a world-space point lies within the bounds of this NavMesh.
        /// </summary>
        /// <param name="point"></param>
        public bool ContainsPoint(Vector3 point) {
            var pos2D = new Vector2(point.x, point.z);
            return WorldBounds.Contains(pos2D);
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

        public NavGridNode WorldToNode(Vector3 pos) {
            var gridPos = WorldToGrid(pos);
            return Node(gridPos.x, gridPos.y);
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