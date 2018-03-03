using System;
using InvincibleEngine.InputSystem;
using InvincibleEngine.Managers;
using InvincibleEngine.VektorLibrary.Utility;
using UnityEngine;
using XInputDotNetPure;

namespace InvincibleEngine.CameraSystem {
    /// <summary>
    /// An advanced 3rd peson camera rig with anti-clipping, input smoothing, and zoom features.
    /// Authors: VektorKnight
    /// </summary>
    public class VektorCamera : MonoBehaviour, InputListener {
        
        // Static Readonly: Player Camera Rects
        // *Indices 1 and 3 will be used for 2 players with their widths set to 1.0f
        // *Indices 1, 3, and 4 will be used for 3 players with 1 having a width of 1.0f
        public static readonly Rect[] CameraRects = new Rect[4] {
            new Rect(0f, 0.5f, 0.5f, 0.5f),   // Upper Left
            new Rect(0.5f, 0.5f, 0.5f, 0.5f), // Upper Right
            new Rect(0f, 0f, 0.5f, 0.5f),     // Lower Left
            new Rect(0.5f, 0f, 0.5f, 0.5f)    // Lower Right
        };

        // Unity Inspector
        [Header("Input Config (Keyboard/Mouse)")] 
        [SerializeField] private string _xAxis = "Mouse X";
        [SerializeField] private string _yAxis = "Mouse Y";

        [Header("Input Config (Controller)")] 
        [SerializeField] private GamepadAxis _lookAxis = GamepadAxis.RightStick;

        [Header("Camera Config")] 
        [SerializeField] private Camera _gameCamera;
        [SerializeField] private Camera _hudCamera;
        [SerializeField] private Transform _cameraAnchor;
        [SerializeField] private Vector3[] _cameraAnchors;
        
        [Header("Camera Tracking")]
        [SerializeField] private Transform _currentTarget;
        public float TrackTime = 0.025f;
        public float MaxPitch = 30f;

        [Header("Zoom Feature")] 
        public float ZoomSpeed = 360f;

        [Header("Anti-Clip Features")] 
        public bool EnableAntiClip = true;
        public float MaxDistanceDelta = 4.0f;
        public float CameraBounds = 0.5f;
        public LayerMask CheckLayer;

        [Header("Aim-Assist Features")] 
        [SerializeField] private bool _enableAimAssist = true;    // Enables or disables the aim-assist feature
        [SerializeField] private float _assistFactor = 0.66f;     // Base sensitivity is multiplied by this value when the reticle is over an enemy
        [SerializeField] private int _maxAssistDistance = 1024;   // Maximum length of the aim-assist raycast in meters
        [SerializeField] private LayerMask _assistCheckLayer;     // Layer(s) included in the raycast (default layer is 'Players')
        
        // Private: State
        private bool _initialized;
        
        // Private: Manager Reference
        private PlayerManager _playerManager;
        
        // Private: Input Handling
        private bool _inputEnabled = true;
        private Vector2 _lookInput;
        private InputSettings _inputSettings;

        // Private: Tracking and Looking
        private Vector3 _tV;
        private LowPassFloat _yawSmooth;
        private LowPassFloat _pitchSmooth;
        private float _desiredYaw;
        private float _desiredPitch;
        
        // Private: Zoom
        private float _normalFov;
        private float _zoomFov;
        
        // Private: Anti-Clip
        private float _checkDistance;
        
        // Public Readonly: Player Metadata
        public PlayerIndex PlayerIndex { get; private set; }
        
        // Public: Camera System
        public Transform CurrentTarget => _currentTarget;

        public Camera GameCamera => _gameCamera;
        public Camera HudCamera => _hudCamera;

