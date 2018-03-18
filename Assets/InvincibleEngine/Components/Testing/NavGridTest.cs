using UnityEngine;
using VektorLibrary.EntityFramework.Components;
using VektorLibrary.Pathfinding.NavGrid;

namespace InvincibleEngine.Components.Testing {
    public class NavGridTest : EntityBehavior {
        public NavGridConfig GridConfig;

        private NavGrid _navGrid;

        public override void OnRegister() {
            _navGrid = new NavGrid(GridConfig);
            base.OnRegister();
        }

        public void OnDrawGizmos() {
            if (!Registered || !Application.isPlaying) return;
            foreach (var tile in _navGrid.Tiles) {
                foreach (var node in tile.Nodes) {
                    if (!node.Passable) continue;             
                    Gizmos.DrawLine(_navGrid.NodeToWorld(node), _navGrid.NodeToWorld(node) + Vector3.up * 5f);
                }
            }
        }
    }
}