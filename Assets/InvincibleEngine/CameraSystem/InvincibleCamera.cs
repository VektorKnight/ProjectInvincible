using System;
using System.Collections.Generic;
using InvincibleEngine.DataTypes;
using InvincibleEngine.UnitFramework.Components;
using InvincibleEngine.UnitFramework.DataTypes;
using VektorLibrary.EntityFramework.Components;
using VektorLibrary.Math;
using VektorLibrary.Utility;
using UnityEngine;

namespace InvincibleEngine.CameraSystem {
    /// <summary>
    /// Overhead camera suitable for RTS-style games.
    /// Author: VektorKnight
    /// </summary>
    public class InvincibleCamera : EntityBehavior {
        // Single object instance
        public static InvincibleCamera Instance;
        
        // Unity Inspector
        [Header("Camera View (Overhead)")]
        [SerializeField] private Vector2 _heightRange = new Vector2(20f, 200f);
        [SerializeField] private Vector2 _pitchRange = new Vector2(60f, 90f);

        [Header("Camera View (Orbital)")] 
        [SerializeField] private Vector2 _heightRangeOrbit = new Vector2(1f, 200f);
        [SerializeField] private Vector2 _pitchRangeOrbit = new Vector2(0f, 90f);
        [SerializeField] private float _orbitModeMaximum = 0.25f;

        [Header("Camera Movement")] 
        [SerializeField] private Vector2 _panSpeedRange = new Vector2(5f, 50f);
        [SerializeField] private float _zoomSpeed = 120f;
        [SerializeField] private float _rotateSpeed = 10f;
        [SerializeField] private float _zoomSmoothing = 20f;
        [SerializeField] private int _edgeScrollBuffer = 6;

        [Header("Camera Features")] 
        [SerializeField] private float _fadeTime = 0.25f;
        [SerializeField] private LayerMask _geometryMask;
        [SerializeField] private bool _zoomToCursor = true;
        [SerializeField] private bool _enableEdgeScroll = true;
        [SerializeField] private bool _enableRotation = true;

        [Header("Icon Rendering")] 
        [SerializeField] [Range(0f, 1f)] private float _iconThreshold;
        [SerializeField] [Range(0f, 1f)] private float _healthBarThreshold;

        [Header("Required Objects")] 
        [SerializeField] private Camera _camera;
        
        // Private: State
        private bool _initialized;
        
        // Private: View
        private float _panSpeed;
        private float _pitchValue;
        private float _zOffset;
        private HashSet<UnitBehavior> _visibleObjects;
        private Plane[] _frustrumPlanes;
        
        // Private: Input
        private Vector3 _mouseInput;
        private Vector3 _controlValues;
        private float _zoomValue, _refV;
        private MouseData _mouseData;
        
        // Private: Mouse Smoothing
        private LowPassFloat _smoothX;
        private LowPassFloat _smoothY;
        
        // Private: Orbit Mode
        private bool _orbitMode;
        private Vector3 _orbitPivot;
        private Vector2 _orbitInput;
        private Vector3 _originalPosition;
        private Vector3 _originalCameraPosition;
        private Quaternion _originalRotation;
        private Vector3 _orbitMinPoint;
        private Vector3 _orbitMaxPoint;
        
        // Private: Icon Rendering
        private Canvas _iconCanvas;
        private bool _iconsRendered;
        private bool _healthBarsRendered;
        
        // Public Static: Useful Properties
        public static HashSet<UnitBehavior> VisibleObjects => Instance._visibleObjects;
        public static Plane[] FrustrumPlanes => Instance._frustrumPlanes;
        public static MouseData MouseData => Instance._mouseData;
        public static Camera PlayerCamera => Instance._camera;
        public static float ZoomLevel => Instance._zoomValue;
        public static bool IconsRendered => Instance._iconsRendered;
        public static bool HealthBarsRendered => Instance._healthBarsRendered;

        // Initialization
        public override void OnRegister() {
            // Ensure this camera is the only instance in the scene
            if (Instance == null) { Instance = this; }
            else if (Instance != this) { Destroy(gameObject); }
            
            // Ensure the camera reference has been set
            if (_camera == null) {
                Debug.LogWarning($"{name}: The required camera reference is missing, please check your configuration!");
                return;
            }
            
            // Load and instantiate the icon canvas
            var iconCanvasPrefab = Resources.Load<GameObject>("Objects/Common/UnitIconCanvas");
            _iconCanvas = Instantiate(iconCanvasPrefab, Vector3.zero, Quaternion.identity).GetComponent<Canvas>();
            
            // Ensure the camera object is centered and aligned
            _camera.transform.localPosition = Vector3.zero;
            _camera.transform.localRotation = Quaternion.identity;
            
            // Calculate orbit mode min/max camera bounds
            _orbitMinPoint = Vector3.back * _heightRangeOrbit.x;
            _orbitMaxPoint = Vector3.back * _heightRangeOrbit.y;
            
            // Initialize frustrum planes array
            _frustrumPlanes = new Plane[6];
            
            // Initialize mouse low-pass filters
            _smoothX = new LowPassFloat(2);
            _smoothY = new LowPassFloat(2);
            
            // Initialize on-screen objects set
            _visibleObjects = new HashSet<UnitBehavior>();
            
            // Call base method
            base.OnRegister();
        }
        
