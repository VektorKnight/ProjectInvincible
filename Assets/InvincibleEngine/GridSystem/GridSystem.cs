using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using InvincibleEngine;
using System;

namespace InvincibleEngine {

    //Holds flags for grid point information
    [Flags]
    public enum GridFlags {
        Open = 1,
        Occupied = 2
    }
    //grid point data
    public class GridPoint {

        //Occupying Object
        public bool Occupied;

        //Enabled flags
        public bool Buildable;

        //Height of terrain at given grid point
        public float Height;

        //World position
        public Vector3 WorldPosition;

        //Grid index
        public Vector2Int GridIndex;

        //get open status, if the node is marked as buildable and no object exists there, it is open
        public bool IsOpen() {
            if (Buildable & !Occupied) {
                return true;
            }
            else {
                return false;
            }
        }

    }
    /// <summary>
    ///Generates a grid of points around the map that can be used to determine leagal building locations
    /// </summary>
    public class GridSystem {

        //Scale of grid, scale of 1 = 1 grid point to 1 meter(s)
        //                        2 = 1 grid point to 2 meter(s)
        //                        8 = 1 grid point to 8 meter(s)
        public static readonly int GridScale = 4;

        //Holds all grid points
        public Dictionary<Vector2Int, GridPoint> GridPoints = new Dictionary<Vector2Int, GridPoint>();
        
        /// <summary>
        /// Generates a grid with current terrain and NavMesh data
        /// </summary>
        public void GenerateGrid() {

            //Fetch Active Terrain
            TerrainData activeTerrain = Terrain.activeTerrain.terrainData;
            Debug.Log(activeTerrain.name);

            //Get dimensions of grid
            Vector3 terrainDimensions = activeTerrain.size;

            //Divide and remain
            int rem, a = new int(), b = new int();
            int GridWidth = Math.DivRem(Mathf.CeilToInt(terrainDimensions.x), GridScale, out rem);
            int GridHeight = Math.DivRem(Mathf.CeilToInt(terrainDimensions.z), GridScale, out rem);

            //--------------------------------------
            //Populate dictionary with grid points
            //--------------------------------------

            //navmesh hit
            NavMeshHit hit;

            //Querry navmesh hit
            bool allowed;

            //populate left to right then up
            for (int v = 0; v < GridHeight; v++) {

                for (int u = 0; u < GridWidth; u++) {

                    //Generate point at given key
                    GridPoint n = new GridPoint();

                    //Assign height based on terrain
                    n.Height = activeTerrain.GetHeight(u * GridScale, v * GridScale);

                    //Set world position for easy retrieval
                    n.WorldPosition = new Vector3(u * GridScale, n.Height, v * GridScale);
                    n.GridIndex = new Vector2Int(u * GridScale, v *GridScale);

                    //Sample Navmesh, set flags
                    allowed = NavMesh.SamplePosition(new Vector3(u * GridScale, n.Height, v * GridScale), out hit, GridScale / 2, NavMesh.AllAreas);
                    n.Buildable = allowed ? true : false;
                    if (allowed) {
                        a++;
                    }
                    else {
                        b++;
                    }

                    //Append to node list
                    GridPoints.Add(new Vector2Int((u * GridScale),(v * GridScale)), n);

                }

            }
            //Log grid sum
            Debug.Log($"Generate grid with {a} good nodes and {b} bad nodes");
        }
        
        /// <summary>
        /// Returns the nearest grid point that cooresponds to a world point.
        /// For now this will just clamp values in increments according to grid scale
        /// </summary>
        /// <returns></returns>
        public GridPoint WorldToGridPoint(Vector3 point) {
            int x, z;
            x = (int)Math.Round(point.x / GridScale) * GridScale;
            z = (int)Math.Round(point.z / GridScale) * GridScale;
            return GridPoints[new Vector2Int(x, z)];
        }

        /// <summary>
        /// returns all points within a specific origin 
        /// </summary>
        /// <param name="origin">Center Point Origin</param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public GridPoint[] WorldToGridPoints(Vector3 origin, int width, int height) {

            origin.x -= width;
            origin.z -= height;

            GridPoint _origin = WorldToGridPoint(origin);

            List<GridPoint> returns = new List<GridPoint>();           

            width *= 2;
            height *= 2;

            var gridX= _origin.GridIndex.x;
            var gridY = _origin.GridIndex.y;

            for (int
                u = gridX;
                u < (width + gridX);
                u += GridScale) {

                for (int
                    v = gridY;
                    v < (height + gridY);
                    v += GridScale) {

                    try {
                        returns.Add(GridPoints[new Vector2Int(u, v)]);
                    }
                    catch (Exception e) {
                        Debug.Log(e);
                    }
                }
            }

            return returns.ToArray();
        }

        /// <summary>
        /// Call to occupy grid points with a structure, done on instantiation in game
        /// </summary>
        /// <param name="origin">Original grid point</param>
        /// <param name="width">Width in grid points</param>
        /// <param name="height">Height in grid points</param>
        public void OnOccupyGrid(GridPoint[] gridPoints) {
            foreach(GridPoint n in gridPoints) {
                GridPoints[n.GridIndex].Occupied = true;
            }
        }

        /// <summary>
        /// Call on grid unoccupy, when a structure is destroyed or removed
        /// </summary>
        /// <param name="origin">Original grid point</param>
        /// <param name="width">Width in grid points</param>
        /// <param name="height">Height in grid points</param>
        public void OnVacateGrid(GridPoint origin, int width, int height) {

        }
    }
}
