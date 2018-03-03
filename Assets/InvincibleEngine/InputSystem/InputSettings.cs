using System;
using UnityEngine;

namespace InvincibleEngine.InputSystem {
	[Serializable]
	public struct InputSettings {
	
		// Unity Inspector
		[SerializeField] private bool _keyboardInput;
		[SerializeField] private float _sensitivity;
		[SerializeField] private float _deadZone;
		[SerializeField] private InputMapping _inputMapping;
	
		// Public Readonly: Input Settings
		public bool KeyboardInput => _keyboardInput;
		
		// Public Read-Only: Input Mapping
		public InputMapping Map => _inputMapping;
		
		// Public Read/Write: Input Settings
		public float Sensitivity {
			get { return _sensitivity; }
			set { _sensitivity = Mathf.Clamp(value, 0.1f, 10f); }
		}

		public float DeadZone {
			get { return _deadZone; }
			set { _deadZone = Mathf.Clamp(value, 0f, 1f); }
		}
	}
}
