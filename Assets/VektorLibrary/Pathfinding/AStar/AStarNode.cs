using System;
using UnityEngine;

namespace VektorLibrary.Pathfinding.AStar {
    /// <summary>
    /// Represents the necessary data for a NavGridNode used by the A* algorithm.
    /// </summary>
    public class AStarNode : IComparable<AStarNode>, IEquatable<AStarNode> {
        public readonly int ID;
        public readonly Vector2Int Local;
        public readonly Vector3 World;
        public int GCost;
        public int HCost;
        public int FCost => GCost + HCost;
        public int Parent;
        
        /// <summary>
        /// Creates a new AStarNode.
        /// </summary>
        /// <param name="local">The grid position of this node.</param>
        public AStarNode(int id, Vector2Int local, Vector3 world) {
            ID = id;
            Local = local;
            World = world;
        }
        
        /// <summary>
        /// Compares this node to another using F and/or G costs.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public int CompareTo(AStarNode other) {
            var compare = FCost.CompareTo(other.FCost);
            return compare == 0 ? -HCost.CompareTo(other.HCost) : -compare;
        }
        
        /// <summary>
        /// Returns true if this node and the other node share a local coordinate.
        /// </summary>
        /// <param name="other">The node to compares this one to.</param>
        public bool Equals(AStarNode other) {
            return ID.Equals(other.ID);
        }
        
        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            return obj is AStarNode && Equals((AStarNode) obj);
        }

        public override int GetHashCode() {
            return ID;
        }
    }
}