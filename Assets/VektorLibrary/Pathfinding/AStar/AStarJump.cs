using System;
using System.Collections.Generic;
using UnityEngine;
using VektorLibrary.Pathfinding.Grid;

namespace VektorLibrary.Pathfinding.AStar {
    /// <summary>
    /// Contains necessary functions for a Jump Point A* implementation.
    /// Currently implements an octile heuristic with heap optimization.
    /// Optimized for concurrency with multiple threads accessing the NavGrid object.
    /// This implementation is optimized for larger grids.
    /// TODO: Currently a work in progress! Methods untested :D
    /// </summary>
    public static class AStarJump {
        public static List<AStarNode> GetNeighbors(NavGrid grid, AStarNode node, Dictionary<int, AStarNode> nodeData) {
            // Create a list of neighbors to return
            var neighbors = new List<AStarNode>();
            
            // Check for neighbors if the current node has a parent
            if (node.Parent >= 0) {
                // Get the parent node
                var parent = nodeData[node.Parent];

                // Get the direction of travel
                var delta = new Vector2Int((node.Local.x - parent.Local.x) / Mathf.Max(Mathf.Abs(node.Local.x - parent.Local.x), 1),
                    (node.Local.y - parent.Local.y) / Mathf.Max(Mathf.Abs(node.Local.y - parent.Local.y), 1));

                // Search all four diagonals of the current node
                if (delta.x != 0 && delta.y != 0) {
                    // Vertical search
                    if (grid.Passable(node.Local.x, node.Local.y + delta.y)) {
                        var neighbor = grid.Node(node.Local.x, node.Local.y + delta.y);
                        var data = nodeData.ContainsKey(neighbor.ID) ? nodeData[neighbor.ID] : new AStarNode(neighbor.ID, neighbor.Local, neighbor.World);
                        if (!nodeData.ContainsKey(data.ID)) nodeData.Add(data.ID, data);
                        neighbors.Add(data);
                    }

                    // Horizontal search
                    if (grid.Passable(node.Local.x + delta.x, node.Local.y)) {
                        var neighbor = grid.Node(node.Local.x + delta.x, node.Local.y);
                        var data = nodeData.ContainsKey(neighbor.ID) ? nodeData[neighbor.ID] : new AStarNode(neighbor.ID, neighbor.Local, neighbor.World);
                        if (!nodeData.ContainsKey(data.ID)) nodeData.Add(data.ID, data);
                        neighbors.Add(data);
                    }

                    // Diagonal search
                    if (grid.Passable(node.Local.x + delta.x, node.Local.y + delta.y)) {
                        var neighbor = grid.Node(node.Local.x + delta.x, node.Local.y + delta.y);
                        var data = nodeData.ContainsKey(neighbor.ID) ? nodeData[neighbor.ID] : new AStarNode(neighbor.ID, neighbor.Local, neighbor.World);
                        if (!nodeData.ContainsKey(data.ID)) nodeData.Add(data.ID, data);
                        neighbors.Add(data);
                    }

                    // Horizontal search
                    if (!grid.Passable(node.Local.x - delta.x, node.Local.y)) {
                        var neighbor = grid.Node(node.Local.x - delta.x, node.Local.y);
                        var data = nodeData.ContainsKey(neighbor.ID) ? nodeData[neighbor.ID] : new AStarNode(neighbor.ID, neighbor.Local, neighbor.World);
                        if (!nodeData.ContainsKey(data.ID)) nodeData.Add(data.ID, data);
                        neighbors.Add(data);
                    }

                    // Vertical search
                    if (!grid.Passable(node.Local.x, node.Local.y - delta.y)) {
                        var neighbor = grid.Node(node.Local.x, node.Local.y - delta.y);
                        var data = nodeData.ContainsKey(neighbor.ID) ? nodeData[neighbor.ID] : new AStarNode(neighbor.ID, neighbor.Local, neighbor.World);
                        if (!nodeData.ContainsKey(data.ID)) nodeData.Add(data.ID, data);
                        neighbors.Add(data);
                    }
                }

                // Search horizontal and vertical directions
                else {
                    if (delta.x == 0) {
                        // Vertical search
                        if (grid.Passable(node.Local.x, node.Local.y + delta.y)) {
                            var neighbor = grid.Node(node.Local.x, node.Local.y + delta.y);
                            var data = nodeData.ContainsKey(neighbor.ID) ? nodeData[neighbor.ID] : new AStarNode(neighbor.ID, neighbor.Local, neighbor.World);
                            if (!nodeData.ContainsKey(data.ID)) nodeData.Add(data.ID, data);
                            neighbors.Add(data);
                        }

                        // Horizontal right
                        if (!grid.Passable(node.Local.x + 1, node.Local.y)) {
                            var neighbor = grid.Node(node.Local.x + 1, node.Local.y);
                            var data = nodeData.ContainsKey(neighbor.ID) ? nodeData[neighbor.ID] : new AStarNode(neighbor.ID, neighbor.Local, neighbor.World);
                            if (!nodeData.ContainsKey(data.ID)) nodeData.Add(data.ID, data);
                            neighbors.Add(data);
                        }

                        // Horizontal left
                        if (!grid.Passable(node.Local.x - 1, node.Local.y)) {
                            var neighbor = grid.Node(node.Local.x - 1, node.Local.y);
                            var data = nodeData.ContainsKey(neighbor.ID) ? nodeData[neighbor.ID] : new AStarNode(neighbor.ID, neighbor.Local, neighbor.World);
                            if (!nodeData.ContainsKey(data.ID)) nodeData.Add(data.ID, data);
                            neighbors.Add(data);
                        }
                    }
                    else {
                        // Horizontal search
                        if (grid.Passable(node.Local.x, node.Local.y + delta.y)) {
                            var neighbor = grid.Node(node.Local.x, node.Local.y + delta.y);
                            var data = nodeData.ContainsKey(neighbor.ID) ? nodeData[neighbor.ID] : new AStarNode(neighbor.ID, neighbor.Local, neighbor.World);
                            if (!nodeData.ContainsKey(data.ID)) nodeData.Add(data.ID, data);
                            neighbors.Add(data);
                        }

                        // Vertical up
                        if (!grid.Passable(node.Local.x, node.Local.y + 1)) {
                            var neighbor = grid.Node(node.Local.x, node.Local.y + 1);
                            var data = nodeData.ContainsKey(neighbor.ID) ? nodeData[neighbor.ID] : new AStarNode(neighbor.ID, neighbor.Local, neighbor.World);
                            if (!nodeData.ContainsKey(data.ID)) nodeData.Add(data.ID, data);
                            neighbors.Add(data);
                        }

                        // Vertical down
                        if (!grid.Passable(node.Local.x, node.Local.y - 1)) {
                            var neighbor = grid.Node(node.Local.x, node.Local.y - 1);
                            var data = nodeData.ContainsKey(neighbor.ID) ? nodeData[neighbor.ID] : new AStarNode(neighbor.ID, neighbor.Local, neighbor.World);
                            if (!nodeData.ContainsKey(data.ID)) nodeData.Add(data.ID, data);
                            neighbors.Add(data);
                        }
                    }
                }
            }

            // Return all neighbors
            else {
                var neighborNodes = grid.GetNeighbors(node.Local);
                for (var i = 0; i < neighborNodes.Count; i++) {
                    var id = neighborNodes[i].ID;
                    var data = nodeData.ContainsKey(id) ? nodeData[id] : new AStarNode(id, neighborNodes[i].Local, neighborNodes[i].World);
                    if (!nodeData.ContainsKey(id)) nodeData.Add(id, data);
                    neighbors.Add(data);
                }
            }
            return neighbors;
        }
        
        // TODO: Finish this!
        public static AStarNode JumpPoint(NavGrid grid, AStarNode node) {
            throw new NotImplementedException();
        }
    }
}