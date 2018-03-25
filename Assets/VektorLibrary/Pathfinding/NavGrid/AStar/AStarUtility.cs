using System.Collections.Generic;
using UnityEngine;
using VektorLibrary.Collections;

namespace VektorLibrary.Pathfinding.NavGrid.AStar {
    public static class AStarUtility {
        public static AStarResult CalculatePath(NavGrid grid, AStarRequest request) {
            // Exit if either node lies outside the NavGrid bounds
            if (!grid.ContainsPoint(request.Start) || !grid.ContainsPoint(request.End)) {
                return new AStarResult(false, null);
            }

            // Convert world points to grid nodes
            var startNode = grid.WorldToNode(request.Start);
            var endNode = grid.WorldToNode(request.End);

            // Exit if either node is impassable 
            if (!startNode.Passable || !endNode.Passable) {
                return new AStarResult(false, null);
            }

            // Begin calculating a path
            var openSet = new MinHeap<NavGridNode>(grid.Config.Dimension * grid.Config.Dimension);
            var closedSet = new HashSet<NavGridNode>();
            openSet.Add(startNode);

            // Track number of nodes checked
            while (openSet.Count > 0) {
                var currentNode = openSet.RemoveFirst();
                closedSet.Add(currentNode);

                // Check if we've arrived at the destination
                if (currentNode == endNode)
                    return new AStarResult(true, RetracePath(startNode, endNode));

                // Check each neighbor of the current node
                foreach (var neighbor in grid.GetNeighbours(currentNode)) {
                    if (!neighbor.Passable || closedSet.Contains(neighbor)) {
                        continue;
                    }

                    var newCostToNeighbour = currentNode.GCost + OctileDistance(currentNode, neighbor);
                    if (newCostToNeighbour >= neighbor.GCost && openSet.Contains(neighbor)) continue;
                    neighbor.GCost = newCostToNeighbour;
                    neighbor.HCost = OctileDistance(neighbor, endNode);
                    neighbor.Parent = currentNode;

                    if (!openSet.Contains(neighbor))
                        openSet.Add(neighbor);
                    else {
                        openSet.UpdateItem(neighbor);
                    }
                }
            }
            return new AStarResult(false, null);
        }
        
        // Retrace the path once it is found
        private static Vector3[] RetracePath(NavGridNode startNode, NavGridNode endNode) {
            var path = new List<Vector3>();
            var currentNode = endNode;

            while (currentNode != startNode) {
                path.Add(currentNode.WorldPosition + Vector3.up);
                currentNode = currentNode.Parent;
            }
            path.Reverse();

            return path.ToArray();
        }
        
        /// <summary>
        /// Returns the Manhattan distance between two nodes.
        /// </summary>
        /// <param name="a">The first node.</param>
        /// <param name="b">The second node.</param>
        public static int ManhattanDistance(NavGridNode a, NavGridNode b) {
            var dX = Mathf.Abs(a.GridPosition.x - b.GridPosition.x);
            var dY = Mathf.Abs(a.GridPosition.y - b.GridPosition.y);
            return dX + dY;
        }
        
        /// <summary>
        /// Returns the Octile distance between two nodes.
        /// Values multiplied by 10 for integer math precision.
        /// </summary>
        /// <param name="a">The first node.</param>
        /// <param name="b">The second node.</param>
        public static int OctileDistance(NavGridNode a, NavGridNode b) {
            var dX = 10 * Mathf.Abs(a.GridPosition.x - b.GridPosition.x);
            var dY = 10 * Mathf.Abs(a.GridPosition.y - b.GridPosition.y);
            return dX < dY ? 14 * (dX + dY) : 14 * (dY + dX);
        }
        
        /// <summary>
        /// Returns the Euclidean distance between two nodes.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        public static float EuclideanDistance(NavGridNode a, NavGridNode b) {
            return Vector2Int.Distance(a.GridPosition, b.GridPosition);
        }
    }
}