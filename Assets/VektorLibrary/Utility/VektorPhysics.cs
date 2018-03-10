using System;
using System.Collections.Generic;
using UnityEngine;

namespace VektorLibrary.Utility {
	/// <summary>
	/// Vektor Physics Library - v1.0a
	/// A modular library of pre-built generic physics components with
	/// a focus on aerodynamics. 
	/// Copyright 2017 VektorKnight | All Rights Reserved
	/// </summary>
	public static class VektorPhysics {
		//Returns average surface data from each raycast
		public static bool GetAverageSurfaceData(Transform center, Transform[] checkPoints, float maxDistance, LayerMask traceLayer, out SurfaceData data) {
			var normals = new List<Vector3>();
			var hitPoints = new List<Vector3>();

			//Raycast from center
			var centerRay = new Ray(center.position, -center.up);
			RaycastHit cHit;

			if (Physics.Raycast(centerRay, out cHit, maxDistance, traceLayer)) {
				data.CenterNormal = cHit.normal;
				data.CenterPoint = cHit.point;
				data.SlopeMagnitude = (Vector3.up - cHit.normal).magnitude;
			}
			else {
				data.CenterNormal = Vector3.up;
				data.CenterPoint = Vector3.zero;
				data.SlopeMagnitude = 0.0f;
			}

			//Raycast from each checkpoint
			foreach (var checkPoint in checkPoints) {
				var checkRay = new Ray(checkPoint.position, -center.up);
				RaycastHit hit;

				if (!Physics.Raycast(checkRay, out hit, maxDistance, traceLayer)) continue;
				normals.Add(hit.normal);
				hitPoints.Add(hit.point);
			}

			//Return data if we can, else return constants
			if (normals.Count != 0) {
				data.AverageNormal = GetAverageVector(normals.ToArray(), true);
				data.AveragePoint = GetAverageVector(hitPoints.ToArray(), false);
				return true;
			}
			else {
				data.AverageNormal = Vector3.up;
				data.AveragePoint = Vector3.zero;
				return false;
			}
		}
		
		//Returns the average of an array of Vector3s
		public static Vector3 GetAverageVector(Vector3[] vectors, bool normalize) {
			var sum = Vector3.zero;

			//Make sure we weren't given an empty array
			if (vectors.Length != 0) {
				//Sum the vectors
				foreach (var vector in vectors) {
					sum += vector;
				}

				//Return the average / normalized average
				if (normalize) return (sum / vectors.Length).normalized;
				else return sum / vectors.Length;
			}
			else return Vector3.zero;
		}
	}
	
	// PID Controller (WIP)
	[Serializable]
	public class PidController : ICloneable {

		[Tooltip("Proportional constant (counters current error)")]
		[SerializeField] private float _kProportion;

		[Tooltip("Integral constant (counters cumulated error)")] 
		[SerializeField] private float _kIntegral;

		[Tooltip("Derivative constant (fights oscillation)")]
		[SerializeField] private float _kDerivative;

		[Tooltip("Clamp the PID output to the specified range")] 
		[SerializeField] private float[] _range = {0f, 1f};

		private float _lastError;
		private float _integral;
		private float _derivative;
		private float _value;
		
		/// <summary>
		/// Create a new PidController
		/// </summary>
		/// <param name="kP">Proportional constant</param>
		/// <param name="kI">Integral constant</param>
		/// <param name="kD">Derivative constant</param>
		/// <param name="range">Range of the Pid output, i.e [0f,1f]</param>
		public PidController(float kP = 1f, float kI = 1f, float kD = 1f, float[] range = null) {
			_kProportion = kP;
			_kIntegral = kI;
			_kDerivative = kD;
			_range = range ?? new [] {0f, 1f};
		}

		/// Update the PID based on the given error which was last updated (dt) seconds ago
		/// <param name="error" />Difference between current and desired outcome.
		/// <param name="dt" />DeltaTime
		public float Update(float error, float dt) {
			_derivative = (error - _lastError) / dt;
			_integral += error * dt;
			_lastError = error;

			_value = Mathf.Clamp(_kProportion * error + _kIntegral * _integral + _kDerivative * _derivative, _range[0], _range[1]);
			return _value;
		}
		
		/// <summary>
		/// Create a clone of this Pid Controller
		/// </summary>
		/// <returns></returns>
		/// <exception cref="NotImplementedException"></exception>
		public object Clone() {
			return new PidController(_kProportion, _kIntegral, _kDerivative, _range);
		}
	}

