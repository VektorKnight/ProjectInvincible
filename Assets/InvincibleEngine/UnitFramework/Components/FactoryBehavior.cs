using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using InvincibleEngine.UnitFramework.Enums;
using UnityEngine;

namespace InvincibleEngine.UnitFramework.Components {
    class FactoryBehavior : StructureBehavior {
        // Unity Inspector
        [Header("Unit Factory Config")] 
        [SerializeField] private List<UnitBehavior> _buildableUnits;    // Units that this factory can build
        [SerializeField] private Transform _buildSpawn;                 // Local position where units will spawn
        [SerializeField] private Transform _exitWaypoint;               // Completed units will move towards this to exit
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
        }

        // Simulation Update
        public override void OnSimUpdate(float fixedDelta, bool isHost) {
            // Call base method
            base.OnSimUpdate(fixedDelta, isHost);


        }

        #endregion

        // Unit Behavior Overrides
        #region Unit Behavior Overrides
        // Unit construction complete callback
        protected override void OnBuildComplete() {
            base.OnBuildComplete();
            ReadyToBuild = true;
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

        }

        /// <summary>
        /// Handles the 'Stop' unit command.
        /// This clears the build list and cancels any other pending actions.
        /// </summary>
        protected virtual void StopCommandHandler(object data) {

        }

        /// <summary>
        /// Handles the 'Hold' unit command.
        /// This pauses the build list and any other pending actions.
        /// </summary>
        /// <param name="data"></param>
        protected virtual void HoldCommandHandler(object data) {

        }
        #endregion

        // Unit Factory Methods
        #region Unit Factory Methods

        /// <summary>
        /// Tries to add a new build order to the build list.
        /// </summary>
        /// <param name="unitIndex">The index of the unit in the buildable units list.</param>
        /// <param name="count">The number to build.</param>
        /// /// <returns>True if successful, false if not.</returns>
        public virtual bool TryAddOrder(int unitIndex, int count = 1) {
            // Exit if the index is outside the bounds or the entry is null
            var unit = (unitIndex > 0 && unitIndex < _buildableUnits.Count) ? _buildableUnits[unitIndex] : null;
            if (unit == null) return false;

            // Exit if the count is less than 1
            if (count < 1) return false;

            // Create a new build order KVP and add it to the build list
            var buildOrder = new KeyValuePair<UnitBehavior, int>(unit, count);
            BuildList.Add(buildOrder);
            
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
            var valid = orderIndex > 0 && orderIndex < BuildList.Count;
            if (!valid) return false;

            // Remove the specified order from the build list
            BuildList.RemoveAt(orderIndex);

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
            var valid = orderIndex > 0 && orderIndex < BuildList.Count;
            if (!valid) return false;

            // Exit if the count is less than 1
            if (count < 1) return false;

            // Edit the order at the specified index
            var originalOrder = BuildList[orderIndex];
            var newOrder = new KeyValuePair<UnitBehavior, int>(originalOrder.Key, count);
            BuildList[orderIndex] = newOrder;

            // Return true for success
            return true;
        }
        #endregion
    }
}
