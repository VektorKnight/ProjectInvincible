using UnityEngine;

namespace InvincibleEngine.Components.Generic {
	[RequireComponent(typeof(AudioSource))]
	public class CollisionSounds : MonoBehaviour {
		
		// Unity Inspector
		[SerializeField] private AudioClip _collisionSound;
		[SerializeField] private float _maxImpulse = 2500f;
		[SerializeField] private float _timeDelay = 0.1f;
		
		// Private
		private AudioSource _audioSource;
		private bool _canPlayAudio = true;
		private float _delayTimer = 0f;
		
		// Initialization
		private void Start() {
			_audioSource = GetComponent<AudioSource>();
		}
		
		// Unity Update
		private void Update() {
			if (_delayTimer > 0f) _delayTimer -= Time.deltaTime;
		}
		
		// Collision Event
		private void OnCollisionEnter(Collision collision) {
			if (_delayTimer > 0f) return;
			var volume = Mathf.Clamp01(collision.impulse.magnitude / _maxImpulse);
			_audioSource.PlayOneShot(_collisionSound, volume);
			_delayTimer = _timeDelay;
		}
	}
}
