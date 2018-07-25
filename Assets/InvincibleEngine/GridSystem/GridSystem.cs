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

    /// <summary>
    ///Generates a grid of points around the map that can be used to determine leagal building locations
    /// </summary>
    public class GridSystem {

        //Scale of grid, scale of 1 = 1 grid point to 1 meter(s)
        //                        2 = 1 grid point to 2 meter(s)
        //                        8 = 1 grid point to 8 meter(s)
        private int GridScale = 8;

        //Holds all grid points
        private Dictionary<UInt32, GridPoint> GridPoints = new Dictionary<uint, GridPoint>();

        //grid point data
        public class GridPoint {

            //Occupying Object
            public GameObject ObjectOccupied;

            //Enabled flags
            public bool Buildable;

            //Height of terrain at given grid point
            public float Height;

            //World position
            public Vector3 WorldPosition;

            //get open status, if the node is marked as buildable and no object exists there, it is open
            public bool IsOpen() {
                if(Buildable & ObjectOccupied == null) {
                    return true;
                }
                else {
                    return false;
                }
            }

        }

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

                    //Sample Navmesh, set flags
                    allowed = NavMesh.SamplePosition(new Vector3(u * GridScale, n.Height, v * GridScale), out hit, GridScale, NavMesh.AllAreas);
                    n.Buildable = allowed ? true : false;
                    if (allowed) {
                        a++;
                    }
                    else {
                        b++;
                    }
                    

                    //Append to node list
                    GridPoints.Add((UInt32)((u * GridScale) << 16 | (v * GridScale)), n);

                }
            }
        }

        //Combines two intergers on a bit level
        private UInt32 MergeInt(int a, int b) {
            return (uint)(a << 16 | b);
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
            return GridPoints[MergeInt(x,z)];
        }
    }
}
