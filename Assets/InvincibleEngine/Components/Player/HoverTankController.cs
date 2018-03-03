using InvincibleEngine.InputSystem;
using InvincibleEngine.Managers;
using InvincibleEngine.VektorLibrary.Utility;
using UnityEngine;
using XInputDotNetPure;

namespace InvincibleEngine.Components.Player {
	/// <summary>
	/// Racing physics controller designed for Hover Tanks
	/// Implements the Vektor Physics Library for Unity
	/// Copyright 2017 vektorKnight | All Rights Reserved
	/// </summary>
	[RequireComponent(typeof(PlayerEntity))]
	[RequireComponent(typeof(Rigidbody))]
	public class HoverTankController : MonoBehaviour, InputListener {
		
		// Inspector: Physics Behavior
		[Header("Physics Options")]
		[SerializeField] private bool _enableCollisionDamage;
		[SerializeField] private float _lethalImpulse = 2150f;
		[SerializeField] private AnimationCurve _damageCurve;
		
		// Inspector: Vektor Physics Components
		[Header("Physics Components")] 
		[SerializeField] private AnimationCurve _slopeCurve;
		[SerializeField] private HoverArray _hoverArray = new HoverArray();
		[SerializeField] private DirectionalEngine _mainEngine = new DirectionalEngine();
		[SerializeField] private DirectionalEngine _strafeEngine = new DirectionalEngine();
		[SerializeField] private VektorGyroscope _gyroscope = new VektorGyroscope();
		[SerializeField] private DragSurface _forwardDragSurface = new DragSurface();
		[SerializeField] private DragSurface _lateralDragSurface = new DragSurface();
		
		// Private: State
		private bool _initialized;
		
		// Private: Manager Reference
		private PlayerManager _playerManager;
		
		// Private: Input Settings
		private PlayerIndex _playerIndex;
		private InputSettings _inputSettings;
		private InputMapping _inputMapping;

		// Private: Physics & Control
		private Rigidbody _tankBody;
		private bool _inputEnabled = true;
		private Vector2 _movementInput;
		private float _engineControl;
		private Vector3 _engineVector;
		private Vector3 _gyroControl;
		private float _slopeMagnitude;
		
		// Public Readonly: Player Metadata
		public short NetworkId { get; private set; }

		// Intialization
		public void Initialize(PlayerManager manager) {
			// Exit if already initialized
			if (_initialized) return;
			
			// Assign player index
			_playerIndex = manager.PlayerIndex;
			
			// Register input callback and grab input settings
			InputManager.RegisterListener(this);
			_inputSettings = InputManager.GetInputSettings(_playerIndex);
			
			// Reference necessary components
			_playerManager = manager;
			_tankBody = GetComponent<Rigidbody>();

			// Initialize the Vektor Physics Stuff
			_hoverArray.Initialize(_tankBody);
			_mainEngine.Initialize(_tankBody);
			_strafeEngine.Initialize(_tankBody);
			_forwardDragSurface.Initialize(_tankBody);
			_lateralDragSurface.Initialize(_tankBody);
			_gyroscope.Initialize(_tankBody, Vector3.up, true);
			
			// Subscribe to the Gameplay UI menu event
			GameplayUI.Instance.OnMenuToggled += OnMenuToggled;
			
			// Initialization complete
			_initialized = true;
		}
		
		// Handle a menu toggle event from the UI
		private void OnMenuToggled() {
			_inputEnabled = !_inputEnabled;
		}

		// Per-Frame Update
		public void InputUpdate() {
			// Exit if not initialized
			if (!_initialized) return;
			
			// Grab appropriate input source
			if (_inputSettings.KeyboardInput && _playerIndex == PlayerIndex.One) {
				// Grab keyboard input
				_movementInput.x = _inputEnabled ? Input.GetAxis(_inputSettings.Map.MovementX) : 0f;
				_movementInput.y = _inputEnabled ? Input.GetAxis(_inputSettings.Map.MovementY) : 0f;
				
				// Update control values
				//_gyroControl.y = _movementInput.x;
				//_engineControl = _movementInput.y;
			}
			else {
				// Grab controller input
				_movementInput = _inputEnabled ? InputManager.GetAxis(_playerIndex, _inputSettings.Map.MovementAxis) : Vector2.zero;
			}
			
			// Calculate control values
			var cameraYaw = _playerManager.CameraSystem.transform.rotation.eulerAngles.y;
			var inputXz = new Vector3(_movementInput.x, 0f, _movementInput.y);
			var inputVector = Quaternion.AngleAxis(cameraYaw, Vector3.up) * inputXz;
			var tankDelta = Vector3.SignedAngle(transform.forward, inputVector, Vector3.up);
				
			// Update control values
			_gyroControl.y = (Mathf.Abs(_movementInput.magnitude) > _inputSettings.DeadZone) ? tankDelta / 180f : 0f;
			_engineControl = (Mathf.Abs(_movementInput.magnitude) > _inputSettings.DeadZone) ? (1f - (tankDelta / 180f)) * Mathf.Sign(_movementInput.y): 0f;
			_engineVector = inputVector;
			
			// Slope compensation based on deviation from Vector3.Up
			_slopeMagnitude = _slopeCurve.Evaluate(Vector3.Dot(transform.up, Vector3.up));
		}

		// Physics Update
		private void FixedUpdate() {
			// Exit if not initialized
			if (!_initialized) return;
			
			// Update physics components
			_hoverArray.Update(Time.fixedDeltaTime);
			_mainEngine.Update(_movementInput.magnitude * _slopeMagnitude, _engineVector);
			
			// Update engines
			if (_inputEnabled) {
				if (_inputSettings.KeyboardInput && _playerIndex == PlayerIndex.One) {
					// Keyboard input
					if (Input.GetKey(_inputSettings.Map.StrafeKeyLeft)) _strafeEngine.Update(-1f * _slopeMagnitude);
					else if (Input.GetKey(_inputSettings.Map.StrafeKeyRight)) _strafeEngine.Update(1f * _slopeMagnitude);
					else _strafeEngine.Update(0f);
				}
				else {
					// Controller input
					if (InputManager.GetButton(_playerIndex, _inputSettings.Map.StrafeButtonLeft)) _strafeEngine.Update(-1f * _slopeMagnitude);
					else if (InputManager.GetButton(_playerIndex, _inputSettings.Map.StrafeButtonRight)) _strafeEngine.Update(1f * _slopeMagnitude);
					else _strafeEngine.Update(0f);
				}
			}
			else {
				_strafeEngine.Update(0f);
			}
			
			_gyroscope.Update(_gyroControl);
			_forwardDragSurface.Update();
			_lateralDragSurface.Update();
		}
		
		/* Collision Enter
		private void OnCollisionEnter(Collision collision) {
			var impulse = collision.impulse.magnitude;
			var pE = GetComponent<PlayerEntity>();
			var damage = _damageCurve.Evaluate(impulse / _lethalImpulse) * (pE.Maxhealth + pE.MaxArmor);
			pE.ApplyDamage(damage, 0);
		}
		*/
	}
}