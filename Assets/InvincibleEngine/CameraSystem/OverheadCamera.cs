using UnityEngine;
using VektorLibrary.EntityFramework.Components;
using VektorLibrary.Math;


namespace InvincibleEngine.CameraSystem {
    /// <summary>
    /// Overhead camera suitable for RTS-style games.
    /// Author: VektorKnight
    /// </summary>
    public class OverheadCamera : EntityBehavior {
        // Unity Inspector
        [Header("Camera View")]
        [SerializeField] private Vector2 _heightRange = new Vector2(10f, 100f);
        [SerializeField] private Vector2 _pitchRange = new Vector2(15f, 75f);

        [Header("Camera Movement")] 
        [SerializeField] private Vector2 _panSpeedRange = new Vector2(40f, 120f);
        [SerializeField] private float _zoomSpeed = 10f;
        [SerializeField] private float _rotateSpeed = 10f;
        [SerializeField] private float _zoomSmoothing = 10f;
        
        [Header("Camera Features")]
        [SerializeField] private bool _zoomToCursor = true;
        [SerializeField] private bool _enableEdgeScroll = true;
        [SerializeField] private bool _enableRotation = true;

        [Header("Anti-Clip Config")] 
        [SerializeField] private LayerMask _antiClipMask;
        [SerializeField] private int _maxRayLength = 512;

        [Header("Required Objects")] 
        [SerializeField] private Camera _camera;
        
        // Private: State
        private bool _initialized;
        
        // Private: View
        private float _panSpeed;
        private float _heightValue;
        private float _pitchValue;
        
        // Private: Input
        private Vector3 _inputValues;
        private float _zoomValue, _refV;
        
        // Private: Anti-Clip
        private readonly LowPassFloat _terrainHeight = new LowPassFloat(32);
        private float _heightOffset;
        
        // Initialization
        public override void OnRegister() {
            // Exit if already initialized
            if (Registered) return;
            
            // Ensure the camera reference has been set
            if (_camera == null) {
                Debug.LogWarning($"{name}: The required camera reference is missing, please check your configuration!");
                return;
            }
            
            // Call base method
            base.OnRegister();
        }
        
        // Unity Update
        public override void OnRenderUpdate(float renderDelta) {
            // Exit if not initialized
            if (!Registered) return;
            
            // Get input values
            //_inputValues.x = Input.GetAxis(_inputMap.MovementX) * _panSpeed * renderDelta;
            //_inputValues.z = Input.GetAxis(_inputMap.MovementY) * _panSpeed * renderDelta;
            //_inputValues.y += Input.GetAxis(_inputMap.ZoomAxis) * _zoomSpeed * renderDelta;
            _inputValues.y = Mathf.Clamp01(_inputValues.y);
            _zoomValue = Mathf.SmoothDamp(_zoomValue, _inputValues.y, ref _refV, _zoomSmoothing * renderDelta);
            
            // Interpolate control values
            _panSpeed = Mathf.Lerp(_panSpeedRange.y, _panSpeedRange.x, _zoomValue);
            _heightValue = Mathf.Lerp(_heightRange.y, _heightRange.x, _zoomValue);
            _pitchValue = Mathf.Lerp(_pitchRange.y, _pitchRange.x, _zoomValue);
            
            // Apply rig and camera transform values
            // TODO: A jerk occurs when zooming into geometry rapidly, should try to fix this eventually
            // TODO: Might be related to the asynchronicity of the Update and FixedUpdate callbacks
            _heightOffset = _heightValue - _heightRange.x < _terrainHeight ? _terrainHeight : 0f;
            transform.position = new Vector3( transform.position.x + _inputValues.x, 0f, transform.position.z + _inputValues.z);
            _camera.transform.localPosition = Vector3.up * (_heightValue + _heightOffset);
            _camera.transform.localRotation = Quaternion.Euler(_pitchValue, 0f, 0f);
        }
        
        // Physics Update
        public override void OnPhysicsUpdate(float physicsDelta) {
            // Ensure we stay above any geometry
            var groundCheckRay = new Ray(new Vector3(transform.position.x, _maxRayLength, transform.position.z), Vector3.down);
            RaycastHit rayHit;
            _terrainHeight.AddSample(Physics.Raycast(groundCheckRay, out rayHit, _maxRayLength, _antiClipMask) ? rayHit.point.y : _terrainHeight);
        }
    }
}