	//Hover Engine class for Vektor Physics
	[System.Serializable]
	public class HoverEngine {
		//Desired Hover Distance
		[Tooltip("Desired distance at which to hover over the given surface")]
		public float HoverDistance;

		[Tooltip("Maximum force of the hover engine")] 
		public float MaxForce;
    
		//PID Controller
		[Tooltip("Adjust these values to modify the responsiveness of the engine")]
		public PidController HoverPid = new PidController();
    
		//Rigidbody Reference
		private Rigidbody _rigidBody;
		
		//World or Local Axis
		private bool _useWorldAxis;
		
		//Engine Values
		private float _appliedForce;
    
		/// <summary>
		/// Initialize the Engine
		/// </summary>
		/// <param name="craftBody">The engine will act on this Rigidbody</param>
		/// <param name="useWorldAxis">Whether the engine operates on a world or local axis</param>>
		public void Initialize(Rigidbody craftBody, bool useWorldAxis) {
			_rigidBody = craftBody;
			_useWorldAxis = useWorldAxis;
		}

		/// <summary>
		/// Update the engine (Must be called in FixedUpdate)
		/// </summary>
		/// <param name="validSurface">Whether or not there is a valid surface below</param>>
		/// <param name="currentDistance">Distance between the craft and the given surface</param>>
		/// <param name="deltaTime">Time since the last update (should always be FixedDeltaTime)</param>>
		public void Update (float currentDistance, float deltaTime, bool validSurface) {
			if (validSurface) {
				//_appliedForce = HoverPid.Update(HoverDistance - currentDistance, deltaTime) * MaxForce;
				_appliedForce = HoverPid.Update(HoverDistance - currentDistance, deltaTime) * _rigidBody.mass * Physics.gravity.y * _rigidBody.velocity.y;

				//World or Local
				if (_useWorldAxis) _rigidBody.AddForce(Vector3.up * _appliedForce, ForceMode.Acceleration);
				else _rigidBody.AddForce(_rigidBody.transform.up * _appliedForce, ForceMode.Acceleration);
			}
			else {
				HoverPid.Update(0f, deltaTime);
				_appliedForce = 0f;
			}
		}

		/// <summary>
		/// Update the engine accounting for extra forces (Must be called in FixedUpdate)
		/// </summary>
		/// <param name="validSurface">Whether or not there is a valid surface below</param>>
		/// <param name="currentDistance">Distance between the craft and the given surface</param>>
		/// <param name="extraForce">Extra force from a spoiler or other component</param>
		/// <param name="deltaTime">Time since the last update (should always be FixedDeltaTime)</param>>
		public void Update (float currentDistance, float extraForce, float deltaTime, bool validSurface) {
			if (validSurface) {
				_appliedForce = HoverPid.Update(HoverDistance - currentDistance, deltaTime) * (MaxForce + extraForce);

				//World or Local
				if (_useWorldAxis) _rigidBody.AddForce(Vector3.up * _appliedForce, ForceMode.Force);
				else _rigidBody.AddForce(_rigidBody.transform.up * _appliedForce, ForceMode.Force);
			}
			else {
				HoverPid.Update(0f, deltaTime);
				_appliedForce = 0f;
			}
		}
	}
	
	// Simple Hover Engine Array for Vektor Physics
	[System.Serializable]
	public class HoverArray {
		// Virtual hover engines defined by points
		public Transform[] HoverEngines;
		
		// The PiD controller template for the engines
		public PidController Controller = new PidController();

		// Maximum combined thrust that the array can produce
		public float MaxThrust;
		
		// Desired distance from a surface that each engine will attempt to maintain
		public float HoverDistance;
		
		// Max distance that each engine will check for a surface
		public float TraceDistance;
		
		// Layer used for raycasting
		public LayerMask TraceLayer;
		
		// Force used by each engine (calculated internally)
		private float _perEngineForce;
		
		// The Rigidbody that this array will act on
		private Rigidbody _rigidbody;
		
		// Internal PID controller array
		private PidController[] _pidControllers;
		
		//Initialization
		public void Initialize(Rigidbody rigidbody) {
			_rigidbody = rigidbody;
			_perEngineForce = MaxThrust / HoverEngines.Length;
			
			// Initialize the PID controller for each engine
			_pidControllers = new PidController[HoverEngines.Length];
			for (var i = 0; i < _pidControllers.Length; i++) {
				_pidControllers[i] = (PidController)Controller.Clone();
			}
		}
		
