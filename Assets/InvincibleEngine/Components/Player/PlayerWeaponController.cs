using System.Collections.Generic;
using InvincibleEngine.InputSystem;
using InvincibleEngine.Managers;
using InvincibleEngine.WeaponSystem.Components.Weapons;
using UnityEngine;
using XInputDotNetPure;

namespace InvincibleEngine.Components.Player {
	[RequireComponent(typeof(PlayerEntity))]
	public class PlayerWeaponController : MonoBehaviour, InputListener {
		
		// Unity Inspector
		[Header("Input Config (Keyboard)")] 
		[SerializeField] private KeyCode _fireKey = KeyCode.Mouse0;
		[SerializeField] private string _swapAxis = "Mouse ScrollWheel";
		[SerializeField] private KeyCode _zoomKey = KeyCode.Mouse1;
		
		[Header("Input Config (Controller)")]
		[SerializeField] private GamepadButton _fireButton = GamepadButton.B;
		[SerializeField] private GamepadButton _swapButton = GamepadButton.Y;
		[SerializeField] private GamepadButton _zoomButton = GamepadButton.RightStick;
		
		[Header("Initial Loadout & Anchor")]
		[SerializeField] private Weapon[] _weaponLoadout;
		[SerializeField] private Transform _weaponAnchor;

		[Header("Perspective Correction")] 
		[SerializeField] private float _minOffset = 0.66f;

		[Header("Aiming Config")] 
		[SerializeField] private float _aimSpeed;
		[SerializeField] private float _maxAimDistance;
		
		// Private: State
		private bool _initialized;
		
		// Private: Manager Reference
		private PlayerManager _playerManager;
		
		// Private: Player Index
		private PlayerIndex _playerIndex;
		
		// Private Members: Aiming
		private Vector3 _clampedRotation;
		
		// Private: Input Handling
		private bool _inputEnabled = true;
		private InputSettings _inputSettings;
		private float _swapInput;
		
		// Private: Weapon Switching & Active Weapon
		private int _weaponIndex;
		
		// Private: Zoom Level
		private int _zoomIndex;

		// Public Readonly : Weapon Instances & Active Weapon
		public List<Weapon> WeaponInstances { get; private set; }
		public Weapon ActiveWeapon { get; private set; }
		
		// Public Readonly: Camera Aim
		public Camera PlayerCamera { get; private set; }

		// Initialization
		public void Initialize(PlayerManager manager) {
			// Exit if already initialized
			if (_initialized) return;
			
			// Assign player index
			_playerIndex = manager.PlayerIndex;
			
			// Register input callback
			InputManager.RegisterListener(this);
			_inputSettings = InputManager.GetInputSettings(manager.PlayerIndex);
			
			// Reference necessary components
			_playerManager = manager;
			PlayerCamera = _playerManager.CameraSystem.GameCamera;
			
			// Subscribe to the Gameplay UI menu event
			try {
				GameplayUI.Instance.OnMenuToggled += OnMenuToggled;
			}
			catch (System.Exception) {
				Debug.LogWarning("The singleton instance for GameplayUI could not be found!\n" +
				                 "Please check your scene setup and the script execution order.");
			}
			
			// Initialize the weapon instances array
			WeaponInstances = new List<Weapon>();
			
			// Instantiate any pre-set loadouts as child objects and deactivate them
			foreach (var weapon in _weaponLoadout) {
				var instance = Instantiate(weapon, _weaponAnchor.position, Quaternion.identity);
				instance.transform.parent = transform;
				instance.Initialize(_playerManager.UniqueId);
				instance.gameObject.SetActive(false);
				WeaponInstances.Add(instance);
			}
			
			// Activate and assign the first weapon in the list of instances
			ActiveWeapon = WeaponInstances[0];
			ActiveWeapon.gameObject.SetActive(true);
			
			// Set the new reticle in the GameplayUI
			_playerManager.HudSystem.SwapReticle(ActiveWeapon.ReticleImage);
			
			// Initialization complete
			_initialized = true;
		}
		
		// Handle a menu toggle event from the UI
		private void OnMenuToggled() {
			_inputEnabled = !_inputEnabled;
			
			// Workaround for GetButtonUp potentially being missed on automatic weapons
			var weaponBehavior = ActiveWeapon.GetComponent<BasicWeapon>();
			if (weaponBehavior.IsFullAuto) weaponBehavior.TriggerUp();
		}
		
