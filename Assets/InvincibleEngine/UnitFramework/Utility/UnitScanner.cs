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
    public static class UnitScanner {
        // Set this to some power of two larger than the maximum expected size
        private const int BUFFER_SIZE = 8192;
        
        // Buffer for raw physics data
        private static readonly UnitBehavior[] ScanBuffer = new UnitBehavior[BUFFER_SIZE];
        
        // Scan for objects within a radius containing the specified component
        public static UnitBehavior ScanForUnit(Vector3 origin, float radius, PlayerTeam exclusions, TargetingMode mode = TargetingMode.Random) { 
            // Declare target reference
            UnitBehavior target = null;

            // Fetch all nearby colliders on the specified layers
            var bufferTail = 0;
            var sqrShortestDistance = float.MaxValue;
            for (var i = 0; i < MatchManager.Instance.UnitList.Count; i++) {
                // Reference the current unit
                var unit = MatchManager.Instance.UnitList[i];

                // Skip if somehow null
                if (unit == null) continue;

                // Skip if the unit belongs to an excluded team (ally, neutral, whatever)
                if ((unit.UnitTeam & exclusions) == exclusions) continue;

                // Calculate sqr distance to the current unit
                var sqrDistance = Vector3.SqrMagnitude(unit.transform.position - origin);

                // Skip this target if it is farther away than the current best
                if (sqrDistance > sqrShortestDistance) continue;

                // Update nearest target and shortest sqr distance
                target = unit;
                sqrShortestDistance = sqrDistance;

                // Add to the buffer and increment the tail index
                ScanBuffer[bufferTail] = unit;
                bufferTail++;
            }

            // Branch for targeting modes
            switch (mode) {
                case TargetingMode.Nearest:
                    // Nearest target was precalculated during the sqr distance check
                    return target;
                case TargetingMode.Random:
                    // Return null if the buffer tail is zero (nothing found)
                    if (bufferTail == 0) return null;
            
                    // Select a random index from the buffer to use as the target
                    var randomIndex = Random.Range(0, bufferTail - 1);
                    target = ScanBuffer[randomIndex];
                    return target;
                default:
                    Debug.LogError("Something went wrong with the targeting algorithm!\n" +
                                   "We're all going to die!");
                    return null;
            }
        }
    }
}