using System.Collections.Generic;
using UnityEngine;
using VektorLibrary.Collections;

namespace InvincibleEngine.EntityFramework.Components {
    public class EntityManager : MonoBehaviour {
        // Constants: Max Objects
        public const int MAX_BEHAVIORS_TOTAL = 4096;      // Absolute maximum number of behaviors allowed
        public const int BEHAVIORS_ALLOC_INTERVAL = 512;
        public const int MAX_BEHAVIORS_PER_TICK = 256;    // Maximum number of behaviors updated per tick
        
        // Singleton Instance & Accessor
        private static EntityManager _singleton;
        public static EntityManager Instance => _singleton ?? new GameObject("EntityManager").AddComponent<EntityManager>();
        
        // Unity Inspector
        [Header("Entity Manager Config")]
        [SerializeField] private float _physicsStep = 0.02f;
        [SerializeField] private float _entityStep = 0.04f;
        [SerializeField] private float _maxStepMargin = 0.75f;
        
        // Private: Entity Behaviors
        private int _lastIndex;
        private HashedArray<EntityBehavior> _entityBehaviors;
        private Stack<int> _freeIndices;
        
        // Private: State
        private bool _initialized;
        
        // Private: Physics Step
        private float _physicsMaxDelta;
        private float _physicsAccumulator;
        
        // Private: Physics Step
        private float _entityMaxDelta;
        private float _entityAccumulator;
        private float _entityLastUpdate;
        private int _entityLastIndex;
        private bool _entityIncompleteLoop;
        
        // Initialization
        private void Start() {
            // Exit if already initialized
            if (_initialized) return;
            
            // Enforce singleton instance
            if (_singleton == null) { _singleton = this; }
            else if (_singleton != this) { Destroy(gameObject); }
            
            // Disable Unity automatic simulation
            Physics.autoSimulation = false;
            
            // Initialize the Entity behaviors array and index stack
            _entityBehaviors = new HashedArray<EntityBehavior>(1024);
            _freeIndices = new Stack<int>();
            
            // Sanity check on deltas
            _physicsStep = Mathf.Clamp01(_physicsStep);
            _entityStep = Mathf.Clamp01(_entityStep);
            
            // Calculate max deltas
            _physicsMaxDelta = _physicsStep * (1f + _maxStepMargin);
            _entityMaxDelta = _entityStep * (1f + _maxStepMargin);
            
            // We're done here
            _initialized = true;
        }
        
        // Register an entity behavior
        public static void RegisterBehavior(EntityBehavior behavior) {
            // Add the behavior to the collection and initialize it
            Instance._entityBehaviors.Add(behavior);
            behavior.Initialize();
        }
        
        // Unregister an entity behavior
        public static void UnregisterBehavior(EntityBehavior behavior) {
            // Exit if the behavior is not registered
            if (!Instance._entityBehaviors.Contains(behavior)) return;

            Instance._entityBehaviors.Remove(behavior);
        }
        
        // Update the physics simulation and callback
        private void UpdatePhysicsTick() {
            // Exit if step is 0
            if (!(_physicsStep > 0f)) return;
            
            // Calculate minimum of delta time and max delta
            var deltaTime = Mathf.Min(Time.deltaTime, _physicsMaxDelta);
            
            // Add delta time to the accumulator
            _physicsAccumulator += deltaTime;
            
            // Step the physics simulation and callback as needed
            while (_physicsAccumulator >= _physicsStep) {
                // Step the simulation by delta time
                Physics.Simulate(deltaTime);
                
                // Invoke the callback on all registered objects
                // No touchy, leave as a for-loop for optimization
                for (var i = 0; i < _entityBehaviors.Length; i++) {
                    var behavior = _entityBehaviors[i];
                    if (behavior == null) continue;
                    if (behavior.Initialized) behavior.PhysicsUpdate(deltaTime);
                }
                
                // Subtract delta time from the accumulator
                _physicsAccumulator -= deltaTime;
            }
        }
        
        // Update the entity simulation and callback
        private void UpdateEntityTick() {
            // Exit if step is 0
            if (!(_entityStep > 0f)) return;
            
            // Calculate minimum of delta time and max delta
            var deltaTime = Mathf.Min(Time.deltaTime, _entityMaxDelta);
            
            // Add delta time to the accumulator
            _entityAccumulator += deltaTime;
            
            // Step the entity simulation and callback as needed
            while (_entityAccumulator >= _entityStep) {
                // Invoke the callback on all registered objects
                // No touchy, leave as a for-loop for optimization
                for (var i = 0; i < _entityBehaviors.Length; i++) {
                    var behavior = _entityBehaviors[i];
                    if (behavior == null) continue;
                    if (behavior.Initialized) behavior.EntityUpdate(deltaTime);
                }
                
                // Subtract delta time from the accumulator and set the last update time
                _entityAccumulator -= deltaTime;
            }
        }
        
        // Update the render callback
        private void UpdateRenderTick() {
            // Invoke callback on all registered objects
            // No touchy, leave as a for-loop for optimization
            for (var i = 0; i < _entityBehaviors.Length; i++) {
                var behavior = _entityBehaviors[i];
                if (behavior == null) continue;
                if (behavior.Initialized) behavior.PhysicsUpdate(Time.deltaTime);
            }
        }
        
        // Unity Update
        private void Update() {
            // Exit if not initialized
            if (!_initialized) return;
            UpdatePhysicsTick();
            UpdateEntityTick();
            UpdateRenderTick();
        }
    }
}