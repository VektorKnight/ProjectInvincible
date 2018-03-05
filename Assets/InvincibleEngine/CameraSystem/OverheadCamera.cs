using InvincibleEngine.InputSystem;
using InvincibleEngine.VektorLibrary.Utility;
using UnityEngine;

namespace InvincibleEngine.CameraSystem {
    /// <summary>
    /// Overhead camera suitable for RTS-style games.
    /// Author: VektorKnight
    /// </summary>
    public class OverheadCamera : MonoBehaviour {
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
        private readonly RTSInputMap _inputMap = new RTSInputMap();
        private Vector3 _inputValues;
        private float _zoomValue, _refV;
        
        // Private: Anti-Clip
        private float _groundHeight;
        
        // Initialization
        private void Start() {
            // Exit if already initialized
            if (_initialized) return;
            
            // Ensure the camera reference has been set
            if (_camera == null) {
                Debug.LogWarning($"{name}: The required camera reference is missing, please check your configuration!");
                return;
            }
            
            // Initialization complete
            _initialized = true;
        }
        
        // Unity Update
        private void Update() {
            // Exit if not initialized
            if (!_initialized) return;
            
            // Get input values
            _inputValues.x = Input.GetAxis(_inputMap.MovementX) * _panSpeed * Time.deltaTime;
            _inputValues.z = Input.GetAxis(_inputMap.MovementY) * _panSpeed * Time.deltaTime;
            _inputValues.y += Input.GetAxis(_inputMap.ZoomAxis) * _zoomSpeed * Time.deltaTime;
            _inputValues.y = Mathf.Clamp01(_inputValues.y);
            _zoomValue = Mathf.SmoothDamp(_zoomValue, _inputValues.y, ref _refV, _zoomSmoothing * Time.deltaTime);
            
            // Interpolate control values
            _panSpeed = Mathf.Lerp(_panSpeedRange.y, _panSpeedRange.x, _zoomValue);
            _heightValue = Mathf.Lerp(_heightRange.y, _heightRange.x, _zoomValue);
            _pitchValue = Mathf.Lerp(_pitchRange.y, _pitchRange.x, _zoomValue);
            
            // Apply rig and camera transform values
            transform.position = new Vector3( transform.position.x + _inputValues.x, _groundHeight, transform.position.z + _inputValues.z);
            _camera.transform.localPosition = Vector3.up * _heightValue;
            _camera.transform.localRotation = Quaternion.Euler(_pitchValue, 0f, 0f);
        }
        
        // Physics Update
        private void FixedUpdate() {
            // Ensure we stay above any geometry
            var groundCheckRay = new Ray(new Vector3(transform.position.x, _maxRayLength, transform.position.z), Vector3.down);
            RaycastHit rayHit;
            _groundHeight = Physics.Raycast(groundCheckRay, out rayHit, _maxRayLength, _antiClipMask) ? rayHit.point.y : 0f;
        }
    }
}