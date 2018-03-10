using System;

namespace InvincibleEngine.EntityFramework.Interfaces {
    public interface IGameEntity {
        void Initialize();
        void PhysicsUpdate(float physicsDelta);
        void EntityUpdate(float entityDelta);
        void RenderUpdate(float renderDelta);
    }
}