        // Initialization
        public void Initialize(PlayerManager manager, PlayerIndex index, Transform target) {
            // Exit if already initialized
            if (_initialized) return;
            
            // Assign player index, target, and manager
            _playerManager = manager;
            PlayerIndex = index;
            SetTarget(target);
            
            // Set up the viewport rect for multiplayer
            switch (index) {
                case PlayerIndex.One:
                    // Assign the viewport rect
                    GameCamera.rect = CameraRects[0];
                    HudCamera.rect = CameraRects[0];
                    
                    // One player only, expand to fill screen
                    if (GameManager.LocalPlayerCount == 1) {
                        GameCamera.rect = new Rect(0f, 0f, 1f, 1f);
                        HudCamera.rect = new Rect(0f, 0f, 1f, 1f);
                    }
                    
                    // Two or three players, expand width to 1 (full)
                    if (GameManager.LocalPlayerCount == 2 || GameManager.LocalPlayerCount == 3) {
                        GameCamera.rect = new Rect(0f, 0.5f, 1.0f, 0.5f);
                        HudCamera.rect = new Rect(0f, 0.5f, 1.0f, 0.5f);
                    }
                    break;
                case PlayerIndex.Two:
                    // Assign the viewport rect
                    GameCamera.rect = CameraRects[1];
                    HudCamera.rect = CameraRects[1];
                    
                    // Two players, use lower left and expand width to 1 (full)
                    if (GameManager.LocalPlayerCount == 2) {
                        GameCamera.rect = new Rect(0f, 0f, 1.0f, 0.5f);
                        HudCamera.rect = new Rect(0f, 0f, 1.0f, 0.5f);
                    }
                    
                    // Three players, use lower left
                    if (GameManager.LocalPlayerCount == 3) {
                        GameCamera.rect = CameraRects[2];
                        HudCamera.rect = CameraRects[2];
                    }
                    break;
                case PlayerIndex.Three:
                    // Assign the viewport rect
                    GameCamera.rect = CameraRects[2];
                    HudCamera.rect = CameraRects[2];
                    
                    // Three players, use lower right
                    if (GameManager.LocalPlayerCount == 3) {
                        GameCamera.rect = CameraRects[3];
                        HudCamera.rect = CameraRects[3];
                    }
                    break;
                case PlayerIndex.Four:
                    // Assign the viewport rect
                    GameCamera.rect = CameraRects[3];
                    HudCamera.rect = CameraRects[3];
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            // Register input callback and fetch input settings
            InputManager.RegisterListener(this);
            _inputSettings = InputManager.GetInputSettings(index);
            
            // Initialize low-pass filters for smoothing
            _pitchSmooth = new LowPassFloat(8);
            _yawSmooth = new LowPassFloat(8);
            
            // Lock the cursor
            Cursor.lockState = CursorLockMode.Locked;
            
            // Set the camera to the anchor position and target rotation
            _gameCamera.transform.position = _cameraAnchor.position;
            transform.rotation = target.rotation;
            _normalFov = _gameCamera.fieldOfView;
            _zoomFov = _normalFov;
            
            // Make sure we have a target or fall back to self
            if (_currentTarget == null) {
                _currentTarget = transform;
                Debug.LogWarning($"Camera Rig <b>{name}</b> with player index <b>{(int)PlayerIndex}</b> could not locate a matching player entity!");
            }
           
            // Calculate distance between target and camera anchor
            _checkDistance = Vector3.Distance(transform.position, _cameraAnchor.position);
            
            // Subscribe to the Gameplay UI menu event
            GameplayUI.Instance.OnMenuToggled += OnMenuToggled;
            
            // Initialization complete
            _initialized = true;
        }
        
        // Set a new target for the camera
        public void SetTarget(Transform target) {
            _currentTarget = target;
            _desiredPitch = target.eulerAngles.x;
            _desiredYaw = target.eulerAngles.y;
        }
        
        // Set the current zoom level
        public void SetZoomLevel(float ratio) {
            _zoomFov = _normalFov * ratio;
        }
        
        // Handle a menu toggle event from the UI
        private void OnMenuToggled() {
            _inputEnabled = !_inputEnabled;
            
            // Lock/Unlock the cursor
            Cursor.lockState = _inputEnabled ? CursorLockMode.Locked : CursorLockMode.None;
        }
        
        // Input Update
        public void InputUpdate() {
            // Exit if not initialized
            if (!_initialized) return;
            
            // Grab appropriate input source and calculate the desired view angles with smoothing
            if (_inputSettings.KeyboardInput && PlayerIndex == PlayerIndex.One) {
                // Keyboard input
                _lookInput.x = _inputEnabled ? Input.GetAxis(_xAxis) : 0f;
                _lookInput.y = _inputEnabled ? Input.GetAxis(_yAxis) : 0f;
            }
            else {
                // Controller input
                _lookInput = _inputEnabled ? InputManager.GetAxis(PlayerIndex, _lookAxis) : Vector2.zero;
            }
        }
        
        // Late Update
        private void LateUpdate () {
            // Exit if not initialized
            if (!_initialized) return;
            
            // Relock the cursor with L (debugging)
            if (Input.GetKeyDown(KeyCode.L)) {
                Cursor.lockState = Cursor.lockState == CursorLockMode.Locked ? CursorLockMode.None : CursorLockMode.Locked;
            }
            
            // Fallback if the current target reference is somehow lost
            if (_currentTarget == null) _currentTarget = transform;
            
            // Follow the target
            transform.position = Vector3.SmoothDamp(transform.position, _currentTarget.position, ref _tV, TrackTime);
            
            // Adjust the view
            var sensitivity = _inputSettings.Sensitivity * (_zoomFov / _normalFov);
            if (_enableAimAssist) AimAssist(ref sensitivity);
            _desiredPitch += _pitchSmooth.GetFilteredValue(_lookInput.y * sensitivity * (1f - GameCamera.aspect) * GameCamera.rect.height) * Time.deltaTime;
            _desiredYaw += _yawSmooth.GetFilteredValue(_lookInput.x * sensitivity * GameCamera.rect.width) * Time.deltaTime;
            
            // Clamp pitch to specified range
            _desiredPitch = Mathf.Clamp(_desiredPitch, -MaxPitch, MaxPitch);
            
            // Apply the desired rotation
            transform.rotation = Quaternion.Euler(_desiredPitch, _desiredYaw, transform.eulerAngles.z);

            // Handle camera zoom function
            _gameCamera.fieldOfView = Mathf.MoveTowards(_gameCamera.fieldOfView, _zoomFov, ZoomSpeed * Time.deltaTime);
            
            // Handle anti-clip function
            if (EnableAntiClip) AntiClip();
        }
        
        // Mitigate Camera Clipping
        private void AntiClip() {      
            // Calculate the vector between the current target and camera anchor
            var checkVector = (_cameraAnchor.position - _currentTarget.position).normalized;
            
            // Set up the check raycast
            var checkRay = new Ray(_currentTarget.position, checkVector);
            RaycastHit checkHit;
            
            // Check if geometry is obstructing the view
            if (Physics.Raycast(checkRay, out checkHit, _checkDistance, CheckLayer)) {
                // Make sure we don't go too far from the anchor position
                if (Vector3.Distance(_gameCamera.transform.position, _cameraAnchor.position) > MaxDistanceDelta) return;

                // Move the camera to a position where the view is no longer obstructed
                _gameCamera.transform.position = checkHit.point - (checkVector * CameraBounds);
            }
            else {
                // Restore the camera to its default position
                _gameCamera.transform.position = _cameraAnchor.position;
            }  
        }
        
        // Aim-Assist Routine
        // TODO: Works but could definitely use some improvement (low-priority)
        private void AimAssist(ref float sensitivity) {
            // Calculate the assist check sphere
            var checkRay = _gameCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            var validTarget = Physics.SphereCast(checkRay, 1.0f, _maxAssistDistance, _assistCheckLayer);
            sensitivity = validTarget ? sensitivity * _assistFactor : sensitivity;
        }
    }
}
