using System;
using InvincibleEngine.Components.Units;
using InvincibleEngine.UnitFramework.Enums;
using UnityEngine;
using UnityEngine.AI;

namespace GameAssets.Resources.Objects.Units.TankPrimitive {
	[RequireComponent(typeof(NavMeshAgent))]
	[RequireComponent(typeof(LineRenderer))]
	public class TankPrimitiveBehavior : LandUnitBehavior {
		
		// Unity Inspector
		[Header("Debugging")] 
		[SerializeField] private bool _showPath = true;
		
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
			
			// Reference required components
			_navAgent = GetComponent<NavMeshAgent>();
			_lineRenderer = GetComponent<LineRenderer>();
			
			// Initialize line renderer
			_lineRenderer.useWorldSpace = true;
			_lineRenderer.positionCount = 1;
			_lineRenderer.SetPosition(0, transform.position);
			
			base.OnRegister();
		}
		
		// Command Handler: Move/AMove
		private void MoveCommandHandler(object data) {
			// Verify data is expected type
			if (!(data is Vector3)) return;
			
			// Set the navagent destination
			_navAgent.SetDestination((Vector3) data);
		}
	}
}
