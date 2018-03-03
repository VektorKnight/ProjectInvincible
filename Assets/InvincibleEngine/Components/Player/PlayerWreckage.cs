using UnityEngine;

namespace InvincibleEngine.Components.Player {
	public class PlayerWreckage : MonoBehaviour {
		
		// Unity Inspector: Physics Options
		[SerializeField] private bool _applyExplosionForce = true;
		[SerializeField] private float _explosionForce = 2500f;
		[SerializeField] private Rigidbody[] _fractureObjects;

		// Initialization
		private void Start () {
			if (!_applyExplosionForce) return;
			foreach (var obj in _fractureObjects) {
				obj.AddExplosionForce(_explosionForce, transform.position, 4f);
			}
		}
	}
}
