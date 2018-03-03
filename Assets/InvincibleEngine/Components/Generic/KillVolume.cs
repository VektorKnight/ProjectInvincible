using InvincibleEngine.WeaponSystem.Interfaces;
using UnityEngine;

namespace InvincibleEngine.Components.Generic {
	public class KillVolume : MonoBehaviour {
		
		// Unity Inspector
		[Header("Visualization (Editor Only)")] 
		[SerializeField] private bool _visualize = true;
		[SerializeField] private Color _color;

		// Collision Callback
		private void OnCollisionEnter(Collision other) {
			var entity = other.gameObject.GetComponent<IDestructable>();
			entity?.ApplyDamage(float.MaxValue, 0);
		}
		
		// Debug info for Unity editor
		#if UNITY_EDITOR
			private void OnDrawGizmos() {
				if (!_visualize) return;
				var originalColor = Gizmos.color;
				var cubeMesh = GetComponent<MeshFilter>().sharedMesh;
				Gizmos.color = _color;
				Gizmos.DrawMesh(cubeMesh, transform.position, transform.rotation, transform.localScale);
				Gizmos.color = originalColor;
			}
		#endif
	}
}
