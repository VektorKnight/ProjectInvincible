using System.Linq;
using UnityEngine;
using VektorLibrary.AI.Systems;
using VektorLibrary.AI.Utility;
using VektorLibrary.Math;
using VektorLibrary.Utility;

namespace InvincibleEngine.Components.AI {
	/// <summary>
	/// Draft class for Base Defenders
	/// </summary>
	[RequireComponent(typeof(CharacterController))]
	public class BaseDefender : MonoBehaviour {
		
		// Unity Inspector
		[Header("AI Sensory Config")] 
		[SerializeField] private LayerMask _sensorLayers;
		[SerializeField] private int _sightRadius = 100;
		[SerializeField] private int _engageRadius = 75;
		[SerializeField] private float _sensorInterval = 0.05f;

		[Header("AI Movement Config")] 
		[SerializeField] private float _moveSpeed = 20f;
		[SerializeField] private float _turnSpeed = 20f;
		[SerializeField] private float _hoverDistance = 10f;

		[Header("AI Path Config")] 
		[SerializeField] private Transform[] _pathNodes;
		[SerializeField] private float _pathEpsilon = 0.01f;	// Distance at which the AI is considered to have reached a node
		[SerializeField] private int _maxStrayDistance = 40;	// Maximum distance the AI can stray from the current node when pursuing a target
		
		// Private: State
		private bool _initialized;
		
		// Private: Stack-Based FSM
		private StackFSM<float> _stateMachine;	// Braaaains...
		
		// Private: Required Components
		private CharacterController _characterController;
		
		// Private: Sensors
		private Transform _currentTarget;
		private float _lastScanTime;
		
		// Private: Sensor Optimization
		private float _sqrSightRadius;
		private float _sqrEngageRadius;
		
		// Private: Pathing
		private Vector3 _nodeAverage;
		private Vector3 _spawnPosition;
		private int _pathIndex;
		
		// Private: Pathing Optimization
		private float _sqrPathEpsilon;
		private float _sqrStrayDistance;
		
		// Initialization
		private void Start () {
			// Exit if already initialized
			if (_initialized) return;
			
			// Initialize the State Machine
			_stateMachine = new StackFSM<float>(PatrolState);
			
			// Reference the required components
			_characterController = GetComponent<CharacterController>();
			
			// Set the spawn position
			_spawnPosition = transform.position;
			
			// Calculate optimized sensor values
			_sqrSightRadius = Mathf.Pow(_sightRadius, 2f);
			_sqrEngageRadius = Mathf.Pow(_engageRadius, 2f);
			
			// Calculate the average position from the path nodes
			var pathVectors = _pathNodes.Select(pos => pos.position);
			_nodeAverage = _pathNodes.Length > 0 ? VektorMath.VectorAverage(pathVectors.ToArray()) : _spawnPosition;
			
			// Calculate optimized path values
			_sqrPathEpsilon = Mathf.Pow(_pathEpsilon, 2f);
			_sqrStrayDistance = Mathf.Pow(_sqrStrayDistance, 2f);
			
			// Done for now
			_initialized = true;
		}
		
		// AI State: Idle
		private void IdleState(float deltaTime) {
			
		}
		
		// AI State: Patrol (Default)
		private void PatrolState(float deltaTime) {
			// Get move delta
			Vector3 moveDelta;
			var arrivedAtNode = AIUtility.MoveTowardsPoint(transform.position, _pathNodes[_pathIndex].position, _moveSpeed, _sqrPathEpsilon, deltaTime, out moveDelta);

			// Cycle the path nodes if we've arrived at the current node
			if (arrivedAtNode) {
				// Cycle the path nodes
				_pathIndex = (_pathIndex + 1) % _pathNodes.Length;
			}
			else {
				// Move the character controller by the move delta
				_characterController.Move(moveDelta);
			}
			
			// Look at the current destination
			var targetVector = Vector3.Normalize(_pathNodes[_pathIndex].position - transform.position);
			var lookVector = Vector3.ProjectOnPlane(targetVector, Vector3.up).normalized;
			var lookRotation = Quaternion.LookRotation(lookVector);
			transform.rotation = Quaternion.RotateTowards(transform.rotation, lookRotation, _turnSpeed);
			
			// Scan for targets on an interval
			if (!(Time.time - _lastScanTime >= _sensorInterval)) return;
			GameObject[] targets;
			if (!AIUtility.ScanForObjects(transform.position, _maxStrayDistance * 0.9f, _sensorLayers, out targets)) return;
			_currentTarget = targets[0].transform;
			_stateMachine.AddTask(EngageState);
			_lastScanTime = Time.time;
		}
		
		// AI State: Engage
		private void EngageState(float deltaTime) {
			// Exit this state if the target is null or no longer active
			if (_currentTarget == null || _currentTarget.gameObject.activeSelf == false) {
				_stateMachine.RemoveTask();
				return;
			}
			
			// Exit this state if we've traversed too far from the last path node
			var pathDistance = VektorMath.PlanarDistance(_pathNodes[_pathIndex].position, transform.position, Vector3.up);
			if (pathDistance > _maxStrayDistance) {
				_stateMachine.RemoveTask();
				return;
			}
			
			// Move towards the target if they're outside the engage radius
			Vector3 moveDelta;
			var withinRange = AIUtility.MoveTowardsPoint(transform.position, _currentTarget.position, _moveSpeed, _sqrEngageRadius, deltaTime, out moveDelta);
			_characterController.Move(moveDelta);
		}
	
		// Update is called once per frame
		private void Update () {
			if (!_initialized) return;
			_stateMachine.Update(Time.deltaTime);
		}
	}
}
