using UnityEngine;
using VektorLibrary.EntityFramework.Components;

namespace InvincibleEngine.Components.Testing {
    public class TestSpawner : EntityBehavior {

        public int GridSize = 64;
        public float VerticalOffset = 40;
        public GameObject Prefab;

        private int _objectCount;
        private int _y;

        public override void OnRenderUpdate(float renderDelta) {
            if (_objectCount == GridSize * GridSize) return;
            
            // Spawn a row
            for (var i = 0; i < GridSize; i++) {
                Instantiate(Prefab, new Vector3(i * 10, VerticalOffset, _y * 10), Quaternion.identity);
                _objectCount++;
            }
            
            // increment y
            _y++;
        }
    }
}