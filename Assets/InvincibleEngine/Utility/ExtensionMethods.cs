using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InvincibleEngine;
namespace InvincibleEngine {
    public static class ExtensionMethods {
        /// <summary>
        /// Checks to ensure that all grid points in the collection are open
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static bool AreOpen(this IEnumerable<GridPoint> input) {
            foreach (GridPoint n in input) {
                if (n.IsOpen()) {
                    continue;
                }
                else {
                    return false;
                }
            }
            return true;
        }
    }
}