		// Update the hover array
		public void Update(float deltaTime) {
			// Update each engine in the array
			for (var i = 0; i < HoverEngines.Length; i++) {
				// Generate a ray for each engine and perform a check
				var engineRay = new Ray(HoverEngines[i].position, -HoverEngines[i].up);
				RaycastHit rayHit;
				
				// Check if a surface exists below this engine and update it, otherwise update the PID only and continue to the next
				if (!Physics.Raycast(engineRay, out rayHit, TraceDistance, TraceLayer)) {
					_pidControllers[i].Update(0f, deltaTime);
					continue;
				}
				
				// Calculate the force this engine needs to produce
				//_rigidbody.AddForceAtPosition(_rigidbody.transform.up * _perEngineForce * (1f - Vector3.Distance(HoverEngines[i].position, rayHit.point) / HoverDistance), HoverEngines[i].position);
				var currentDistance = Vector3.Distance(HoverEngines[i].position, rayHit.point);
				var pidValue = _pidControllers[i].Update(HoverDistance - currentDistance, deltaTime);
				_rigidbody.AddForceAtPosition(_rigidbody.transform.up * _perEngineForce * pidValue, HoverEngines[i].position);
			}
		}
	}
	
	//Forward Engine class for Vektor Physics
	[System.Serializable]
	public class DirectionalEngine {
		[Tooltip("Maximum force of the directional engine")]
		public float MaxForce;

		[Tooltip("Time in seconds that the engine takes to respond to the throttle")] 
		public float EngineLag;
		
		[Tooltip("Engine value is mapped to this curve to determine acceleration")]
		public AnimationCurve Acceleration;
		
		[Tooltip("Local axis on which the engine will operate")]
		public LocalDirection LocalAxis = LocalDirection.Forward;
		
		[Tooltip("Allow the engine to accept negative input values")]
		public bool AllowNegativeInput = false;
		
		//Rigidbody Reference
		private Rigidbody _rigidBody;
		
		//Engine Value
		private float _engineValue;
		
		//Velocity reference for SmoothDamp
		private float _refV;
		
		/// <summary>
		/// Initialize the Engine
		/// </summary>
		/// <param name="craftBody">The engine will act on this Rigidbody</param>
		public void Initialize(Rigidbody craftBody) {
			_rigidBody = craftBody;
		}
		
		/// <summary>
		/// Update the engine ignoring any surface data (Must be called from FixedUpdate)
		/// </summary>
		/// <param name="controlValue">Throttle for the engine (Should be between -1 and 1)</param>
		public void Update(float controlValue) {
			if (!AllowNegativeInput && controlValue < 0f) return;
			_rigidBody.AddForce(GetForceVector() * CalculateForce(controlValue), ForceMode.Force);
		}

		/// <summary>
		/// Update the engine ignoring any surface data (Must be called from FixedUpdate)
		/// </summary>
		/// <param name="controlValue">Throttle for the engine (Should be between -1 and 1)</param>
		/// <param name="direction">The direction of the force.</param>
		public void Update(float controlValue, Vector3 direction) {
			if (!AllowNegativeInput && controlValue < 0f) return;
			_rigidBody.AddForce(direction.normalized * CalculateForce(controlValue), ForceMode.Force);
		}
		
		//Calculate Engine Force
		private float CalculateForce(float controlValue) {
			_engineValue = Mathf.SmoothDamp(_engineValue, Acceleration.Evaluate(Mathf.Abs(controlValue)), ref _refV, EngineLag);
			return Mathf.Sign(controlValue) * _engineValue * MaxForce;
		}
		
		//Get Desired Force Vector
		private Vector3 GetForceVector() {
			switch (LocalAxis) {
				case LocalDirection.Forward:			
					return _rigidBody.transform.forward;
				case LocalDirection.Back:
					return -_rigidBody.transform.forward;
				case LocalDirection.Left:
					return -_rigidBody.transform.right;
				case LocalDirection.Right:
					return _rigidBody.transform.right;
				case LocalDirection.Up:
					return _rigidBody.transform.up;
				case LocalDirection.Down:
					return -_rigidBody.transform.up;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}	
	}
	
	//Gyroscope class for Vektor Physics
	[System.Serializable]
	public class VektorGyroscope {
		[Tooltip("Torque applied by the gyroscope to align the rigidbody")]
		public float AlignmentTorque;

