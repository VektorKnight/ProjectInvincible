using System.Collections.Generic;
using UnityEngine;
using VektorLibrary.Pathfinding.NavGrid;

namespace InvincibleEngine.Components.Testing {
    public class NavGridTest : MonoBehaviour {
        public LayerMask GroundMask;
        public LayerMask ObstacleMask;
        public Mesh NodeMesh;
        public Material[] NodeMaterials;
        private NavGrid _navGrid;

        private List<Matrix4x4> _passable = new List<Matrix4x4>();
        private List<Matrix4x4> _blocked = new List<Matrix4x4>();
        
        public void Start() {
            var config = new NavGridConfig() {
                Origin = Vector3.zero,
                Size = 64,
                UnitsPerNode = 8,
                Subdivision = 4,
                MaxHeight = 100f,
                MaxSteepness = 0.66f,
                GroundMask = GroundMask,
                ObstacleMask = ObstacleMask                              
            };
            
            _navGrid = new NavGrid(config);
            
            // Debugging
            for (var i = 0; i < _navGrid.Nodes.Length; i++) {
                var node = _navGrid.Nodes[i];
                var nodeWorld = NavGrid.NodeToWorld(_navGrid, node.GridX, node.GridY);
                if (node.Passable) _passable.Add(Matrix4x4.TRS(nodeWorld, Quaternion.identity, Vector3.one));
                if (!node.Passable) _blocked.Add(Matrix4x4.TRS(nodeWorld, Quaternion.identity, Vector3.one));
            }
        }
    }
}