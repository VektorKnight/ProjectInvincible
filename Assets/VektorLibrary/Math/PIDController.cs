using System;
using UnityEngine;

namespace VektorLibrary.Math {
    /// <summary>
    /// Basic implementation of a PID controller.
    /// </summary>
    [Serializable] public class PIDController : ICloneable {
        
        // Unity Inspector: Config
        [SerializeField] private float _gainProportion;    // Counters current error
        [SerializeField] private float _gainIntegral;      // Counters cumulative error
        [SerializeField] private float _gainDerivative;    // Reduces oscillation
        
        // Private: PID Values
        private float _lastError;
        private float _integral;
        private float _derivative;
        private float _value;
        
        // Properties: Config
        public float GainProportion {
            get { return _gainProportion; } 
            set { _gainProportion = value; }
        }
        
        public float GainIntegral {
            get { return _gainIntegral; }
            set { _gainIntegral = value; }
        }

        public float GainDerivative {
            get { return _gainDerivative; }
            set { _gainDerivative = value; }
        }

        /// <summary>
        /// Create a new PID Controller
        /// </summary>
        /// <param name="kP">Proportional constant.</param>
        /// <param name="kI">Integral constant.</param>
        /// <param name="kD">Derivative constant.</param>
        public PIDController(float kP = 1f, float kI = 1f, float kD = 1f) {
            _gainProportion = kP;
            _gainIntegral = kI;
            _gainDerivative = kD;
        }

        /// <summary>
        /// Update the PID based on the given error which was last updated (dt) seconds ago
        /// </summary>
        /// <param name="error" />Difference between current and desired outcome.
        /// <param name="deltaTime"></param>
        /// DeltaTime
        public float Update(float error, float deltaTime) {
            _derivative = (error - _lastError) / deltaTime;
            _integral += error * deltaTime;
            _lastError = error;

            return Mathf.Clamp01(_gainProportion * error + _gainIntegral * _integral + _gainDerivative * _derivative);
        }
		
        /// <summary>
        /// Create a clone of this PID Controller
        /// </summary>
        /// <returns>A copy of this PID controller with the same configuration values.</returns>
        public object Clone() {
            return new PIDController(_gainProportion, _gainIntegral, _gainDerivative);
        }
    }
}