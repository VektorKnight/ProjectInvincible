using UnityEngine;
using InvincibleEngine;
namespace InvincibleEngine.Components.Testing {
    public class TestEntity : EntityBehavior {
        public override void OnRenderUpdate(float renderDelta) {
            transform.position += Vector3.up * Mathf.Sin(Time.time);
        }
    }
}