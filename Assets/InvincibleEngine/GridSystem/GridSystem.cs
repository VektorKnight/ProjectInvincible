using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InvincibleEngine;

namespace InvincibleEngine {

    /// <summary>
    /// Interface for locking objects to a set grid
    /// By default, the grid is locked to 1 grid point = 1 unity meter
    /// </summary>
    public class GridSystem {

        //Grid Dimensions
        public int GridSizeX, GridSizeY;
        
        //Stores node occupation
        public Dictionary<Vector2, bool> Nodes = new Dictionary<Vector2, bool>();

        //Converts a world point into grid space
        public Vector3 WorldToGridPoint(Vector3 point) {

            //Find x,z coordinates
            Vector3 gridPoint = new Vector3(Mathf.RoundToInt(point.x), point.y, Mathf.RoundToInt(point.z));

            //Raycast to get height at location of grid
            RaycastHit hit;
            if (Physics.Raycast(gridPoint += Vector3.up, Vector3.down, out hit, 5, 1 << 8)) {
                gridPoint.y = hit.point.y;
            }

            //return point
            return gridPoint;
        }

        //Sets occupation of nodes
        public void SetNodeOccupy(Vector2[] nodes, Vector2 position, bool value) {
            for (int i = 0; i < nodes.Length; i++) {
                Nodes[nodes[i]] = value;
            }
        }

        //Returns false if any nodes have occupied spots
        public bool GetNodeOccupy(Vector2[] nodes, Vector2 position) {

            for (int i = 0; i < nodes.Length; i++) {

                //if key does not exist, create it
                if (!Nodes.ContainsKey(nodes[i] + position)) {

                }
                if (!Nodes[nodes[i] + position]) {
                    return false;
                }
            }

            //return true after all node checks
            return true;
        }
    }
}
