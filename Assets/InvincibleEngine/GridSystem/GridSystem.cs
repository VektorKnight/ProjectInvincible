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

        public class GridData {

            //grid point data
            struct GridPoint {
                
                //Enabled/disabled grid point
                public bool State;              
            }
        }

        /// <summary>
        /// Saves current grid data to a local file that can be loaded
        /// </summary>
        public void SaveGridToFile() {


        }

        /// <summary>
        /// Returns the grid from the local file
        /// </summary>
        /// <returns></returns>
        public GridData GetGridFromFile() {

            return new GridData();
        }
    }
}
