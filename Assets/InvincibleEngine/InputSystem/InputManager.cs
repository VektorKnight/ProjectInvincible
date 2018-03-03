using System;
using System.Collections.Generic;
using UnityEngine;
using XInputDotNetPure;

namespace InvincibleEngine.InputSystem {
	/// <summary>
	/// Manages multiple input sources making them easier to access from entity behaviors.
	/// </summary>
	public class InputManager : MonoBehaviour {
		
		// Singleton Instance
		private static InputManager _singleton;
		public static InputManager Instance => _singleton ?? new GameObject("InputManager").AddComponent<InputManager>();
		
		// Input Listeners
		private readonly List<InputListener> _inputListeners = new List<InputListener>();

		// Unity Inspector
		[Header("Enhancements")] 
		[SerializeField] private bool _filterLeftStick = true;
		[SerializeField] private bool _filterRightStick = true;
		[SerializeField] private AnimationCurve _joystickCurve = new AnimationCurve();
		
		[Header("Input Mappings")]
		[SerializeField] private InputMapping _playerInputMapping = new InputMapping();
		
		[Header("Input Settings")] 
		[SerializeField] private InputSettings[] _playerInputSettings = new InputSettings[4];
		
		// Private: Gamepad States
		private readonly GamePadState[] _currentStates = new GamePadState[4];
		private readonly GamePadState[] _previousStates = new GamePadState[4];

		// Initialization
		private void Awake() {
			// Enforce Singleton Instance
			if (_singleton == null) { _singleton = this; }
			else if (_singleton != this) { Destroy(gameObject); }
			
			// Ensure this manager is not destroyed on scene load
			DontDestroyOnLoad(gameObject);
			
			// Initialize the Gamepad states
			for (var i = 0; i < 4; i++) {
				_currentStates[i] = GamePad.GetState((PlayerIndex) i);
				_previousStates[i] = _currentStates[i];
			}
		}
		
		// Per-Frame Update
		private void Update() {
			// Update the gamepad states
			for (var i = 0; i < 4; i++) {
				_previousStates[i] = _currentStates[i];
				_currentStates[i] = GamePad.GetState((PlayerIndex) i);
			}
			
			// Update all Input Listeners
			if (_inputListeners.Count == 0) return;
			foreach (var listener in _inputListeners) {
				listener?.InputUpdate();
			}
		}
		
		/// <summary>
		/// Register an Input Listener with the Input Update callback.
		/// </summary>
		/// <param name="listener">The listener to register.</param>
		public static void RegisterListener(InputListener listener) {
			if (!Instance._inputListeners.Contains(listener)) {
				Instance._inputListeners.Add(listener);
			}
		}
		
		/// <summary>
		/// Grab the input settings for the specified local player.
		/// </summary>
		/// <param name="index">The index of the local player.</param>
		/// <returns></returns>
		public static InputSettings GetInputSettings(PlayerIndex index) {
			return Instance._playerInputSettings[(int) index];
		}
	
