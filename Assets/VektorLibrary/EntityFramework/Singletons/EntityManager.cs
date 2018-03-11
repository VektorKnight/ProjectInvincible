using InvincibleEngine.EntityFramework.Components;
using InvincibleEngine.EntityFramework.Interfaces;
using UnityEngine;
using VektorLibrary.Collections;
using VektorLibrary.EntityFramework.Interfaces;

namespace VektorLibrary.EntityFramework.Singletons {
    public class EntityManager : MonoBehaviour {    
        // Singleton Instance Accessor
        public static EntityManager Instance { get; private set; }

        // Constants: Timestep
        public const float FIXED_TIMESTEP = 0.02f;     // Time interval for the fixed timestep update
        public const float MAX_STEP_MARGIN = 0.75f;    // Maximum margin for delta time if a spike occurs
        
        // Private: Entity Behaviors
        private HashedArray<IBehavior> _entityBehaviors;
        
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
        
        // Preload
        [RuntimeInitializeOnLoadMethod]
        private static void Preload() {
            // Ensure this singleton initializes at startup
            if (Instance == null) Instance = new GameObject("EntityManager").AddComponent<EntityManager>();
            Instance.Initialize();
            
            // Ensure this singleton does not get destroyed on scene load
            DontDestroyOnLoad(Instance);
        }
        
        // Initialization
        private void Initialize() {
            // Exit if already initialized
            if (_initialized) return;
            
            // Enforce singleton instance
            if (Instance == null) { Instance = this; }
            else if (Instance != this) { Destroy(gameObject); }
            
            // Disable Unity automatic simulation
            Physics.autoSimulation = false;
            
            // Initialize the Entity behaviors array and index stack
            _entityBehaviors = new HashedArray<IBehavior>(1024);
            
            // Calculate max deltas
            _physicsMaxDelta = FIXED_TIMESTEP * (1f + MAX_STEP_MARGIN);
            
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
            
            // Remove the behavior
            Instance._entityBehaviors.Remove(behavior);
            behavior.Terminate();
        }
        
        // Update the render callback
        private void UpdateRenderTick() {
            // Invoke callback on all registered objects
            // No touchy, leave as a for-loop for optimization
            for (var i = 0; i < _entityBehaviors.LastIndex; i++) {
                var behavior = _entityBehaviors[i];
                if (behavior == null) continue;
                if (!behavior.Initialized || behavior.Terminating) continue;
                behavior.RenderUpdate(Time.deltaTime);
            }
        }
        
        // Unity Update
        private void Update() {
            // Exit if not initialized
            if (!_initialized) return;
           
            // Calculate minimum of delta time and max delta
            var deltaTime = Mathf.Min(Time.deltaTime, _physicsMaxDelta);
            
            // Add delta time to the accumulator
            _physicsAccumulator += deltaTime;
            
            // Step the physics simulation and callback as needed
            while (_physicsAccumulator >= FIXED_TIMESTEP) {
                // Step the simulation by delta time
                Physics.Simulate(deltaTime);
                
                // Invoke the callback on all registered objects
                // No touchy, leave as a for-loop for optimization
                for (var i = 0; i < _entityBehaviors.LastIndex; i++) {
                    var behavior = _entityBehaviors[i];
                    if (behavior == null) continue;
                    if (!behavior.Initialized || behavior.Terminating) continue;
                    behavior.PhysicsUpdate(deltaTime);
                    behavior.EntityUpdate(deltaTime);
                }
                
                // Subtract delta time from the accumulator
                _physicsAccumulator -= deltaTime;
            }
            
            // Update Render Callback
            UpdateRenderTick();
        }
    }
}