		// Handle swapping of weapons by index
		private void CycleWeapons(int i) {
			// Workaround for GetButtonUp potentially being missed on automatic weapons
			ActiveWeapon.TriggerUp();
			
			// Deactivate the current weapon (teleport it very far away) and store its rotation
			var weaponRotation = ActiveWeapon.transform.rotation;
			ActiveWeapon.transform.position = -Vector3.up * 10000f;
			
			// Round-robin the weapon index
			if (_weaponIndex + i < 0) _weaponIndex += WeaponInstances.Count;
			_weaponIndex = (_weaponIndex + i) % WeaponInstances.Count;
			
			// Set the new active weapon and set its rotation
			ActiveWeapon = WeaponInstances[_weaponIndex];
			ActiveWeapon.transform.SetPositionAndRotation(_weaponAnchor.position, weaponRotation);
			ActiveWeapon.gameObject.SetActive(true);
			
			// Reset the zoom level
			_zoomIndex = 0;
			_playerManager.CameraSystem.SetZoomLevel(ActiveWeapon.ZoomLevels[_zoomIndex]);
			
			// Set the new reticle in the GameplayUI
			_playerManager.HudSystem.SwapReticle(ActiveWeapon.ReticleImage);
		}
		
		// Handle weapon zoom levels by index
		private void CycleZoomLevel(int i) {
			var zoomLevels = ActiveWeapon.ZoomLevels;
			_zoomIndex = (_zoomIndex + i) % zoomLevels.Length;
			_playerManager.CameraSystem.SetZoomLevel(zoomLevels[_zoomIndex]);
		}
	
		// Per-Frame Update
		public void InputUpdate () {
			// Exit if not initialized
			if (!_initialized) return;
			
			// Keep the weapon anchored
			ActiveWeapon.transform.position = _weaponAnchor.transform.position;
			
			// Aim the weapon
			var aimingRay = new Ray(PlayerCamera.transform.position, PlayerCamera.transform.forward);
			RaycastHit aimingHit;
			
			//TODO: Could probably use some cleanup and bugfixes
			var aimVector = PlayerCamera.transform.TransformPoint(Vector3.forward * _maxAimDistance);
			var aimPoint = Physics.Raycast(aimingRay, out aimingHit, _maxAimDistance) ? aimingHit.point : aimVector;
			var aimDotP = Vector3.Dot(ActiveWeapon.transform.forward, (aimingHit.point - ActiveWeapon.transform.position).normalized);
			ActiveWeapon.transform.rotation = Quaternion.LookRotation(aimVector - ActiveWeapon.transform.position);		
			ActiveWeapon.CameraAimPoint = aimDotP > _minOffset ? aimPoint : aimVector;
			
			// Clamp weapon rotation
			_clampedRotation = WrapAngles(ActiveWeapon.transform.localEulerAngles);
			_clampedRotation.x = Mathf.Clamp(_clampedRotation.x, ActiveWeapon.PitchRange.x, ActiveWeapon.PitchRange.y);
			_clampedRotation.y = Mathf.Clamp(_clampedRotation.y, ActiveWeapon.YawRange.x, ActiveWeapon.YawRange.y);
			ActiveWeapon.transform.localRotation = Quaternion.Euler(_clampedRotation);
			
			// Grab the appropriate input source
			if (!_inputEnabled) return;
			if (_inputSettings.KeyboardInput && _playerIndex == PlayerIndex.One) {
				if (Input.GetKeyDown(_fireKey)) ActiveWeapon.TriggerDown();
				if (Input.GetKeyUp(_fireKey)) ActiveWeapon.TriggerUp();
				if (Input.GetKeyDown(_zoomKey)) CycleZoomLevel(1);
					
				// Handle scrollwheel weapon swapping
				_swapInput = _inputEnabled ? Input.GetAxis(_swapAxis) : 0f;
				if (Mathf.Abs(_swapInput) > float.Epsilon) CycleWeapons((int)Mathf.Sign(_swapInput));
			}
			else {
				// Controller input
				if (InputManager.GetButtonDown(_playerIndex, _fireButton)) ActiveWeapon.TriggerDown();
				if (InputManager.GetButtonUp(_playerIndex, _fireButton)) ActiveWeapon.TriggerUp();				
				if (InputManager.GetButtonDown(_playerIndex, _swapButton)) CycleWeapons(1);
				if (InputManager.GetButtonDown(_playerIndex, _zoomButton)) CycleZoomLevel(1);
			}
		}
	
		// Wrap Euler Angles
		private static Vector3 WrapAngles(Vector3 angles) {
			if (angles.x > 180) angles.x -= 360;
			if (angles.x < -180) angles.x += 360;
			if (angles.y > 180) angles.y -= 360;
			if (angles.y < -180) angles.y += 360;
			if (angles.z > 180) angles.z -= 360;
			if (angles.z < -180) angles.z += 360;
			return angles;
		}
	}
}
