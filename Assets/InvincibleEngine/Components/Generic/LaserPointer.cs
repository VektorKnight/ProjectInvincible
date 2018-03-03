using UnityEngine;

namespace InvincibleEngine.Components.Generic {
	[RequireComponent(typeof(LineRenderer))]
	public class LaserPointer : MonoBehaviour {
		
		// Unity Inspector
		public float MaxDistance = 1024f;
		
		// Private
		private bool _enableLaser = true;
		private LineRenderer _laserRenderer;

		// Initialization
		private void Start () {
			_laserRenderer = GetComponent<LineRenderer>();
		}
	
		// Update is called once per frame
		private void FixedUpdate () {
			// Raycast to check for collisions
			var laserRay = new Ray(transform.position, transform.forward);
			RaycastHit rayhit;

			_laserRenderer.SetPosition(1, Physics.Raycast(laserRay, out rayhit, MaxDistance) ? transform.InverseTransformPoint(rayhit.point) : transform.InverseTransformDirection(transform.forward * MaxDistance));
		}
	}
}