		[Tooltip("Torque applied by the gyroscope to steer the rigidbody")]
		public float SteeringTorque;
		
		//Rigidbody Reference
		private Rigidbody _rigidBody;

		//Reference Vector for Alignment
		private Vector3 _levelVector;
		
		//Enable/Disable Airbourne Alignment
		private bool _autoLevel;

		/// <summary>
		/// Intialize the Gyroscope
		/// </summary>
		/// <param name="craftBody">The gyroscope will act on this Rigidbody</param>
		/// /// <param name="levelVector">Normalized vector with which the gyroscope will attempt to align</param>
		/// <param name="autoLevel">Whether or not the gyroscope will attempt to align with a given vector automatically</param>
		public void Initialize(Rigidbody craftBody, Vector3 levelVector, bool autoLevel) {
			_rigidBody = craftBody;
			_levelVector = levelVector;
			_autoLevel = autoLevel;
		}
		
		/// <summary>
		/// Update the gyroscope taking input into account and initial alignment vector
		/// </summary>
		/// <param name="input">Steering input for the gyroscope (Pitch, Yaw, Roll)</param>
		public void Update(Vector3 input) {
			_rigidBody.AddRelativeTorque(input * SteeringTorque, ForceMode.Force);
			
			//Return if auto-levelling is disabled
			if (!_autoLevel) return;
			
			_rigidBody.AddTorque(Vector3.Cross(_rigidBody.transform.up, _levelVector) * AlignmentTorque, ForceMode.Force);
		}

		/// <summary>
		/// Update the gyroscope taking input into account and an updated alignment vector
		/// </summary>
		/// <param name="input">Steering input for the gyroscope (Pitch, Yaw, Roll)</param>
		/// <param name="alignmentVector">New vector to be used for alignment</param>
		public void Update(Vector3 input, Vector3 alignmentVector) {
			_rigidBody.AddRelativeTorque(input * SteeringTorque, ForceMode.Force);
			
			//Return if auto-alignment is disabled
			if (!_autoLevel) return;
			
			_rigidBody.AddTorque(Vector3.Cross(_rigidBody.transform.up, alignmentVector) * AlignmentTorque, ForceMode.Force);
		}
		
		/// <summary>
		/// Manually align the gyroscope with a given vector
		/// Use if auto-alignment is disabled
		/// </summary>
		/// <param name="alignmentVector"></param>
		public void Align(Vector3 alignmentVector) {
			_rigidBody.AddTorque(Vector3.Cross(_rigidBody.transform.up, alignmentVector) * AlignmentTorque, ForceMode.Force);
		}

	}
	
	//Aerodynamic Surface class for VektorPhysics
	[System.Serializable]
	public class LiftSurface {
		[Tooltip("Approximate area of the lift surface used for calculation (sq.meters)")]
		public float SurfaceArea;

		[Tooltip("Approximate air density value (default is 1.228)")] 
		public float AirDensity = 1.228f;

		[Tooltip("Coefficient of lift (Defualt is 0.5)")]
		public float LiftCoef = 0.5f;
		
		//Lift Force Property
		public float LiftForce { get; private set; }

		//Rigidbody Reference
		private Rigidbody _rigidBody;
		
		//Lift Force

		//Lift Coefficient
		private float _liftCoef;
		
		/// <summary>
		/// Initialize the lift surface
		/// </summary>
		/// <param name="rigidBody">Lift surface will reference this rigidbody</param>
		public void Initialize(Rigidbody rigidBody) {
			_rigidBody = rigidBody;
		}
		
		/// <summary>
		/// Update the lift surface
		/// </summary>
		public void Update() {
			LiftForce = 0.5f * AirDensity * Mathf.Pow(_rigidBody.velocity.magnitude, 2f) * SurfaceArea * LiftCoef;
			_rigidBody.AddForce(LiftForce * _rigidBody.transform.up, ForceMode.Force);
		}
	}
	
	//Axial Compensator class for VektorPhysics
	[System.Serializable]
	public class DragSurface {
		[Tooltip("Effective drag applied to the rigidbody along the specified local axis")]
		public float AxialDrag;
		
		[Tooltip("Local axis upon which the compensator will act")]
		public LocalAxis Axis;
		
		//Rigidbody Reference
		private Rigidbody _rigidBody;
		
