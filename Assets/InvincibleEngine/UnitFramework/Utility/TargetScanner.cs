using System;
using InvincibleEngine.UnitFramework.Components;
using InvincibleEngine.UnitFramework.Enums;
using UnityEngine;
using Random = UnityEngine.Random;

namespace InvincibleEngine.UnitFramework.Utility {
    /// <summary>
    /// Used to scan for and retrieve valid targets for combat.
    /// </summary>
    public static class TargetScanner {
        // Set this to some power of two larger than the maximum expected size needed
        private const int BUFFER_SIZE = 8192;
        
        // Buffer for raw physics data
        private static readonly Collider[] ScanBuffer = new Collider[BUFFER_SIZE];
        
        // Scan for targets within a radius on the specified layers
        public static UnitBehavior ScanForTarget(Vector3 origin, float radius, LayerMask scanMask, TargetingMode mode = TargetingMode.Random) {            
            // Fetch all nearby colliders on the specified layers
            var bufferTail = Physics.OverlapSphereNonAlloc(origin, radius, ScanBuffer, scanMask);
            
            // Declare target reference
            UnitBehavior target = null;
            
            // Branch for targeting modes
            switch (mode) {
                case TargetingMode.Nearest:
                    // Iterate over the buffer and find the nearest target
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
                        target = unitBehavior;
                        sqrShortestDistance = sqrDistance;
                    }
                    
                    // Sorting is done, return the target
                    return target;
                case TargetingMode.Random:
                    // Return null if the buffer tail is zero (nothing found)
                    if (bufferTail == 0) return null;
            
                    // Select a random index from the buffer to use as the target
                    var randomIndex = Random.Range(0, bufferTail - 1);
                    target = ScanBuffer[randomIndex]?.GetComponent<UnitBehavior>();
                    return target;
                default:
                    Debug.LogError("Something went wrong with the targeting algorithm!");
                    return null;
            }
        }
    }
}