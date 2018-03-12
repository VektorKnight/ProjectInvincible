using UnityEngine;
using VektorLibrary.Collections;
using VektorLibrary.EntityFramework.Components;
using VektorLibrary.EntityFramework.Interfaces;

namespace VektorLibrary.EntityFramework.Singletons {
    /// <summary>
    /// Manages entities implementing the IBehavior interface and relevant update callbacks.
    /// </summary>
    public class EntityManager : MonoBehaviour {    
        // Singleton Instance Accessor
        public static EntityManager Instance { get; private set; }

        // Constants: Timestep
        public const float FIXED_TIMESTEP = 0.02f;     // Time interval for the fixed timestep update
        public const float MAX_STEP_MARGIN = 0.75f;    // Maximum margin for delta time if a spike occurs
        
        // Private: Entity Behaviors
        private HashedArray<IEntity> _behaviors;
        
        // Private: State
        private bool _initialized;
        
        // Private: Fixed Timestep
        private float _stepMaxDelta;
        private float _stepAccumulator;
        private bool _physicsSimulated;
        
        // Preload Method
        [RuntimeInitializeOnLoadMethod]
        private static void Preload() {
            // Ensure this singleton initializes at startup
            if (Instance == null) Instance = new GameObject("EntityManager").AddComponent<EntityManager>();
            
            // Ensure this singleton does not get destroyed on scene load
            DontDestroyOnLoad(Instance.gameObject);
            
            // Initialize the instance
            Instance.Initialize();
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
            _behaviors = new HashedArray<IEntity>(1024);
            
            // Calculate max deltas
            _stepMaxDelta = FIXED_TIMESTEP * (1f + MAX_STEP_MARGIN);
            
            // We're done here
            _initialized = true;
        }
        
        // Register an entity behavior
        public static void RegisterBehavior(IEntity behavior) {
            // Add the behavior to the collection and initialize it
            Instance._behaviors.Add(behavior);
            behavior.OnRegister();
        }
        
        // Unregister an entity behavior
        public static void UnregisterBehavior(IEntity behavior) {
            // Exit if the behavior is not registered
            if (!Instance._behaviors.Contains(behavior)) return;
            
            // Remove the behavior
            Instance._behaviors.Remove(behavior);
            behavior.OnTerminate();
        }
        
        // Unity Update
        private void Update() {
            // Exit if not initialized
            if (!_initialized) return;
           
            // Calculate minimum of delta time and max delta
            var deltaTime = Mathf.Min(Time.deltaTime, _stepMaxDelta);
            
            // Add delta time to the accumulator
            _stepAccumulator += deltaTime;
            
            // Iterate through the behaviors
            for (var i = 0; i < _behaviors.LastIndex; i++) {
                // Reference the current behavior
                var behavior = _behaviors[i];
                
                // Handle fixed timestep callbacks and physics simulation
                while (_stepAccumulator >= FIXED_TIMESTEP) {
                    // Step the physics simulation if necessary
                    if (!_physicsSimulated) {
                        Physics.Simulate(deltaTime);
                        _physicsSimulated = true;
                    }
                    
                    // Subtract the timestep/count from the accumulator
                    _stepAccumulator -= FIXED_TIMESTEP / _behaviors.LastIndex;
                    
                    // Check for any flags         
                    if (behavior == null || !behavior.Registered || behavior.Terminating) continue;
                    
                    // Invoke the fixed timestep callbacks as necessary
                    behavior.OnPhysicsUpdate(deltaTime);
                    behavior.OnEntityUpdate(deltaTime);
                }
                
                // Check for any flags and invoke the render callback if necessary    
                if (behavior == null || !behavior.Registered || behavior.Terminating) continue;
                behavior.OnRenderUpdate(deltaTime);
            }
            
            // Reset the physics simulated flag
            _physicsSimulated = false;
        }
    }
}