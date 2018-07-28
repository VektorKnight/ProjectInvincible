using InvincibleEngine.UnitFramework.Components;
using InvincibleEngine.UnitFramework.Enums;
using UnityEngine;

namespace InvincibleEngine.UnitFramework.Utility {
    /// <summary>
    /// Used to scan for and retrieve valid targets for combat.
    /// </summary>
    public static class TargetScanner {
        // Set this to some power of two larger than the maximum expected size needed
        private const int BUFFER_SIZE = 8192;
        
        // Buffer for raw physics data
        private static readonly Collider[] ScanBuffer = new Collider[BUFFER_SIZE];
        
        // Scan for nearest target
        public static UnitBehavior ScanForNearestTarget(Vector3 origin, float radius, LayerMask scanMask) {            
            // Fetch all nearby colliders on the specified layers
            var bufferTail = Physics.OverlapSphereNonAlloc(origin, radius, ScanBuffer, scanMask);
            
            // Iterate over the buffer and find the nearest target
            UnitBehavior nearestTarget = null;
            var sqrShortestDistance = float.MaxValue;
            
            for (var i = 0; i < bufferTail; i++) {
                // Attempt to acquire the UnitBehavior component of the current object
                var unitBehavior = ScanBuffer[i].GetComponent<UnitBehavior>();
                
                // Skip this object if it does not have a UnitBehavior
                if (unitBehavior == null) continue;
                
                // Calculate sqr distance to the current target
                var sqrDistance = Vector3.SqrMagnitude(ScanBuffer[i].transform.position - origin);
                
                // Skip this target if it is farther away than the current best
                if (sqrDistance > sqrShortestDistance) continue;
                
                // Update nearest target and shortest sqr distance
                nearestTarget = unitBehavior;
                sqrShortestDistance = sqrDistance;
            }
            
            // Sorting is done, return the result
            return nearestTarget;
        }
    }
}