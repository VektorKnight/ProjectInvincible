using System.Collections.Generic;
using UnityEngine;
using VektorLibrary.Collections;
using VektorLibrary.Pathfinding.Grid;

namespace VektorLibrary.Pathfinding.AStar {
    public static class AStarBasic {
        /// <summary>
        /// Calculates a path through a NavGrid using standard A* with octile heuristics and heap optimization.
        /// </summary>
        /// <param name="grid">The NavGrid through which to calculate a path.</param>
        /// <param name="request">Path request structure.</param>
        /// <returns>Results of the path calculations.</returns>
        public static AStarResult CalculatePath(NavGrid grid, AStarRequest request) {
            // Exit if either node lies outside the NavGrid bounds
            if (!grid.ContainsPoint(request.Start) || !grid.ContainsPoint(request.End)) {
                return new AStarResult(false, null, request.Callback);
            }

            // Convert world points to grid nodes
            var startPos = grid.WorldToNode(request.Start);
            var endPos = grid.WorldToNode(request.End);

            // Exit if either node is impassable 
            if (!startPos.Passable || !endPos.Passable) {
                return new AStarResult(false, null, request.Callback);
            }

            // Initialize temporary collections
            var nodeData = new Dictionary<int, AStarNode>();
            var openSet = new MinHeap<AStarNode>(grid.Config.Dimension * grid.Config.Dimension);
            var closedSet = new HashSet<AStarNode>();
            
            // Create a new AStarNode from the start and end nodes
            var startNode = new AStarNode(startPos.ID, startPos.Local, startPos.World);
            var endNode = new AStarNode(endPos.ID, endPos.Local, endPos.World);
            
            // Set up dictionary references for the start and end nodes
            nodeData.Add(startNode.ID, startNode);
            nodeData.Add(endNode.ID, endNode);
            openSet.Add(startNode);

            // Track number of nodes checked
            while (openSet.Count > 0) {
                var currentNode = openSet.RemoveFirst();
                closedSet.Add(currentNode);

                // Check if we've arrived at the destination
                if (currentNode.Equals(endNode))
                    return new AStarResult(true, RetracePath(startNode, endNode, nodeData), request.Callback);

                // Check each neighbor of the current node
                foreach (var node in grid.GetNeighbors(currentNode.Local)) {
                    // Reference existing or create new AStarNode data for the given node
                    var neighbor = nodeData.ContainsKey(node.ID) ? nodeData[node.ID] : new AStarNode(node.ID, node.Local, node.World);
                    if (!nodeData.ContainsKey(node.ID)) nodeData.Add(node.ID, neighbor);
                    
                    // If this neighbor is impassable skip it and continue
                    if (!node.Passable || closedSet.Contains(neighbor)) continue;
                    
                    // Calculate the new GCost for the neighbor
                    var newCostToNeighbor = currentNode.GCost + Heuristics.Octile(currentNode.Local, neighbor.Local);
                    
                    // Skip if the new GCost is >= to the current and the neighbor lies within the open set
                    if (newCostToNeighbor >= neighbor.GCost && openSet.Contains(neighbor)) continue;
                    
                    // Set the new G and H costs and set the parent node value
                    neighbor.GCost = newCostToNeighbor;
                    neighbor.HCost = Heuristics.Octile(neighbor.Local, endNode.Local);
                    neighbor.Parent = currentNode.ID;
                    
                    // Update or add the neighbor to the open set
                    if (!openSet.Contains(neighbor)) openSet.Add(neighbor);
                    else openSet.UpdateItem(neighbor);
                    
                }
            }
            
            // Failed to find a path, return a null result
            return new AStarResult(false, null, request.Callback);
        }
        
        // Retrace the path once it is found
        private static Vector3[] RetracePath(AStarNode startNode, AStarNode endNode, IReadOnlyDictionary<int, AStarNode> nodeData) {
            var path = new List<Vector3>();
            var currentNode = endNode;

            while (!currentNode.Equals(startNode)) {
                if (currentNode.ID == 0) continue;
                path.Add(currentNode.World);
                currentNode = nodeData[currentNode.Parent];
            }
            path.Reverse();

            return path.ToArray();
        }
    }
}