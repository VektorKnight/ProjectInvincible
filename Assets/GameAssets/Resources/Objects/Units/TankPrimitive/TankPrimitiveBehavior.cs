using InvincibleEngine.Components.Units;
using UnityEngine;
using UnityEngine.AI;

namespace GameAssets.Resources.Objects.Units.TankPrimitive {
	[RequireComponent(typeof(NavMeshAgent))]
	[RequireComponent(typeof(LineRenderer))]
	public class TankPrimitiveBehavior : MonoBehaviour {
		
		// Unity Inspector
		[Header("Debugging")] 
		[SerializeField] private bool _showPath = true;
		
		// Required References
		private NavMeshAgent _navAgent;
		private LineRenderer _lineRenderer;

		// Use this for initialization
		private void Start () {
			// Reference required components
			_navAgent = GetComponent<NavMeshAgent>();
			_lineRenderer = GetComponent<LineRenderer>();
			
			// Initialize line renderer
			_lineRenderer.useWorldSpace = true;
			_lineRenderer.positionCount = 1;
			_lineRenderer.SetPosition(0, transform.position);
		}
	
		// Update is called once per frame
		private void Update () {
			// Check for mouse click
			if (!Input.GetKeyDown(KeyCode.Mouse0)) return;
			
			// Cast a ray from the mouse position to the world
			var mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;
			
			// Exit if the raycast doesn't hit sturf
			if (!Physics.Raycast(mouseRay, out hit)) return;
			
			// Update the pathfinding destination if the raycast hits sturf
			_navAgent.SetDestination(hit.point);
		}
	}
}
