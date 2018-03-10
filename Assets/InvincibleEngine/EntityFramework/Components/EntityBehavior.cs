using InvincibleEngine.EntityFramework.Interfaces;
using UnityEngine;

namespace InvincibleEngine.EntityFramework.Components {
    public abstract class EntityBehavior : MonoBehaviour, IGameEntity {
        
        // Private: Entity Tick Delta
        private float _lastEntityCall;
        
        // Property: State
        public bool Initialized { get; private set; }
        
        // Unity Initialization
        private void Start() {
            // Exit if already initialized
            if (Initialized) return;
            
            // Register with the Entity Manager
            EntityManager.RegisterBehavior(this);
        }
        
        // Unity Destroy
        protected virtual void OnDestroy() {
            EntityManager.UnregisterBehavior(this);
        }

        public virtual void Initialize() {
            Initialized = true;
        }

        public abstract void PhysicsUpdate(float physicsDelta);

        public abstract void EntityUpdate(float entityDelta);

        public abstract void RenderUpdate(float renderDelta);
    }
}