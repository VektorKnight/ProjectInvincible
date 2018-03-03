using UnityEngine;

namespace InvincibleEngine.Components.Generic {
	public class WaterFloat : MonoBehaviour {
		//Test code for floating various objects in water
		private void OnTriggerStay(Collider other) {
			var rigidBody = other.GetComponent<Rigidbody>();
			
			// Float the object roughly at center above the water height
			if (!(other.transform.position.y < transform.position.y)) return;
			
			rigidBody?.AddForce(-Physics.gravity * rigidBody.mass);
			rigidBody?.AddTorque(Vector3.Cross(rigidBody.transform.up, -Physics.gravity.normalized) * rigidBody.mass, ForceMode.Force);
		}
	}
}