		/// <summary>
		/// Check if the specified button was pressed this frame.
		/// </summary>
		/// <param name="index">The player controller index.</param>
		/// <param name="button">The button to check.</param>
		public static bool GetButtonDown(PlayerIndex index, GamepadButton button) {
			// Poll the state of the given controller index
			var i = (int) index;
			
			// Check the specified button
			switch (button) {
				case GamepadButton.A:
					if (Instance._previousStates[i].Buttons.A == ButtonState.Released && Instance._currentStates[i].Buttons.A == ButtonState.Pressed) return true;
					break;
				case GamepadButton.B:
					if (Instance._previousStates[i].Buttons.B == ButtonState.Released && Instance._currentStates[i].Buttons.B == ButtonState.Pressed) return true;
					break;
				case GamepadButton.X:
					if (Instance._previousStates[i].Buttons.X == ButtonState.Released && Instance._currentStates[i].Buttons.X == ButtonState.Pressed) return true;
					break;
				case GamepadButton.Y:
					if (Instance._previousStates[i].Buttons.Y == ButtonState.Released && Instance._currentStates[i].Buttons.Y == ButtonState.Pressed) return true;
					break;
				case GamepadButton.LeftBumper:
					if (Instance._previousStates[i].Buttons.LeftShoulder == ButtonState.Released && Instance._currentStates[i].Buttons.LeftShoulder == ButtonState.Pressed) return true;
					break;
				case GamepadButton.RightBumper:
					if (Instance._previousStates[i].Buttons.RightShoulder == ButtonState.Released && Instance._currentStates[i].Buttons.RightShoulder == ButtonState.Pressed) return true;
					break;
				case GamepadButton.Select:
					if (Instance._previousStates[i].Buttons.Back == ButtonState.Released && Instance._currentStates[i].Buttons.Back == ButtonState.Pressed) return true;
					break;
				case GamepadButton.Start:
					if (Instance._previousStates[i].Buttons.Start == ButtonState.Released && Instance._currentStates[i].Buttons.Start == ButtonState.Pressed) return true;
					break;
				case GamepadButton.LeftStick:
					if (Instance._previousStates[i].Buttons.LeftStick == ButtonState.Released && Instance._currentStates[i].Buttons.LeftStick == ButtonState.Pressed) return true;
					break;
				case GamepadButton.RightStick:
					if (Instance._previousStates[i].Buttons.RightStick == ButtonState.Released && Instance._currentStates[i].Buttons.RightStick == ButtonState.Pressed) return true;
					break;
				case GamepadButton.DPadLeft:
					if (Instance._previousStates[i].DPad.Left == ButtonState.Released && Instance._currentStates[i].DPad.Left == ButtonState.Pressed) return true;
					break;
				case GamepadButton.DPadRight:
					if (Instance._previousStates[i].DPad.Right == ButtonState.Released && Instance._currentStates[i].DPad.Right == ButtonState.Pressed) return true;
					break;
				case GamepadButton.DPadUp:
					if (Instance._previousStates[i].DPad.Up == ButtonState.Released && Instance._currentStates[i].DPad.Up == ButtonState.Pressed) return true;
					break;
				case GamepadButton.DPadDown:
					if (Instance._previousStates[i].DPad.Down == ButtonState.Released && Instance._currentStates[i].DPad.Down == ButtonState.Pressed) return true;
					break;
				case GamepadButton.LeftTrigger:
					// Round the trigger values to whole numbers to mitigate errors
					var leftTriggerPrev = Mathf.Round(Instance._previousStates[i].Triggers.Left);
					var leftTriggerCurr = Mathf.Round(Instance._currentStates[i].Triggers.Left);
					
					// Check previous vs current state
					if (leftTriggerPrev < 1f && leftTriggerCurr > 0f) return true;
					
					break;
				case GamepadButton.RightTrigger:
					// Round the trigger values to whole numbers to mitigate errors
					var rightTriggerPrev = Mathf.Round(Instance._previousStates[i].Triggers.Right);
					var rightTriggerCurr = Mathf.Round(Instance._currentStates[i].Triggers.Right);
					
					// Check previous vs current state
					if (rightTriggerPrev < 1f && rightTriggerCurr > 0f) return true;
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(button), button, null);
			}
			
			// Return false if no conditions met
			return false;
		}
		
