﻿using UnityEngine;
using UnityEngine.UI;
using InvincibleEngine;
using VektorLibrary.EntityFramework.Singletons;

namespace InvincibleEngine {
    public abstract class EntityBehavior : MonoBehaviour, IEntity, ISelectable {
        
        // Property: Registered
        public bool Registered { get; private set; }
        
        // Property: Terminating
        public bool Terminating { get; private set; }

        //Owner of object, -1 is the empty player
        public int PlayerOwner = -1;

        //Build Options
        [SerializeField] public BuildOption[] BuildOptions;

        //Image used for unit icon
        public Sprite Icon;

        // Unity Initialization
        public virtual void Start() {
            // Exit if already initialized
            if (Registered) return;
            
            // Register with the Entity Manager
            EntityManager.RegisterBehavior(this);
        }
        
        // Unity Destroy
        protected virtual void OnDestroy() {
            EntityManager.UnregisterBehavior(this);
        }
        
        // Behavior regisration callback
        public virtual void OnRegister() {
            Registered = true;
        }
        
        // Physics update callback
        public virtual void OnPhysicsUpdate(float physicsDelta) { }
        
        // Entity update callback
        public virtual void OnEntityHostUpdate(float entityDelta) { }
        
        // Render update callback
        public virtual void OnRenderUpdate(float renderDelta) { }
        
        // Termination
        public virtual void Terminate() {
            Terminating = true;
            Destroy(this);
        }

        public virtual void OnSelected() {

        }

        public virtual void OnDeselected() {

        }
    }
}