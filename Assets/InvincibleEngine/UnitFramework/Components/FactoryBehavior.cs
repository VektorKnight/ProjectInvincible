using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using InvincibleEngine.UnitFramework.DataTypes;
using InvincibleEngine.UnitFramework.Enums;
using SteamNet;
using UnityEngine;
using UnityEngine.AI;

namespace InvincibleEngine.UnitFramework.Components {
    public class FactoryBehavior : StructureBehavior {
        // Unity Inspector
        [Header("Unit Factory Config")] 
        [SerializeField] private List<UnitBehavior> _buildableUnits;    // Units that this factory can build
        [SerializeField] private Transform _buildSpawn;                 // Local position where units will spawn
        [SerializeField] private Transform _exitWaypoint;               // Completed units will move towards this to exit
        [SerializeField] private float _buildDelay = 2f;                // How long the factory pauses before building another unit
        [SerializeField] private ParticleSystem _buildEffect;           // Particle system that players while a unit builds

        /// <summary>
        /// The build list stores key-value pairs for each build task.
        /// The key is the unit to build and the value is the count to be built.
        /// </summary>
        public IReadOnlyList<UnitBehavior> BuildableUnits => _buildableUnits.AsReadOnly();
        public List<KeyValuePair<UnitBehavior, int>> BuildList { get; protected set; }
        public UnitBehavior CurrentUnit { get; protected set; }
        public bool ReadyToBuild { get; protected set; }
        public bool PauseBuilding = false;
        public bool LoopQueue = false;

        // Build Timer
        protected float BuildDelayTimer;

        // Build List Event Callbacks
        public delegate void BuildListChanged();
        public event BuildListChanged OnBuildListChanged;

        // Entity Callback Overrides
        #region Entity Callback Overrides
        // Initialization
        public override void OnRegister() {
            // Call base method
            base.OnRegister();

            // Check for invalid configuration
            if (_buildSpawn == null || _exitWaypoint == null) {
                Debug.LogException(new ArgumentNullException("The build spawn and exit waypoint cannot be null!"));
                OnDeath();
            }

            // Check for valid build list
            if (_buildableUnits.Count == 0) {
                Debug.LogWarning($"{name}: The build list is empty!");
            }

            // Declare supported commands
            SupportedCommands =
                UnitCommands.Move | // This will add a waypoint to recently built units
                UnitCommands.Stop | // This wil clear the current build list
                UnitCommands.Hold;  // This will pause the build list

            // Register command handlers
            CommandParser.RegisterHandler(UnitCommands.Move, MoveCommandHandler);
            CommandParser.RegisterHandler(UnitCommands.Stop, StopCommandHandler);
            CommandParser.RegisterHandler(UnitCommands.Hold, HoldCommandHandler);

            // Declare unit feature set
            Features = UnitFeatures.Factory | 
                       UnitFeatures.Defensive;

            // Initialize the build list
            BuildList = new List<KeyValuePair<UnitBehavior, int>>();
        }

        // Simulation Update
        public override void OnSimUpdate(float fixedDelta, bool isHost) {
            // Call base method
            base.OnSimUpdate(fixedDelta, isHost);

            // Process build orders
            ProcessOrders(fixedDelta);
        }

        #endregion

        // Unit Behavior Overrides
        #region Unit Behavior Overrides
        // Unit construction complete callback
        protected override void OnBuildComplete() {
            base.OnBuildComplete();
            ReadyToBuild = true;
        }

        // OnSelected Callback
        public override void OnSelected() {
            _exitWaypoint.gameObject.SetActive(true);
            base.OnSelected();
        }

        // OnDeselected Callback
        public override void OnDeselected() {
            _exitWaypoint.gameObject.SetActive(false);
            base.OnDeselected();
        }

        #endregion

        // Unit Command Handlers
        #region Unit Command Handlers
        /// <summary>
        /// Handles the 'Move' unit command.
        /// In this case it simply sets a waypoint to be issued as
        /// a move command to units once they finish building.
        /// </summary>
        protected virtual void MoveCommandHandler(object data) {
            _exitWaypoint.position = (Vector3)data;
        }

        /// <summary>
        /// Handles the 'Stop' unit command.
        /// This clears the build list and cancels any other pending actions.
        /// </summary>
        protected virtual void StopCommandHandler(object data) {
            BuildList.Clear();
        }

        /// <summary>
        /// Handles the 'Hold' unit command.
        /// This pauses the build list and any other pending actions.
        /// </summary>
        /// <param name="data"></param>
        protected virtual void HoldCommandHandler(object data) {

        }
        #endregion