        // Appends a unit icon to the canvas
        public static void AppendElement(UnitScreenSprite icon) {
            if (icon == null || Instance == null) return;
            icon.SetParent(Instance._iconCanvas.transform);
        }
        
        // Returns the screen position of a given transform
        public static Vector2 GetScreenPosition(Vector3 position) {
            return Instance._camera.WorldToScreenPoint(position);
        }
        
        // Sim Update Callback
        public override void OnSimUpdate(float fixedDelta, bool isHost) {
            var mouseRay = _camera.ScreenPointToRay(Input.mousePosition);

            // Perform a raycast from the mouse position to the game world
            RaycastHit rayHit;
            var hasHit = Physics.Raycast(mouseRay, out rayHit, 2048, _geometryMask);

            // Update the mouse world position if the raycast hit something
            _mouseData.WorldPosition = hasHit ? rayHit.point : transform.position;
            _mouseData.HoveredObject = hasHit ? rayHit.collider.gameObject : null;
        }

        // Render Update Callback
        public override void OnRenderUpdate(float deltaTime) {
            // Update debug readout
            DevReadout.UpdateField("[Cam] Visible Units", VisibleObjects.Count.ToString());
            DevReadout.UpdateField("[Cam] Zoom level", $"{ZoomLevel:n1}");
            DevReadout.UpdateField("[Cam] View Angle", $"{_pitchValue:n0}");
            
            // Update frustrum planes
            GeometryUtility.CalculateFrustumPlanes(_camera, FrustrumPlanes);
            
            // Update current mouse position
            _mouseData.ScreenPosition = Input.mousePosition;
            
            // Update mouse input values
            _mouseInput.x = Input.GetAxis("Mouse X");
            _mouseInput.y = Input.GetAxis("Mouse Y");
            _mouseInput.z = Input.GetAxis("Mouse ScrollWheel");
            
            // Update keyboard input values
            _controlValues.x = Input.GetAxis("Horizontal");
            _controlValues.z = Input.GetAxis("Vertical");
           
            // Update icon canvas rendering
            _iconsRendered = _zoomValue <= _iconThreshold && !_orbitMode;
            _healthBarsRendered = _zoomValue >= _healthBarThreshold && !_orbitMode;
            
            // Check for orbit mode start
            if (Input.GetKeyDown(KeyCode.Space)) {
                // Lock the cursor
                Cursor.lockState = CursorLockMode.Locked;
                
                // Find world point at center of screen
                var screenRay = _camera.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0f));
                
                // Perform a raycast from screen center to the game world
                RaycastHit rayHit;
                var hasHit = Physics.Raycast(screenRay, out rayHit, 2048, _geometryMask);
                
                // Exit if there's nothing to pivot around
                if (!hasHit) return;
                
                // Set initial orbit mode control values
                _orbitInput.x = _pitchValue;
                _orbitInput.y = 0f;
                
                // Cache current transform values and world position at center of view
                _originalCameraPosition = _camera.transform.position;
                _originalPosition = transform.position;
                _originalRotation = transform.rotation;
                _orbitPivot = rayHit.point + Vector3.up;
                
                // Convert the camera setup to a 3rd person configuration
                transform.position = _orbitPivot;
                transform.rotation = Quaternion.Euler(_orbitInput.x, _orbitInput.y, 0f);
                _camera.transform.localPosition = transform.InverseTransformPoint(_originalCameraPosition);
                _camera.transform.localRotation = Quaternion.identity;
                
                // Set orbit mode flag
                _orbitMode = true;
            }
            
            // Check for continuing orbit mode input
            if (_orbitMode && Input.GetKey(KeyCode.Space) && !Input.GetKey(KeyCode.Mouse2)) {
                // Calculate rotation angles
                _orbitInput.x += -_mouseInput.y * _panSpeedRange.x * Time.deltaTime;
                _orbitInput.y += _mouseInput.x * _panSpeedRange.x * Time.deltaTime;
                
                // Clamp pitch value (x) to specified boundaries
                _orbitInput.x = Mathf.Clamp(_orbitInput.x, _pitchRangeOrbit.x, _pitchRangeOrbit.y);
                
                // Generate quaternion rotation from orbit rotation angles and apply it
                transform.rotation = Quaternion.Euler(_orbitInput.x, _orbitInput.y, 0f);
            }
            
            // Check for orbit mode end
            if (_orbitMode && Input.GetKeyUp(KeyCode.Space)) {
                // Unlock the cursor
                Cursor.lockState = CursorLockMode.None;
                
                // Assign original transform values
                transform.position = _originalPosition;
                transform.rotation = _originalRotation;
                
                // Set orbit mode flag
                _orbitMode = false;
            }
            
