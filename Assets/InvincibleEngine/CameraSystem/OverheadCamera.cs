using System;
using System.Collections.Generic;
using UnityEngine;
using InvincibleEngine;
using InvincibleEngine.UnitFramework.Components;
using VektorLibrary.EntityFramework.Components;
using VektorLibrary.Math;
using VektorLibrary.Utility;


namespace InvincibleEngine {
    /// <summary>
    /// Overhead camera suitable for RTS-style games.
    /// Author: VektorKnight
    /// </summary>
    public class OverheadCamera : EntityBehavior {
        // TEMPORARY
        public static OverheadCamera Instance;
        
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
        [SerializeField] private LayerMask _geometryMask;
        [SerializeField] private bool _zoomToCursor = true;
        [SerializeField] private bool _enableEdgeScroll = true;
        [SerializeField] private bool _enableRotation = true;

        [Header("Required Objects")] 
        [SerializeField] private Camera _camera;
        
        // Private: State
        private bool _initialized;
        
        // Private: View
        private float _panSpeed;
        private float _heightValue;
        private float _pitchValue;
        private float _zOffset;
        
        // Private: Input
        private Vector3 _inputValues;
        private float _zoomValue, _refV;
        private LowPassFloat _scrollWheel;
        private Vector2 _mousePrevious;
        private Vector2 _mouseCurrent;
        
        // Private: Mouse Smoothing
        private LowPassFloat _smoothX;
        private LowPassFloat _smoothY;
        
        // Private: Cursor Position / Target
        private Vector3 _targetPosition;
        private Vector2 _screenCenter;
        private Vector3 _refVt;
        
        // Public: Useful Properties
        public HashSet<UnitBehavior> VisibleObjects { get; private set; }
        public Vector3 MouseWorld { get; private set; }
        public Camera PlayerCamera => _camera;
        public Plane[] FrustrumPlanes { get; private set; }

        // Initialization
        public override void OnRegister() {            
            // Ensure the camera reference has been set
            if (_camera == null) {
                Debug.LogWarning($"{name}: The required camera reference is missing, please check your configuration!");
                return;
            }
            
            // Initialize frustrum planes array
            FrustrumPlanes = new Plane[6];
            
            // Initialize mouse low-pass filters
            _scrollWheel = new LowPassFloat(8);
            _smoothX = new LowPassFloat(2);
            _smoothY = new LowPassFloat(2);
            
            // Initialize on-screen objects set
            VisibleObjects = new HashSet<UnitBehavior>();

            Instance = this;
            
            DebugReadout.AddField("Visible Units");
            
            // Call base method
            base.OnRegister();
        }
        
        // FixedUpdate
        public override void OnSimUpdate(float fixedDelta, bool isHost) {
            var mouseRay = _camera.ScreenPointToRay(Input.mousePosition);

            // Perform a raycast from the mouse position to the game world
            RaycastHit rayHit;
            var hasHit = Physics.Raycast(mouseRay, out rayHit, 1024, _geometryMask);

            // Update the mouse world position if the raycast hit something
            MouseWorld = hasHit ? new Vector3(rayHit.point.x, transform.position.y, rayHit.point.z) : transform.position;
        }

        // Render Update Callback
        public override void OnRenderUpdate(float deltaTime) {
            // Update debug readout
            DebugReadout.UpdateField("Visible Units", VisibleObjects.Count.ToString());
            
            // Update frustrum planes
            GeometryUtility.CalculateFrustumPlanes(_camera, FrustrumPlanes);
            
            // Update current mouse position
            _mouseCurrent = Input.mousePosition;

            // Branch for pan controls (mouse vs keyboard)
            if (Input.GetMouseButton(2)) {
                // Apply low-pass filtering to mouse deltas
                _smoothX.AddSample((_mousePrevious.x - _mouseCurrent.x) * (_panSpeed / 6f) * Time.deltaTime);
                _smoothY.AddSample((_mousePrevious.y - _mouseCurrent.y) * (_panSpeed / 6f) * Time.deltaTime);
                
                // Create delta value
                var dPos = new Vector3(_smoothX, 0f, _smoothY);
                
                // Apply delta to rig transform
                transform.position += dPos;
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
            _mousePrevious = _mouseCurrent;
            
            // Get zoom input values
            _scrollWheel.AddSample(Input.GetAxis("Mouse ScrollWheel"));
            _inputValues.y += _scrollWheel * _zoomSpeed * Time.deltaTime;
            _inputValues.y = Mathf.Clamp01(_inputValues.y);
            _zoomValue = Mathf.SmoothDamp(_zoomValue, _inputValues.y, ref _refV, _zoomSmoothing * Time.deltaTime);
            
            // Interpolate control values
            _panSpeed = Mathf.Lerp(_panSpeedRange.y, _panSpeedRange.x, _zoomValue);
            _heightValue = Mathf.Lerp(_heightRange.y, _heightRange.x, _zoomValue);
            _pitchValue = Mathf.Lerp(_pitchRange.y, _pitchRange.x, _zoomValue);
            
            // Move towards cursor if enabled and zooming
            if (_zoomToCursor && _scrollWheel > 0f && _zoomValue < 0.98f) {
                // Recalculate screen center
                _screenCenter.x = Screen.width / 2f;
                _screenCenter.y = Screen.height / 2f;
                
                // Calculate direction and distance of mouse cursor from center
                var mouseDirection = (_mouseCurrent - _screenCenter);
                var mouseDistance = mouseDirection.magnitude;
                mouseDirection = mouseDirection.normalized;

                transform.position += new Vector3(mouseDirection.x, 0f, mouseDirection.y) * _panSpeed * (1f - _zoomValue) * Time.deltaTime;
            }

            // Calculate camera z-offset with trig (black magic)
            _zOffset = Mathf.Tan((_pitchValue + 90f) * Mathf.Deg2Rad) * _heightValue;
            
            // Apply rig and camera transform values
            transform.position = new Vector3(transform.position.x + _inputValues.x, 0f, transform.position.z + _inputValues.z);
            _camera.transform.localPosition = new Vector3(0f, _heightValue, _zOffset);
            _camera.transform.localRotation = Quaternion.Euler(_pitchValue, 0f, 0f);
        }
    }
}