﻿using UnityEngine;
using System.Collections;
using VektorLibrary.Collections;
using InvincibleEngine;
using SteamNet;
using VektorLibrary.EntityFramework.Components;

namespace InvincibleEngine {
    /// <summary>
    /// Manages entities implementing the IBehavior interface and relevant update callbacks.
    /// </summary>
    public class EntityManager : MonoBehaviour {    
        // Singleton Instance Accessor
        public static EntityManager Instance { get; private set; }

        // Constants: Timestep
        public const float FIXED_TIMESTEP = 0.02f;     // Time interval for the fixed timestep update
        public const float MAX_STEP_MARGIN = 0.75f;    // Maximum margin for delta time if a spike occurs
        
        // Properties: Total number of sim ticks
        public static int SimTickCount { get; private set; }

        // Private: Entity Behaviors
        private HashedArray<EntityBehavior> _entityBehaviors;
        
        // Private: State
        [SerializeField] private bool _initialized;
        
        // Private: Fixed Timestep
        private float _stepMaxDelta;
        private float _stepAccumulator;
        private bool _physicsSimulated;
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Preload() {
            //Make sure the Managers object exists
            GameObject managers = GameObject.Find("Managers") ?? new GameObject("Managers");

            // Ensure this singleton initializes at startup
            if (Instance == null) Instance = managers.GetComponent<EntityManager>() ?? managers.AddComponent<EntityManager>();

            // Ensure this singleton does not get destroyed on scene load
            DontDestroyOnLoad(Instance.gameObject);
            
            // Initialize the instance
            Instance.Initialize();
        }

        // Initialization        
        private void Initialize() {
            // Exit if already initialized
            if (_initialized) return;            
          
            // Disable Unity automatic simulation
            Physics.autoSimulation = false;
            
            // Calculate max deltas
            _stepMaxDelta = FIXED_TIMESTEP * (1f + MAX_STEP_MARGIN);
            
            _entityBehaviors = new HashedArray<EntityBehavior>(1024);

            // We're done here
            _initialized = true;
        }
        
        // Register an entity behavior
        public static void RegisterBehavior(EntityBehavior behavior) {
            // Add the behavior to the collection and initialize it
            Instance._entityBehaviors.Add(behavior);
            behavior.OnRegister();
        }
        
        // Unregister an entity behavior
        public static void UnregisterBehavior(EntityBehavior behavior) {
            // Exit if the behavior is not registered
            if (!Instance._entityBehaviors.Contains(behavior)) return;
            
            // Remove the behavior
            Instance._entityBehaviors.Remove(behavior);
            behavior.Terminate();
        }
        
        // Unity Update
        private void Update() {        
            // Calculate minimum of delta time and max delta
            var deltaTime = Mathf.Min(Time.deltaTime, _stepMaxDelta);
            
            // Add delta time to the accumulator
            _stepAccumulator += deltaTime;
            
            // Step the physics simulation and callback as needed
            while (_stepAccumulator >= FIXED_TIMESTEP) {
                // Run economy before any behavior on this tick
                for (var i = 0; i < _entityBehaviors.TailIndex; i++) {
                    var behavior = _entityBehaviors[i];
                    if (behavior == null || !behavior.Registered || !behavior.enabled || behavior.Terminating) continue;
                    behavior.OnEconomyUpdate(FIXED_TIMESTEP, SteamNetManager.Instance.Hosting);
                }

                // Invoke SIM update callback on all objects
                // No touchy, leave as a for-loop for optimization
                for (var i = 0; i < _entityBehaviors.TailIndex; i++) {
                    var behavior = _entityBehaviors[i];
                    if (behavior == null || !behavior.Registered || !behavior.enabled || behavior.Terminating) continue;
                    behavior.OnSimUpdate(FIXED_TIMESTEP, SteamNetManager.Instance.Hosting);
                }
                
                // Simulate physics once callbacks have run
                Physics.Simulate(deltaTime);
                
                // Subtract delta time from the accumulator
                _stepAccumulator -= deltaTime;
                
                // Increment sim frame count
                SimTickCount++;
            }
            
            // Invoke the render update callback
            // No touchy, leave as a for-loop for optimization
            for (var i = 0; i < _entityBehaviors.TailIndex; i++) {
                var behavior = _entityBehaviors[i];
                if (behavior == null || !behavior.Registered || !behavior.enabled || behavior.Terminating) continue;
                behavior.OnRenderUpdate(Time.deltaTime);
            }
        }
    }
}