		//Local Velocity
		private Vector3 _localVelocity;
		
		//Local Axis Velocity
		private float _axialVelocity;
		
		/// <summary>
		/// Initialize the Axial Compensator
		/// </summary>
		/// <param name="rigidBody"></param>
		public void Initialize(Rigidbody rigidBody) {
			_rigidBody = rigidBody;
		}
		
		/// <summary>
		/// Update the drag surface using the initial drag value
		/// </summary>
		public void Update() {
			_localVelocity = _rigidBody.transform.InverseTransformDirection(_rigidBody.velocity);
			_rigidBody.AddRelativeForce(GetForceAxis() * _rigidBody.mass * -GetAxialVelocity() * AxialDrag);
		}
		
		/// <summary>
		/// Update the drag surface with a different drag value
		/// </summary>
		/// <param name="axialDrag"></param>
		public void Update(float axialDrag) {
			_localVelocity = _rigidBody.transform.InverseTransformDirection(_rigidBody.velocity);
			_rigidBody.AddRelativeForce(GetForceAxis() * 0.5f * _rigidBody.mass * -GetAxialVelocity() * axialDrag);
		}
		
		//Get Axial velocity
		private float GetAxialVelocity() {
			switch (Axis) {
				case LocalAxis.X:
					return _localVelocity.x;
				case LocalAxis.Y:
					return _localVelocity.y;
				case LocalAxis.Z:
					return _localVelocity.z;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}
		
		//Get Force Axis
		private Vector3 GetForceAxis() {
			switch (Axis) {
				case LocalAxis.X:
					return Vector3.right;
				case LocalAxis.Y:
					return Vector3.up;
				case LocalAxis.Z:
					return Vector3.forward;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}


	}
	
	//Speed Limiter class for VektorPhysics
	[System.Serializable]
	public class SpeedLimiter {
		[Tooltip("Rigidbody will be limited to this velocity (m/s)")]
		public float MaxVelocity;
		
		//PID Controller
		public PidController LimiterPid = new PidController();
		
		//Rigidbody Reference
		private Rigidbody _rigidBody;
		
		//Counterforce
		private float _counterForce;
		
		/// <summary>
		/// Initialize the speed limiter
		/// </summary>
		/// <param name="rigidBody">Limiter will apply to this rigidbody</param>
		public void Initialize(Rigidbody rigidBody) {
			_rigidBody = rigidBody;
		}
		
		/// <summary>
		/// Update the speed limiter
		/// </summary>
		/// <param name="deltaTime">Time in seconds since last update (Should always be FixedDeltaTime)</param>
		public void Update(float deltaTime) {
			if (_rigidBody.velocity.magnitude > MaxVelocity) {
				_counterForce = (_rigidBody.mass / Mathf.Pow(_rigidBody.drag, 2f)) * LimiterPid.Update(_rigidBody.velocity.magnitude / MaxVelocity, deltaTime);
				_rigidBody.AddForce(-_rigidBody.velocity.normalized * _counterForce);
			}
			else {
				LimiterPid.Update(0f, deltaTime);
			}	
		}

		/// <summary>
		/// Update the speed limiter with a different max velocity
		/// </summary>
		/// <param name="maxVelocity">Rigidbody will be limited to this velocity</param>
		/// <param name="deltaTime">Time in seconds since last update (Should always be FixedDeltaTime)</param>
		public void Update(float maxVelocity, float deltaTime) {
			if (_rigidBody.velocity.magnitude > MaxVelocity) {
				_counterForce = (_rigidBody.mass / Mathf.Pow(_rigidBody.drag, 2f)) * LimiterPid.Update(_rigidBody.velocity.magnitude / maxVelocity, deltaTime);
				_rigidBody.AddForce(-_rigidBody.velocity.normalized * _counterForce);
			}
			else {
				LimiterPid.Update(0f, deltaTime);
			}	
		}
	}
	
	//Local Direction Enum
	public enum LocalDirection {
		Forward,
		Back,
		Left,
		Right,
		Up,
		Down
	}
	
	//Local Axis Enum
	public enum LocalAxis {
		X,
		Y,
		Z
	}
	
	//Surface Data Struct
	[System.Serializable]
	public struct SurfaceData {
		public Vector3 CenterNormal;
		public Vector3 CenterPoint;
		public Vector3 AverageNormal;
		public Vector3 AveragePoint;
		public float SlopeMagnitude;
	}	
}