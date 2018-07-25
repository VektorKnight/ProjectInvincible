using InvincibleEngine.Components.Units;
using InvincibleEngine.UnitFramework.Enums;
using UnityEngine;
using UnityEngine.AI;

namespace GameAssets.Resources.Objects.Units.Land_DebugTank {
	[RequireComponent(typeof(NavMeshAgent))]
	public class DebugTankBehavior : LandUnitBehavior {
	
		// Required References
		private NavMeshAgent _navAgent;
		private LineRenderer _lineRenderer;

		// Use this for initialization
		public override void OnRegister () {
			// Declare supported commands
			SupportedCommands = UnitActions.Move |
			                    UnitActions.AMove |
			                    UnitActions.Patrol |
			                    UnitActions.Stop |
			                    UnitActions.Hold |
			                    UnitActions.Engage;
			
			// Register command handlers
			CommandParser.RegisterHandler(UnitActions.Move, MoveCommandHandler);
			CommandParser.RegisterHandler(UnitActions.AMove, MoveCommandHandler);
			CommandParser.RegisterHandler(UnitActions.Stop, StopCommandHandler);
			
			// Reference required components
			_navAgent = GetComponent<NavMeshAgent>();			

			base.OnRegister();
		}
		
		// Command Handler: Move/AMove
		private void MoveCommandHandler(object data) {
			// Verify data is expected type
			if (!(data is Vector3)) return;
			
			// Set the navagent destination
			_navAgent.SetDestination((Vector3) data);
		}
		
		// Command Handler: Stop
		private void StopCommandHandler(object data) {
			_navAgent.ResetPath();
		}
	}
}
