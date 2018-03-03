using InvincibleEngine.Components.Utility;
using InvincibleEngine.VektorLibrary.Utility;
using UnityEngine;

namespace InvincibleEngine.Dynamics.DayNight.Components {
    /// <summary>
    /// Primary controller class for the dynamic day/night system.
    /// Part of the Vektor Common Framework.
    /// </summary>
    public class DayNightSystem : MonoBehaviour {
        
        // Constants: Time
        public const int SECONDS_PER_DAY = 86400;
        public const int TIME_MORNING = 0;
        public const int TIME_NOON = 21600;
        public const int TIME_EVENING = 43200;
        public const int TIME_NIGHT = 64800;

        // Unity Inspector
        [Header("Skybox Settings")] 
        [SerializeField] private Material _skyboxMaterial;
        [SerializeField] private Gradient _skyboxGradient;
        [SerializeField] private AnimationCurve _skyboxIntensity;

        [Header("Main Light Settings")] 
        [SerializeField] private Light _mainLight;
        [SerializeField] private Gradient _mainLightGradient;
        [SerializeField] private AnimationCurve _mainLightIntensity;
        [SerializeField] private AnimationCurve _mainLightRotation;

        [Header("Time Settings")] 
        [SerializeField] private int _startTime = 5400;
        [SerializeField] private float _timeScale = 60f;
        
        // Private: State
        private bool _initialized;
        
        // Private: Time
        private float _currentTime;
        private bool _pauseTime = true;
        
        // Private: Required References
        private Material _skyboxInstance;
        
        // Initialization
        private void Start() {
            // Exit if already initialized
            if (_initialized) return;
            
            // Check all required references
            if (_skyboxMaterial == null || _mainLight == null) {
                Debug.LogWarning($"Day/Night: One or more required object references are missing!");
                return;
            }
            
            // Ensure the skybox material is set and update GI
            RenderSettings.skybox = _skyboxMaterial;
            DynamicGI.UpdateEnvironment();
            
            // Ensure both lights are children of this transform
            transform.ResetTransform();
            _mainLight.transform.parent = transform;
            _mainLight.transform.ResetTransform();
            
            // Add a field for Time to the debug readout
            DebugReadout.AddField("Time");
            
            // We're done here
            _currentTime = _startTime;
            _pauseTime = false;
            _initialized = true;
        }
        
        // Unity Update
        private void Update() {
            // Exit if not initialized or time is paused
            if (!_initialized || _pauseTime) return;
            
            // Update the current time and wrap it if necessary
            _currentTime += Time.deltaTime * _timeScale;
            _currentTime = VektorUtility.WrapFloat(_currentTime, TIME_MORNING, SECONDS_PER_DAY);
            DebugReadout.UpdateField("Time", $"{_currentTime:n0}");
            
            // Update the skybox & ambient light
            var timeKey = _currentTime / SECONDS_PER_DAY;
            RenderSettings.ambientSkyColor = _skyboxGradient.Evaluate(timeKey);
            _skyboxMaterial.SetColor("_Color2", _skyboxGradient.Evaluate(timeKey));
            _skyboxMaterial.SetColor("_Color1", _skyboxGradient.Evaluate(timeKey));
            _skyboxMaterial.SetFloat("_Intensity", _skyboxIntensity.Evaluate(timeKey));
            
            // Update the main light
            _mainLight.color = _mainLightGradient.Evaluate(timeKey);
            _mainLight.intensity = _mainLightIntensity.Evaluate(timeKey);
            _mainLight.transform.localRotation = Quaternion.Euler(_mainLightRotation.Evaluate(timeKey), 180f, 0f);
        }
    }
}