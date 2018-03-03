using UnityEngine;

namespace InvincibleEngine.Components.Generic {
	public class GravliftPhysics : MonoBehaviour {
		
		// Unity Inspector
		[SerializeField] private float _acceleration = 10.0f;	// Acceleration (m/s) exerted on the object
		[SerializeField] private float _multiplier = 1.0f;		// Acceleration is multiplied by this value

		private void OnTriggerStay(Collider other) {
			var rigidBody = other.GetComponent<Rigidbody>();
			rigidBody.AddForce(transform.forward * _acceleration * _multiplier, ForceMode.Acceleration);
		}
	}
}
