﻿using InvincibleEngine.UnitFramework.Components;
using UnityEngine;

namespace InvincibleEngine.UnitFramework.Utility {
    /// <summary>
    /// Used to scan for and retrieve valid targets for combat.
    /// </summary>
    public class TargetScanner {
        // Buffer for raw physics data
        private readonly Collider[] _scanBuffer;
        
        // LayerMask for scanning
        private LayerMask _scanMask;
        
        // Constructor
        public TargetScanner(LayerMask scanMask) {
            // Initialize the raw scan buffer
            _scanBuffer = new Collider[4096];
            
            // Set the scan mask
            _scanMask = scanMask;
        }
        
        // Update the scan layer mask
        public void UpdateScanMask(LayerMask mask) {
            _scanMask = mask;
        }
        
        // Scan for nearest target
        public UnitBehavior ScanForNearestTarget(Vector3 origin, float range) {            
            // Fetch all nearby colliders on the specified layers
            var bufferTail = Physics.OverlapBoxNonAlloc(origin, Vector3.one * range, _scanBuffer, Quaternion.identity, _scanMask);
            
            // Iterate over the buffer and find the nearest target
            UnitBehavior nearestTarget = null;
            var sqrShortestDistance = float.MaxValue;
            
            for (var i = 0; i < bufferTail; i++) {
                // Attempt to acquire the UnitBehavior component of the current object
                var unitBehavior = _scanBuffer[i].GetComponent<UnitBehavior>();
                
                // Skip this object if it does not have a UnitBehavior
                if (unitBehavior == null) continue;
                
                // Calculate sqr distance to the current target
                var sqrDistance = Vector3.SqrMagnitude(_scanBuffer[i].transform.position - origin);
                
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