using UnityEngine;

namespace InvincibleEngine.SelectionSystem {
	/// <summary>
	/// Modified variant of a glowing outline system created by Dan Moran.
	/// </summary>
	public class GlowingObject : MonoBehaviour
	{	
		// Unity Inspector
		public float LerpFactor = 10;
		
		// Property: Renderers
		public Renderer[] Renderers { get; private set; }
		
		// Property: Current Color
		public Color CurrentColor => _currentColor;
		
		// Private: Colors
		private Color _currentColor;
		private Color _targetColor;
		
		// Unity Initialization
		private void Start()
		{
			Renderers = GetComponentsInChildren<Renderer>();
			GlowController.RegisterObject(this);
			_targetColor = Color.white;
		}
		
		// Set new glow color
		public void SetTargetColor(Color color) {
			_targetColor = color;
			enabled = true;
		}

		/// <summary>
		/// Update color, disable self if we reach our target color.
		/// </summary>
		private void Update()
		{
			_currentColor = Color.Lerp(_currentColor, _targetColor, Time.deltaTime * LerpFactor);

			if (_currentColor.Equals(_targetColor))
			{
				enabled = false;
			}
		}
	}
}