        // Public Factory Methods
        #region Public Factory Methods

        /// <summary>
        /// Tries to add a new build order to the build list.
        /// </summary>
        /// <param name="unitIndex">The index of the unit in the buildable units list.</param>
        /// <param name="count">The number to build.</param>
        /// /// <returns>True if successful, false if not.</returns>
        public virtual bool TryAddOrder(int unitIndex, int count = 1) {
            // Exit if the index is outside the bounds or the entry is null
            var unit = (unitIndex >= 0 && unitIndex < _buildableUnits.Count) ? _buildableUnits[unitIndex] : null;
            if (unit == null) return false;

            // Exit if the count is less than 1
            if (count < 1) return false;

            // Edit the last order if the units are the same, otherwise add to the end
            if (BuildList.Count > 0 && BuildList[BuildList.Count - 1].Key == unit) {
                var newOrder = new KeyValuePair<UnitBehavior, int>(unit, BuildList[BuildList.Count - 1].Value + count);
                BuildList[BuildList.Count - 1] = newOrder;
            }
            else {
                var buildOrder = new KeyValuePair<UnitBehavior, int>(unit, count);
                BuildList.Add(buildOrder);
            }

            // Invoke the build list event
            OnBuildListChanged?.Invoke();
            
            // Return true for success
            return true;
        }

        /// <summary>
        /// Tries to cancel an entire build order.
        /// </summary>
        /// <param name="orderIndex">The index of the order in the build list.</param>
        /// <returns>True if successful, false if not.</returns>
        public virtual bool TryCancelOrder(int orderIndex) {
            // Exit if the index is invalid
            var valid = orderIndex >= 0 && orderIndex < BuildList.Count;
            if (!valid) return false;

            // Remove the specified order from the build list
            BuildList.RemoveAt(orderIndex);

            // Invoke the build list event
            OnBuildListChanged?.Invoke();

            // Return true for success
            return true; 
        }

        /// <summary>
        /// Tries to edit the build count of a build order.
        /// </summary>
        /// <param name="orderIndex">The index of the order in the build list.</param>
        /// <param name="count">The new count for the order.</param>
        /// <returns>True if successful, false if not.</returns>
        public virtual bool TryEditOrder(int orderIndex, int count) {
            // Exit if the index is invalid
            var valid = orderIndex >= 0 && orderIndex < BuildList.Count;
            if (!valid) return false;

            // Exit if the count is less than 1
            if (count < 1) return false;

            // Edit the order at the specified index
            var originalOrder = BuildList[orderIndex];
            var newOrder = new KeyValuePair<UnitBehavior, int>(originalOrder.Key, count);
            BuildList[orderIndex] = newOrder;

            // Invoke the build list event
            OnBuildListChanged?.Invoke();

            // Return true for success
            return true;
        }
        #endregion

        // Protected Factory Methods
        #region Protected Factory Methods
        // Called repeatedly to process the build list and related functions
        protected virtual void ProcessOrders(float deltaTime) {
            // Check the status of the current order if possible
            if (CurrentUnit != null && !ReadyToBuild) {
                // Exit if the current unit is not complete
                if (!CurrentUnit.FullyBuilt) return;

                CurrentUnit.ProcessCommand(new UnitCommand(UnitCommands.Move, _exitWaypoint.position));

                // Update the build delay timer
                if (BuildDelayTimer > 0f) {
                    BuildDelayTimer -= deltaTime;
                    return;
                }

                // Reset the current unit reference and ready flag
                CurrentUnit = null;
                ReadyToBuild = true;

                // We're done here
                return;
            }
            
            // Exit if the build list is empty
            if (BuildList.Count == 0) return;

            // Process the order at the front of the list (index 0)
            var order = BuildList[0];
            CurrentUnit = MatchManager.Instance.SpawnUnit(SteamNetManager.Instance.GetNetworkID(), order.Key.AssetID, _buildSpawn.position,
                _buildSpawn.rotation.eulerAngles, UnitTeam, SteamNetManager.LocalPlayer.SteamID);

            BuildDelayTimer = _buildDelay;

            // Decrement the order count or remove it if the count will be zero
            if (order.Value - 1 == 0) {
                TryCancelOrder(0);
            }
            else {
                TryEditOrder(0, order.Value - 1);
            }

            // Set the ready to build flag to false
            ReadyToBuild = false;
        }
        #endregion
    }
}
