using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using VektorLibrary.Collections;
using VektorLibrary.Pathfinding.NavGrid.AStar;
using VektorLibrary.Utility;
using Debug = UnityEngine.Debug;
using ThreadPriority = System.Threading.ThreadPriority;

namespace VektorLibrary.Pathfinding.NavGrid {
    public static class NavGridUtility {
        
        public static Dictionary<string, NavGrid> NavGrids = new Dictionary<string, NavGrid>();

        public static NavGrid GetSceneNavGrid() {
            // Check if the NavGrid is already loaded
            var sceneName = SceneManager.GetActiveScene().name;
            if (NavGrids.ContainsKey(sceneName))
                return NavGrids[sceneName];
            
            // Load the NavGrid for this scene
            var gridName = sceneName + "_NavGrid";
            var navGridAsset = Resources.Load<NavGridAsset>(gridName);
            var navGrid = navGridAsset.Deserialize();
            NavGrids.Add(sceneName, navGrid);
            return navGrid;
        }
        
        /// <summary>
        /// Asynchronously generates a mesh from the provided navgrid and returns the result.
        /// Designed to be awaited from the main Unity thread. Threaded work is handled internally.
        /// </summary>
        /// <param name="grid"></param>
        /// <returns>A mesh representation of the supplied grid.</returns>
        public static async Task<Mesh> MeshFromGridAsync (NavGrid grid) {
            // Create a new mesh instance and all required components
            var mesh = new Mesh {name = $"NavGrid: {grid.GetHashCode()}"};
            var rowLength = grid.Config.Dimension;
            var vertices = new Vector3[rowLength * rowLength];
            var uv = new Vector2[vertices.Length];
            var tangents = new Vector4[vertices.Length];
            var triangles = new int[(rowLength - 1) * (rowLength - 1) * 6];
            
            // Generate vertices and triangles on a worker thread and await the result
            await Task.Run(() => {
                try {
                    // Acquire a read lock on the navgrid object
                    grid.ThreadLock.AcquireReaderLock(-1);
                    
                    // Loop through each tile and gather it's vertices
                    for (var t = 0; t < grid.Tiles.Length; t++) {
                        for (var n = 0; n < grid.Tiles[t].Nodes.Length; n++) {
                            // Transform grid space to world space
                            var node = grid.Tiles[t].Nodes[n];
                            var vertex = node.WorldPosition;
                            
                            // Offset the Y position a bit to avoid Z-fighting
                            vertex.y += 1f; 

                            // Create mesh data from nodes
                            var index = grid.Config.Dimension * node.GridPosition.y + node.GridPosition.x;
                            vertices[index] = vertex;
                            uv[index] = new Vector2((float) node.GridPosition.x / (rowLength - 1), (float) node.GridPosition.y / (rowLength - 1));
                        }
                    }

                    // Create triangles from mesh data   
                    for (var i = 0; i < triangles.Length; i += 6) {
                        var row = (i / 6) / (rowLength - 1);
                        // First Triangle (Lower left of quad)
                        triangles[i] = i / 6 + row;                      // Bottom Left
                        triangles[i + 1] = (i / 6) + rowLength + row;    // Top Left
                        triangles[i + 2] = (i / 6) + 1 + row;            // Bottom Right

                        // Second Triangle (Upper right of quad)
                        triangles[i + 3] = (i / 6) + rowLength + row;        // Top Left
                        triangles[i + 4] = (i / 6) + rowLength + 1 + row;    // Top Right
                        triangles[i + 5] = (i / 6) + 1 + row;                // Bottom Right
                    }
                }
                catch (Exception ex) {
                    Debug.LogError("Exception occured during async mesh generation!");
                    Debug.LogException(ex);
                }
                finally {
                    // Ensure that the reader lock is released
                    if (grid.ThreadLock.IsReaderLockHeld)
                        grid.ThreadLock.ReleaseReaderLock();
                }
            });

            // Set the mesh data
            mesh.vertices = vertices;
            mesh.uv = uv;
            mesh.tangents = tangents;
                   
            // Set triangle data and recalculate normals
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
            
            // Return the mesh
            return mesh;
        }

        public static Texture2D TextureFromGrid(NavGrid grid, Color passable, Color impassable) {
            // Create a new texture with dimensions based on the grid
            var texture = new Texture2D(grid.Config.Dimension, grid.Config.Dimension);
            
            // Loop through each tile and set pixel color based on passability
            for (var t = 0; t < grid.Tiles.Length; t++) {
                for (var n = 0; n < grid.Tiles[t].Nodes.Length; n++) {
                    // Transform grid space to local tile space
                    var node = grid.Tiles[t].Nodes[n];
                
                    // Set pixel data according to node
                    texture.SetPixel(node.GridPosition.x, node.GridPosition.y, node.Passable ? passable : impassable);
                }
            }
            
            // Apply texture changes
            texture.filterMode = FilterMode.Point;
            texture.Apply();
            
            // Return the texture
            return texture;
        }
        
        /// <summary>
        /// Tries to calculate a path through a specified NavGrid using standard A*.
        /// </summary>
        /// <param name="grid">The NavGrid through which to calculate a path.</param>
        /// <param name="request">The path request.</param>
        public static async void CalculatePath(NavGrid grid, AStarRequest request) {
            // Create the path result object
            var result = new AStarResult();
            
            // Start a worker thread to calculate the path
            var sW = new Stopwatch();
            sW.Start();
            await Task.Run(() => {
                try {
                    // Acquire a read lock on the navgrid
                    grid.ThreadLock.AcquireReaderLock(request.Timeout);

                    result = AStarUtility.CalculatePath(grid, request);
                }
                catch (Exception ex) {
                    Debug.LogError("Exception occured during async A* path calculation!");
                    Debug.LogException(ex);
                }
                finally {
                    // Ensure that the reader lock is released
                    if (grid.ThreadLock.IsReaderLockHeld)
                        grid.ThreadLock.ReleaseReaderLock();
                }
            });
            sW.Stop();
            DebugReadout.UpdateField("A* Last", sW.ElapsedMilliseconds + "ms");
            
            // Invoke the callback from the request
            request.Callback?.Invoke(result);
        }
    }
}