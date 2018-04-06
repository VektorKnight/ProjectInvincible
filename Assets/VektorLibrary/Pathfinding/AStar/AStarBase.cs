using System.Collections.Generic;
using UnityEngine;
using VektorLibrary.Collections;
using VektorLibrary.Pathfinding.Grid;

namespace VektorLibrary.Pathfinding.AStar {
    /// <summary>
    /// Contains necessary functions for a standard A* implementation.
    /// Currently implements an octile heuristic with heap optimization.
    /// Optimized for concurrency with multiple threads accessing the NavGrid object.
    /// This implementation is inefficient for larger grids.
    /// </summary>
    public static class AStarBase {
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
            
            // Exit if either node is null
            if (startPos == null || endPos == null)
                return new AStarResult(false, null, request.Callback);

            // Exit if either node is impassable 
            if (!startPos.Passable || !endPos.Passable) 
                return new AStarResult(false, null, request.Callback);
            

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
                    
                    // Set the new G and H costs and the parent node ID
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
        
        /// <summary>
        /// Tries to update a given node data dictionary with data from a given node.
        /// </summary>
        /// <param name="grid"></param>
        /// <param name="current"></param>
        /// <param name="nodeData"></param>
        /// <returns>The current or new data entry.</returns>
        private static AStarNode UpdateDictionary(NavGrid grid, Vector2Int current, Dictionary<int, AStarNode> nodeData) {
            // Fetch node data from the NavGrid
            var node = grid.Node(current.x, current.y - 1);
            
            // Return null if the node data is null
            if (node == null) return null;
            
            // Reference an existing entry if it exists or create a new one and add it to the dictionary
            var data = nodeData.ContainsKey(node.ID) ? nodeData[node.ID] : new AStarNode(node.ID, node.Local, node.World);
            if (!nodeData.ContainsKey(data.ID)) nodeData.Add(data.ID, data);
            
            // Return the new data entry
            return data;
        }
        
        /// <summary>
        /// Returns an array of Vectors representing the calculated path.
        /// </summary>
        /// <param name="startNode"></param>
        /// <param name="endNode"></param>
        /// <param name="nodeData"></param>
        /// <returns></returns>
        public static Vector3[] RetracePath(AStarNode startNode, AStarNode endNode, IReadOnlyDictionary<int, AStarNode> nodeData) {
            var path = new List<Vector3>();
            var currentNode = endNode;

            while (!currentNode.Equals(startNode)) {
                path.Add(currentNode.World + Vector3.up * 4f);
                currentNode = nodeData[currentNode.Parent];
            }
            path.Reverse();

            return SimplifyPath(path.ToArray());
        }
        
        /// <summary>
        /// Attempts to simplify a path by removing any nodes not representing significant changes in direction.
        /// </summary>
        /// <param name="path">The array of path nodes to simplify.</param>
        /// <param name="epsilon">The maximum delta allowed before the a node is kept.</param>
        /// <returns></returns>
        public static Vector3[] SimplifyPath(Vector3[] path, float epsilon = 0.00015f) {
            // Create a list for the new path
            var newPath = new List<Vector3>();
            
            // Add the starting node
            newPath.Add(path[0]);
            var previous = Vector3.zero;
            
            // Loop through the nodes only adding major turning points
            for (var i = 1; i < path.Length - 1; i++) {
                // Calculate the delta between the vectors ignoring height (y)
                var direction = (previous - new Vector3(path[i].x, 0f, path[i].z)).normalized;
                var delta = 1f - Vector3.Dot(direction, previous.normalized);
                
                // Skip this node if delta < epsilon
                if (delta < epsilon) continue;
                
                // Add this node to the new path
                newPath.Add(path[i]);
                previous = direction;
            }
            
            // Add the end node
            newPath.Add(path[path.Length - 1]);
            
            return newPath.ToArray();
        }
    }
}