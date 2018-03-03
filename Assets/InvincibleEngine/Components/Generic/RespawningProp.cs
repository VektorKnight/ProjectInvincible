using UnityEngine;

namespace InvincibleEngine.Components.Generic {
	/// <summary>
	/// Add to any rigidbody-enabled prop/object to have it revert to its intial position/rotation
	/// after a specified delay.
	/// </summary>
	[RequireComponent(typeof(Rigidbody))]
	public class RespawningProp : MonoBehaviour {
		
		// Unity Inspector: Respawn Conditions
		[Header("Respawn Conditions")] 
		[SerializeField] private float _respawnTimer = 120.0f;	// Respawning will be attempted on this interval (seconds)
		[SerializeField] private float _maxDeltaPosition = 1.0f; 	// Respawning will not occur if delta position is less than this value
		
		// Unity Inspector: Respawn Options
		[Header("Respawn Settings")] 
		[SerializeField] private bool _resetRigidbody = true;	// Will reset the velocity vectors of the attached rigidbody
		
		// Private: Initial Position/Rotation/Rigidbody
		private Rigidbody _rigidBody;
		private Vector3 _initialPosition;
		private Quaternion _initialRotation;

		// Initialization
		private void Start() {
			// Grab the intial transform values & rigidbody
			_rigidBody = GetComponent<Rigidbody>();
			_initialPosition = transform.position;
			_initialRotation = transform.rotation;
			
			// Sanity check on respawn conditions
			if (_respawnTimer < 1f) {
				Debug.LogWarning($"{name}: Has a very small respawn interval of {_respawnTimer} seconds and will be clamped to a value of 1 second! \n" +
				                 $"Consider increasing the respawn interval to avoid performance/gameplay issues.");
				_respawnTimer = 1f;
			}
			
			// Invoke the respawn function after the specified delay
			InvokeRepeating(nameof(Respawn), _respawnTimer, _respawnTimer);
		}
		
		// Respawn Function
		private void Respawn() {
			// Check if delta position is within tolerance
			var deltaPos = Vector3.Distance(_initialPosition, transform.position);
			if (!(deltaPos > _maxDeltaPosition)) return;
			_rigidBody.MovePosition(_initialPosition);
			_rigidBody.MoveRotation(_initialRotation);
			_rigidBody.velocity = Vector3.zero;
			_rigidBody.angularVelocity = Vector3.zero;
		}
	}
}
