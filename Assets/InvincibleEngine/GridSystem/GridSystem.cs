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

        //Scale of grid, scale of 2 = 1 grid point to 2 meter(s)
        private int GridScale = 8;

        //Holds all grid points
        private Dictionary<Vector2, GridPoint> GridPoints = new Dictionary<Vector2, GridPoint>();

        //grid point data
        public struct GridPoint {

            //Enabled flags
            public GridFlags GridFlags;

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

            //Key markers
            Vector2 activekey = new Vector2(0, 0);

            //navmesh hit
            NavMeshHit hit;

            //Querry navmesh hit
            bool allowed;

            //populate left to right then up
            for (int v = 0; v < GridHeight; v++) {

                //Set row key
                activekey.y = v;

                for (int u = 0; u < GridWidth; u++) {

                    //set column key
                    activekey.x = u;

                    //Generate point at given key
                    GridPoint n = new GridPoint();

                    //Sample Navmesh, set flags
                    allowed = NavMesh.SamplePosition(new Vector3(u * GridScale, Terrain.activeTerrain.terrainData.GetHeight(u * GridScale, v * GridScale), v * GridScale), out hit, GridScale, NavMesh.AllAreas);
                    n.GridFlags = allowed ? GridFlags.Open : GridFlags.Occupied;
                    if(allowed) {
                        a++;
                    }
                    else {
                        b++;
                    }

                    //Append to node list
                    GridPoints.Add(activekey, n);

                }
            }

            Debug.Log($"Generated grid with {a} good nodes and {b} inactive nodes");
        }
    }
}
