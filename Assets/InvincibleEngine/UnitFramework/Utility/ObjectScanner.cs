using System;
using InvincibleEngine.UnitFramework.Components;
using InvincibleEngine.UnitFramework.Enums;
using UnityEngine;
using Random = UnityEngine.Random;

namespace InvincibleEngine.UnitFramework.Utility {
    /// <summary>
    /// Used to scan for and retrieve valid targets for combat.
    /// The default target selection mode is currently set to Random.
    /// Random provides a more organic feel to unit combat and runs faster
    /// due to the lack of a sorting loop.
    /// Nearest is best used for units where engaging the closest target makes
    /// more sense such as point defenses and countermeasures.
    /// </summary>
    public static class ObjectScanner {
        // Set this to some power of two larger than the maximum expected size
        private const int BUFFER_SIZE = 8192;
        
        // Buffer for raw physics data
        private static readonly Collider[] ScanBuffer = new Collider[BUFFER_SIZE];
        
        // Scan for objects within a radius containing the specified component
        public static T ScanForObject<T>(Vector3 origin, float radius, LayerMask scanMask, TargetingMode mode = TargetingMode.Random) where T : MonoBehaviour {            
            // Fetch all nearby colliders on the specified layers
            var bufferTail = Physics.OverlapSphereNonAlloc(origin, radius, ScanBuffer, scanMask);
            
            // Declare target reference
            T target = null;
            
            // Branch for targeting modes
            switch (mode) {
                case TargetingMode.Nearest:
                    // Iterate over the buffer and find the nearest target
                    var sqrShortestDistance = float.MaxValue;
                    for (var i = 0; i < bufferTail; i++) {
                        // Attempt to acquire the UnitBehavior component of the current object
                        var component = ScanBuffer[i].GetComponent<T>();
                
                        // Skip this object if it does not have a UnitBehavior
                        if (component == null) continue;
                
                        // Calculate sqr distance to the current target
                        var sqrDistance = Vector3.SqrMagnitude(ScanBuffer[i].transform.position - origin);
                
                        // Skip this target if it is farther away than the current best
                        if (sqrDistance > sqrShortestDistance) continue;
                
                        // Update nearest target and shortest sqr distance
                        target = component;
                        sqrShortestDistance = sqrDistance;
                    }
                    
                    // Sorting is done, return the target
                    return target;
                case TargetingMode.Random:
                    // Return null if the buffer tail is zero (nothing found)
                    if (bufferTail == 0) return null;
            
                    // Select a random index from the buffer to use as the target
                    var randomIndex = Random.Range(0, bufferTail - 1);
                    target = ScanBuffer[randomIndex]?.GetComponent<T>();
                    return target;
                default:
                    Debug.LogError("Something went wrong with the targeting algorithm!\n" +
                                   "We're all going to die!");
                    return null;
            }
        }
    }
}