            // Check for missed input
            if (_orbitMode && !Application.isFocused) {
                // Lock the cursor
                Cursor.lockState = CursorLockMode.None;
                
                // Assign original transform values
                transform.position = _originalPosition;
                transform.rotation = _originalRotation;
                
                // Set orbit mode flag
                _orbitMode = false;
            }

            // Branch for pan controls (mouse vs keyboard)
            if (Input.GetMouseButton(2)) {
                // Apply low-pass filtering to mouse deltas
                _smoothX.AddSample(-_mouseInput.x);
                _smoothY.AddSample(-_mouseInput.y);
                
                // Create delta value from low-pass values
                var mouseDelta = new Vector3(_smoothX, 0f, _smoothY);
                
                // Apply delta to rig transform
                transform.position += Quaternion.Euler(0f, transform.eulerAngles.y, 0f) * mouseDelta;
            }
            else {
                // Construct movement vector using keyboard input values
                var moveVector = new Vector3(_controlValues.x * _panSpeed * Time.deltaTime,
                                             0f,
                                             _controlValues.z * _panSpeed * Time.deltaTime);
                
                // Apply movement to transform accounting for rig rotation
                transform.position += Quaternion.Euler(0f, transform.eulerAngles.y, 0f) * moveVector;
                
                // Check for edge scroll if enabled
                if (_enableEdgeScroll) {
                    if (Input.mousePosition.x <= _edgeScrollBuffer) { // Scroll Left
                        transform.position += transform.right * -_panSpeed * Time.deltaTime;
                    }
                    else if (Input.mousePosition.x >= Screen.width - _edgeScrollBuffer) { //Scroll Right
                        transform.position += transform.right * _panSpeed * Time.deltaTime;
                    }
                    if (Input.mousePosition.y <= _edgeScrollBuffer) { //Scroll Up
                        transform.position += transform.forward * -_panSpeed * Time.deltaTime;
                    }
                    if (Input.mousePosition.y >= Screen.height - _edgeScrollBuffer) { //Scroll Down
                        transform.position += transform.forward * _panSpeed * Time.deltaTime;
                    }
                }
            }
            
            // Smooth mouse scroll input for zooming and calculate clamped zoom value
            _controlValues.y = Mathf.SmoothDamp(_controlValues.y, _mouseInput.z, ref _refV, _zoomSmoothing);
            _zoomValue = Mathf.Clamp01(1f - (transform.position.y - _heightRange.x) / (_heightRange.y - _heightRange.x));
            
            // Interpolate control values
            _panSpeed = Mathf.Lerp(_panSpeedRange.y, _panSpeedRange.x, _zoomValue);
            _pitchValue = Mathf.Lerp(_pitchRange.y, _pitchRange.x, _zoomValue);
            
            // Handle zooming to cursor if not in orbit mode
            if (!_orbitMode && (int)Mathf.Sign(_controlValues.y) != 0) {
                // Calculate direction to mouse cursor world position
                var zoomDirection = MouseData.WorldPosition - transform.position;

                // Calculate movement vector
                var movementVector = zoomDirection.normalized * _controlValues.y * _zoomSpeed;
                
                // Cancel X/Z deltas if we are fully zoomed in or out
                if (Math.Abs(transform.position.y - _heightRange.x) < float.Epsilon || Math.Abs(transform.position.y - _heightRange.y) < float.Epsilon)
                    movementVector = new Vector3(0f, movementVector.y, 0f);

                // Apply movement vector to camera rig transform
                transform.position += movementVector;
            }
            
            // Handle zooming of camera object to pivot if in orbit mode
            if (_orbitMode && (int) Mathf.Sign(_controlValues.y) != 0) {
                // Move towards the min point if zooming in
                var targetPoint = Mathf.Sign(_controlValues.y) > 0f ? _orbitMinPoint : _orbitMaxPoint; 
                _camera.transform.localPosition = Vector3.MoveTowards(_camera.transform.localPosition, targetPoint, Mathf.Abs(_controlValues.y) * _zoomSpeed);
            }

            // Clamp camera height
            if (!_orbitMode) {
                transform.position = new Vector3(transform.position.x,
                    Mathf.Clamp(transform.position.y, _heightRange.x, _heightRange.y),
                    transform.position.z);
            }

            // Calculate camera z-offset with trig to correct for parallax error (black magic)
            _zOffset = Mathf.Tan((_pitchValue + 90f) * Mathf.Deg2Rad) * transform.position.y;
            
            // Apply desired rotation based on height to camera transform
            if (_orbitMode) return;
            _camera.transform.localRotation = Quaternion.Euler(_pitchValue, 0f, 0f);
            _camera.transform.localPosition = Vector3.forward * _zOffset;
        }
    }
}