		/// <summary>
		/// Check if the specified button was released this frame.
		/// </summary>
		/// <param name="index">The player controller index.</param>
		/// <param name="button">The button to check.</param>
		public static bool GetButtonUp(PlayerIndex index, GamepadButton button) {
			// Poll the state of the given controller index
			var i = (int) index;
			
			// Check the specified button
			switch (button) {
				case GamepadButton.A:
					if (Instance._previousStates[i].Buttons.A == ButtonState.Pressed && Instance._currentStates[i].Buttons.A == ButtonState.Released) return true;
					break;
				case GamepadButton.B:
					if (Instance._previousStates[i].Buttons.B == ButtonState.Pressed && Instance._currentStates[i].Buttons.B == ButtonState.Released) return true;
					break;
				case GamepadButton.X:
					if (Instance._previousStates[i].Buttons.X == ButtonState.Pressed && Instance._currentStates[i].Buttons.X == ButtonState.Released) return true;
					break;
				case GamepadButton.Y:
					if (Instance._previousStates[i].Buttons.Y == ButtonState.Pressed && Instance._currentStates[i].Buttons.Y == ButtonState.Released) return true;
					break;
				case GamepadButton.LeftBumper:
					if (Instance._previousStates[i].Buttons.LeftShoulder == ButtonState.Pressed && Instance._currentStates[i].Buttons.LeftShoulder == ButtonState.Released) return true;
					break;
				case GamepadButton.RightBumper:
					if (Instance._previousStates[i].Buttons.RightShoulder == ButtonState.Pressed && Instance._currentStates[i].Buttons.RightShoulder == ButtonState.Released) return true;
					break;
				case GamepadButton.Select:
					if (Instance._previousStates[i].Buttons.Back == ButtonState.Pressed && Instance._currentStates[i].Buttons.Back == ButtonState.Released) return true;
					break;
				case GamepadButton.Start:
					if (Instance._previousStates[i].Buttons.Start == ButtonState.Pressed && Instance._currentStates[i].Buttons.Start == ButtonState.Released) return true;
					break;
				case GamepadButton.LeftStick:
					if (Instance._previousStates[i].Buttons.LeftStick == ButtonState.Pressed && Instance._currentStates[i].Buttons.LeftStick == ButtonState.Released) return true;
					break;
				case GamepadButton.RightStick:
					if (Instance._previousStates[i].Buttons.RightStick == ButtonState.Pressed && Instance._currentStates[i].Buttons.RightStick == ButtonState.Released) return true;
					break;
				case GamepadButton.DPadLeft:
					if (Instance._previousStates[i].DPad.Left == ButtonState.Pressed && Instance._currentStates[i].DPad.Left == ButtonState.Released) return true;
					break;
				case GamepadButton.DPadRight:
					if (Instance._previousStates[i].DPad.Right == ButtonState.Pressed && Instance._currentStates[i].DPad.Right == ButtonState.Released) return true;
					break;
				case GamepadButton.DPadUp:
					if (Instance._previousStates[i].DPad.Up == ButtonState.Pressed && Instance._currentStates[i].DPad.Up == ButtonState.Released) return true;
					break;
				case GamepadButton.DPadDown:
					if (Instance._previousStates[i].DPad.Down == ButtonState.Pressed && Instance._currentStates[i].DPad.Down == ButtonState.Released) return true;
					break;
				case GamepadButton.LeftTrigger:
					// Round the trigger values to whole numbers to mitigate errors
					var leftTriggerPrev = Mathf.Round(Instance._previousStates[i].Triggers.Left);
					var leftTriggerCurr = Mathf.Round(Instance._currentStates[i].Triggers.Left);
					
					// Check previous vs current state
					if (leftTriggerPrev > 0f && leftTriggerCurr < 1f) return true;
					
					break;
				case GamepadButton.RightTrigger:
					// Round the trigger values to whole numbers to mitigate errors
					var rightTriggerPrev = Mathf.Round(Instance._previousStates[i].Triggers.Right);
					var rightTriggerCurr = Mathf.Round(Instance._currentStates[i].Triggers.Right);
					
					// Check previous vs current state
					if (rightTriggerPrev > 0f && rightTriggerCurr < 1f) return true;
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(button), button, null);
			}
			
