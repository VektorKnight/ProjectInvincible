using System.Linq;
using InvincibleEngine.VektorLibrary.Utility;
using UnityEngine;

namespace InvincibleEngine.VektorLibrary.AI.Utility {
    public static class AIUtility {
        /// <summary>
        /// Calculates a movement delta (Vector3) based on provided values.
        /// </summary>
        /// <param name="current"></param>
        /// <param name="target"></param>
        /// <param name="maxDelta">Maximum delta to move by.</param>
        /// <param name="sqrEpsilon">Squared minimum distance to reach the target.</param>
        /// <param name="deltaTime">Time since the last call to this function.</param>
        /// <param name="moveDelta">The calculated move delta.</param>
        /// <returns>True if within epsilon of the target point, false otherwise.</returns>
        public static bool MoveTowardsPoint(Vector3 current, Vector3 target, float maxDelta, float sqrEpsilon, float deltaTime, out Vector3 moveDelta) {
            var targetVector = Vector3.Normalize(target - current);
            var sqrTargetDistance = VektorUtility.SqrPlanarDistance(current, target, Vector3.up);
			
            // Move towards the current destination sqrtargetDistance <= _pathEpsilon
            if (sqrTargetDistance > sqrEpsilon) {
                // Calculate movement delta based on the remaining distance to target
                var moveVector = targetVector * maxDelta * deltaTime;
                moveDelta = moveVector.sqrMagnitude > sqrTargetDistance ? targetVector * Mathf.Sqrt(sqrTargetDistance) : moveVector;

                // Return false and set the moveVector
                return false;
            }
			
            // Return true if we are within sqrEpsilon of the target
            moveDelta = Vector3.zero;
            return true;
        }
		
        // Helper: Scan for Targets
        public static bool ScanForObjects(Vector3 origin, float radius, LayerMask scanLayers, out GameObject[] targets) {
            var objects = Physics.OverlapSphere(origin, radius, scanLayers);
            if (objects.Length > 0) {
                targets = objects.Select(obj => obj.gameObject).ToArray();
                return true;
            }
            
            targets = null;
            return false;
        }
    }
}