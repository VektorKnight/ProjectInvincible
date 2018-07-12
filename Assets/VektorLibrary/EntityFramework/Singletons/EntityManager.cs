using UnityEngine;
using System;
using System.Threading;
using System.Collections;
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
        public HashedArray<IEntity> _behaviors = new HashedArray<IEntity>(1024);
        
        // Private: State
        [SerializeField] private bool _initialized;
        
        // Private: Fixed Timestep
        private float _stepMaxDelta;
        private float _stepAccumulator;
        private bool _physicsSimulated;
        
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Preload() {
            //Make sure the Managers object exists
            GameObject Managers = GameObject.Find("Managers") ?? new GameObject("Managers");

            // Ensure this singleton initializes at startup
            if (Instance == null) Instance = Managers.GetComponent<EntityManager>() ?? Managers.AddComponent<EntityManager>();

            // Ensure this singleton does not get destroyed on scene load
            DontDestroyOnLoad(Instance.gameObject);
        }

        // Initialization        
        private void Start() {
            // Exit if already initialized
            if (_initialized) return;            
          
            // Disable Unity automatic simulation
            Physics.autoSimulation = false;
            
            // Calculate max deltas
            _stepMaxDelta = FIXED_TIMESTEP * (1f + MAX_STEP_MARGIN);

            //Start Coroutine
            StartCoroutine(Tick());

            // We're done here
            _initialized = true;
        }
        
        // Register an entity behavior
        public static void RegisterBehavior(IEntity behavior) {
            // Add the behavior to the collection and initialize it
            Instance._behaviors.Add(behavior);
            Debug.Log($"Behavior <i> {behavior} </i> added");
            behavior.OnRegister();
        }
        
        // Unregister an entity behavior
        public static void UnregisterBehavior(IEntity behavior) {
            // Exit if the behavior is not registered
            if (!Instance._behaviors.Contains(behavior)) return;
            
            // Remove the behavior
            Instance._behaviors.Remove(behavior);
            behavior.Terminate();
        }

        // Unity Update
        IEnumerator Tick() {
            while (true) {

                // Exit if not initialized
                // if (!_initialized) 
                {

                    // Calculate minimum of delta time and max delta
                    var deltaTime = Mathf.Min(Time.deltaTime, _stepMaxDelta);


                    // Iterate through the behaviors
                    for (var i = 0; i < _behaviors.Count; i++) {


                        // Reference the current behavior
                        var behavior = _behaviors[i];

                        // Handle fixed timestep callbacks and physics simulation

                        // Step the physics simulation if necessary
                        if (!_physicsSimulated) {
                            Physics.Simulate(deltaTime);
                            _physicsSimulated = true;
                        }

                        // Check for any flags         
                        if (behavior == null || !behavior.Registered || behavior.Terminating) continue;

                        // Invoke the fixed timestep callbacks as necessary
                        behavior.OnPhysicsUpdate(deltaTime);
                        behavior.OnEntityHostUpdate(deltaTime);


                        // Check for any flags and invoke the render callback if necessary    
                        if (behavior == null || !behavior.Registered || behavior.Terminating) continue;
                        behavior.OnRenderUpdate(deltaTime);
                    }

                    // Reset the physics simulated flag
                    _physicsSimulated = false;                  
                }

                //Yeild Coroutine
                yield return new WaitForSeconds(0.05f);
            }
        }
    }
}