			// Return false if no conditions met
			return false;
		}
		
		/// <summary>
		/// Check if the specified button is being pressed this frame.
		/// </summary>
		/// <param name="index">The player controller index.</param>
		/// <param name="button">The button to check.</param>
		public static bool GetButton(PlayerIndex index, GamepadButton button) {
			// Poll the state of the given controller index
			var i = (int) index;
			
			// Check the specified button
			switch (button) {
				case GamepadButton.A:
					if (Instance._currentStates[i].Buttons.A == ButtonState.Pressed) return true;
					break;
				case GamepadButton.B:
					if (Instance._currentStates[i].Buttons.B == ButtonState.Pressed) return true;
					break;
				case GamepadButton.X:
					if (Instance._currentStates[i].Buttons.X == ButtonState.Pressed) return true;
					break;
				case GamepadButton.Y:
					if (Instance._currentStates[i].Buttons.Y == ButtonState.Pressed) return true;
					break;
				case GamepadButton.LeftBumper:
					if (Instance._currentStates[i].Buttons.LeftShoulder == ButtonState.Pressed) return true;
					break;
				case GamepadButton.RightBumper:
					if (Instance._currentStates[i].Buttons.RightShoulder == ButtonState.Pressed) return true;
					break;
				case GamepadButton.Select:
					if (Instance._currentStates[i].Buttons.Back == ButtonState.Pressed) return true;
					break;
				case GamepadButton.Start:
					if (Instance._currentStates[i].Buttons.Start == ButtonState.Pressed) return true;
					break;
				case GamepadButton.LeftStick:
					if (Instance._currentStates[i].Buttons.LeftStick == ButtonState.Pressed) return true;
					break;
				case GamepadButton.RightStick:
					if (Instance._currentStates[i].Buttons.RightStick == ButtonState.Pressed) return true;
					break;
				case GamepadButton.DPadLeft:
					if (Instance._currentStates[i].DPad.Left == ButtonState.Pressed) return true;
					break;
				case GamepadButton.DPadRight:
					if (Instance._currentStates[i].DPad.Right == ButtonState.Pressed) return true;
					break;
				case GamepadButton.DPadUp:
					if (Instance._currentStates[i].DPad.Right == ButtonState.Pressed) return true;
					break;
				case GamepadButton.DPadDown:
					if (Instance._currentStates[i].DPad.Right == ButtonState.Pressed) return true;
					break;
				case GamepadButton.LeftTrigger:
					if (Mathf.Round(Instance._currentStates[i].Triggers.Left) > 0f) return true;
					break;
				case GamepadButton.RightTrigger:
					if (Mathf.Round(Instance._currentStates[i].Triggers.Right) > 0f) return true;
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(button), button, null);
			}
			
			// Return false if no conditions met
			return false;
		}
		
		/// <summary>
		/// Check the state of the specified axis.
		/// </summary>
		/// <param name="index"></param>
		/// <param name="axis"></param>
		/// <returns></returns>
		public static Vector2 GetAxis(PlayerIndex index, GamepadAxis axis) {
			// Poll the state of the given controller index
			var i = (int) index;
			var thumbStick = Vector2.zero;

			switch (axis) {
				case GamepadAxis.LeftStick:
					// Grab the Input Values for the Left Stick
					thumbStick.x = Instance._currentStates[i].ThumbSticks.Left.X;
					thumbStick.y = Instance._currentStates[i].ThumbSticks.Left.Y;
					
					// Return the raw input values if curve-mapping is disabled
					if (!Instance._filterLeftStick) return thumbStick;
					
					// Map the input values to a curve if enabled
					thumbStick.x = Mathf.Sign(thumbStick.x) * Instance._joystickCurve.Evaluate(Mathf.Abs(thumbStick.x));
					thumbStick.y = Mathf.Sign(thumbStick.y) * Instance._joystickCurve.Evaluate(Mathf.Abs(thumbStick.y));

					// Return the curve-mapped values
					return thumbStick;
				case GamepadAxis.RightStick:
					// Grab the Input Values for the Right Stick
					thumbStick.x = Instance._currentStates[i].ThumbSticks.Right.X;
					thumbStick.y = Instance._currentStates[i].ThumbSticks.Right.Y;
					
					// Return the raw input values if curve-mapping is disabled
					if (!Instance._filterRightStick) return thumbStick;
					
					// Map the input values to a curve if enabled
					thumbStick.x = Mathf.Sign(thumbStick.x) * Instance._joystickCurve.Evaluate(Mathf.Abs(thumbStick.x));
					thumbStick.y = Mathf.Sign(thumbStick.y) * Instance._joystickCurve.Evaluate(Mathf.Abs(thumbStick.y));

					// Return the curve-mapped values
					return thumbStick;
				case GamepadAxis.Triggers:
					return new Vector2(Instance._currentStates[i].Triggers.Left, Instance._currentStates[i].Triggers.Right);
				default:
					Debug.LogError("InputManager: Unsupported call to GetAxis! The axis {axis} does not exist!");
					return Vector2.zero;
			}
		}
	}
	
	[Serializable]
	public enum InputMode {
		Keyboard,
		Controller
	}
	
	// Gamepad Buttons
	[Serializable]
	public enum GamepadButton {
		A,
		B,
		X,
		Y,
		LeftBumper,
		RightBumper,
		LeftTrigger,
		RightTrigger,
		Select,
		Start,
		LeftStick,
		RightStick,
		DPadLeft,
		DPadRight,
		DPadUp,
		DPadDown
	}
	
	// Gamepad Axes
	[Serializable]
	public enum GamepadAxis {
		LeftStick,
		RightStick,
		Triggers
	}
}
