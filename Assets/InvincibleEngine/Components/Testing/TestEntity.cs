using UnityEngine;
using InvincibleEngine.EntityFramework.Components;

namespace InvincibleEngine.Components.Testing {
    public class TestEntity : EntityBehavior {
        public override void PhysicsUpdate(float physicsDelta) {
            return;
        }

        public override void EntityUpdate(float entityDelta) {
            return;
        }

        public override void RenderUpdate(float renderDelta) {
            return;
        }
    }
}