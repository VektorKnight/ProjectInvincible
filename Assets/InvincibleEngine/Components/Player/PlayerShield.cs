using UnityEngine;

namespace InvincibleEngine.Components.Player {
	[RequireComponent(typeof(MeshRenderer))]
	public class PlayerShield : MonoBehaviour {
		
		// Unity Inspector
		[Header("Energy Shield Effects")] 
		[SerializeField] private ParticleSystem _burstParticles;
		[SerializeField] private ParticleSystem _rechargeParticles;
		[SerializeField] private float _flareFadeTime = 1.0f;
		[SerializeField] private Gradient _shieldGradient;
		
		// Private: Required References
		private Material _shieldMaterial;
		
		// Private: Fade Timer
		private float _fadeTimer = 0f;
		
		// Private: Shield Rendering
		private bool _renderShield = true;

		// Initialization
		private void Start () {
			// Reference the mesh renderer material
			_shieldMaterial = GetComponent<MeshRenderer>().material;
			_shieldMaterial.SetColor("_Color", _shieldGradient.Evaluate(0.0f));
		}
	
		// Unity Per-Frame Update
		private void Update () {
			// Fade out the shield if necessary
			if (_fadeTimer > 0f && _renderShield) {
				_fadeTimer -= Time.deltaTime;
				_shieldMaterial.SetColor("_Color", _shieldGradient.Evaluate(_fadeTimer/_flareFadeTime));
			}
		}

		public void FlareShield() {
			if (!_renderShield) _renderShield = true;
			if (!GetComponent<MeshRenderer>().enabled) GetComponent<MeshRenderer>().enabled = true;
			_fadeTimer = _flareFadeTime;
		}

		public void BurstShield() {
			// Play the burst particle effect
			if (_burstParticles != null) _burstParticles.Play();
			
			// Reset shield effect values
			_shieldMaterial.SetColor("_Color", _shieldGradient.Evaluate(0.0f));
			_fadeTimer = 0f;
			
			// Disable shield rendering
			_renderShield = false;
			GetComponent<MeshRenderer>().enabled = false;
		}

		public void RechargeShield() {
			// Play the recharge particle effect
			if (_rechargeParticles != null) _rechargeParticles.Play();
			
			// Reset shield effect values
			_fadeTimer = _flareFadeTime;
			
			// Enable shield rendering
			_renderShield = true;
		}
	}
}
