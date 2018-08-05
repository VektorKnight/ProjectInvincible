﻿using System;
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
        private Vector4 _inputValues;
        private float _zoomValue, _refV;
        private Vector2 _mousePrevious;
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
            
            // Initialize frustrum planes array
            _frustrumPlanes = new Plane[6];
            
            // Initialize mouse low-pass filters
            _smoothX = new LowPassFloat(2);
            _smoothY = new LowPassFloat(2);
            
            // Initialize on-screen objects set
            _visibleObjects = new HashSet<UnitBehavior>();
            
            DebugReadout.AddField("Visible Units");
            
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
            DebugReadout.UpdateField("Visible Units", VisibleObjects.Count.ToString());
            
            // Update frustrum planes
            GeometryUtility.CalculateFrustumPlanes(_camera, FrustrumPlanes);
            
            // Update current mouse position
            _mouseData.ScreenPosition = Input.mousePosition;
           
            // Update icon canvas rendering
            _iconsRendered = _zoomValue <= _iconThreshold;
            _healthBarsRendered = _zoomValue >= _healthBarThreshold;
            
            // Check for orbit mode start
            if (Input.GetKeyDown(KeyCode.Space)) {
                // Lock the cursor
                Cursor.lockState = CursorLockMode.Locked;
                
                // Set initial orbit mode control values
                _orbitInput.x = _pitchValue;
                _orbitInput.y = 0f;
                
                // Cache current transform values and world position at center of view
                _originalCameraPosition = _camera.transform.position;
                _originalPosition = transform.position;
                _originalRotation = transform.rotation;
                _orbitPivot = _mouseData.WorldPosition;
                
                // Convert the camera setup to a 3rd person configuration
                transform.position = _orbitPivot;
                transform.rotation = Quaternion.Euler(_orbitInput.x, _orbitInput.y, 0f);
                _camera.transform.localPosition = transform.InverseTransformPoint(_originalCameraPosition);
                _camera.transform.localRotation = Quaternion.identity;
                
                // Set orbit mode flag
                _orbitMode = true;
            }
            
            // Check for continuing orbit mode input
            if (_orbitMode && Input.GetKey(KeyCode.Space)) {
                // Calculate rotation angles
                _orbitInput.x += -Input.GetAxis("Mouse Y") * _panSpeed * Time.deltaTime;
                _orbitInput.y += Input.GetAxis("Mouse X") * _panSpeed * Time.deltaTime;
                
                // Clamp pitch value (x) to specified boundaries
                _orbitInput.x = Mathf.Clamp(_orbitInput.x, _pitchRangeOrbit.x, _pitchRangeOrbit.y);
                
                // Generate quaternion rotation from orbit rotation angles and apply it
                transform.rotation = Quaternion.Euler(_orbitInput.x, _orbitInput.y, 0f);
            }
            
            // Check for orbit mode end
            if (_orbitMode && Input.GetKeyUp(KeyCode.Space)) {
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
                //_smoothX.AddSample((_mousePrevious.x - _mouseData.ScreenPosition.x) * (_panSpeed / 8f) * Time.deltaTime);
                //_smoothY.AddSample((_mousePrevious.y - _mouseData.ScreenPosition.y) * (_panSpeed / 8f) * Time.deltaTime);
                _smoothX.AddSample((_mousePrevious.x - _mouseData.ScreenPosition.x));
                _smoothY.AddSample((_mousePrevious.y - _mouseData.ScreenPosition.y));
                
                // Create delta value from low-pass values
                var mouseDelta = new Vector3(_smoothX, 0f, _smoothY);
                
                // Apply delta to rig transform
                transform.position += mouseDelta;
            }
            else {
                // Get pan input values from keyboard
                _inputValues.x = Input.GetAxis("Horizontal") * _panSpeed * Time.deltaTime;
                _inputValues.z = Input.GetAxis("Vertical") * _panSpeed * Time.deltaTime;
                
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
            
            // Update previous mouse position
            _mousePrevious = _mouseData.ScreenPosition;
            
            // Get zoom input values
            _inputValues.w = Mathf.SmoothDamp(_inputValues.w, Input.GetAxis("Mouse ScrollWheel"), ref _refV, _zoomSmoothing);
            _zoomValue = Mathf.Clamp01(1f - (transform.position.y - _heightRange.x) / (_heightRange.y - _heightRange.x));
            
            // Interpolate control values
            _panSpeed = Mathf.Lerp(_panSpeedRange.y, _panSpeedRange.x, _zoomValue);
            _pitchValue = Mathf.Lerp(_pitchRange.y, _pitchRange.x, _zoomValue);
            
            // Zoom to cursor if enabled, input is valid, and we are not in orbit mode
            if (_zoomToCursor && (int)Mathf.Sign(_inputValues.w) != 0 && !_orbitMode) {

                // Calculate direction to cursor world position
                var mouseDirection = MouseData.WorldPosition - transform.position;

                // Calculate movement vector
                var movementVector = mouseDirection.normalized * _inputValues.w * _zoomSpeed;
                
                // Cancel X/Z deltas if we are fully zoomed in or out
                if (Math.Abs(transform.position.y - _heightRange.x) < float.Epsilon || Math.Abs(transform.position.y - _heightRange.y) < float.Epsilon)
                    movementVector = new Vector3(0f, movementVector.y, 0f);

                // Apply movement vector to camera rig transform
                transform.position += movementVector;
            }

            // Apply input values to rig transform and clamp height
            transform.position = new Vector3(transform.position.x + _inputValues.x, 
                                             Mathf.Clamp(transform.position.y, _heightRange.x, _heightRange.y), 
                                             transform.position.z + _inputValues.z);
            
            // Calculate camera z-offset with trig to correct for parallax error (black magic)
            _zOffset = Mathf.Tan((_pitchValue + 90f) * Mathf.Deg2Rad) * transform.position.y;
            
            // Apply desired rotation based on height to camera transform
            if (_orbitMode) return;
            _camera.transform.localRotation = Quaternion.Euler(_pitchValue, 0f, 0f);
            _camera.transform.localPosition = Vector3.forward * _zOffset;
        }
    }
}