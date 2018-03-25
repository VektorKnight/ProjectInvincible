using System;
using UnityEngine;

namespace VektorLibrary.Pathfinding.NavGrid.AStar {
    /// <summary>
    /// Represents an A* path request.
    /// </summary>
    public struct AStarRequest {
        public Vector3 Start;                   // The starting position of the path
        public Vector3 End;                     // The desired position to reach
        public int Timeout;                     // Optional timeout for the request
        public Action<AStarResult> Callback;    // The callback to invoke once the path request is completed

        public AStarRequest(Vector3 start, Vector3 end, int timeout, Action<AStarResult> callback) {
            Start = start;
            End = end;
            Timeout = timeout;
            Callback = callback;
        }
    }
}