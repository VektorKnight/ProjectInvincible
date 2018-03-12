using UnityEngine;
using VektorLibrary.EntityFramework.Interfaces;

namespace VektorLibrary.EntityFramework.Components {
    public abstract class PooledBehavior : EntityBehavior, IPoolable {
        public bool Initialized { get; private set; }

        public virtual void Initialize() {
            Initialized = true;
        }

        public abstract void OnRetrieved();

        public abstract void OnReturned();

        public virtual void ReturnToPool() {
            throw new System.NotImplementedException();
        }
    }
}