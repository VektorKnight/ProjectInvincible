using InvincibleEngine.EntityFramework.Interfaces;
using UnityEngine;
using VektorLibrary.EntityFramework.Interfaces;
using VektorLibrary.EntityFramework.Singletons;

namespace InvincibleEngine.EntityFramework.Components {
    public abstract class EntityBehavior : MonoBehaviour, IBehavior {
        
        // Private: Entity Tick Delta
        private float _lastEntityCall;
        
        // Property: Initialized
        public bool Initialized { get; private set; }
        
        // Property: Terminating
        public bool Terminating { get; private set; }

        // Unity Initialization
        private void Awake() {
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
        
        public virtual void Terminate() {
            Terminating = true;
            Destroy(this);
        }
    }
}