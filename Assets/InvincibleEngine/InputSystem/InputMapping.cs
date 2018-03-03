using System;
using UnityEngine;

namespace InvincibleEngine.InputSystem {
    /// <summary>
    /// Input mapping class for Tank Shooter
    /// </summary>
    [Serializable]
    public class InputMapping {
        // Movement & Navigation (Keyboard/Mouse)
        [Header("Movement & Navigation (Keyboard/Mouse)")] 
        public string MovementX = "Horizontal";
        public string MovementY = "Vertical";
        public KeyCode StrafeKeyLeft = KeyCode.Q;
        public KeyCode StrafeKeyRight = KeyCode.E;

        // Movement & Navigation (Controller)
        [Header("Movement & Navigation (Controller)")] 
        public GamepadAxis MovementAxis = GamepadAxis.LeftStick;
        public GamepadAxis LookAxis = GamepadAxis.RightStick;
        public GamepadButton StrafeButtonLeft = GamepadButton.LeftBumper;
        public GamepadButton StrafeButtonRight = GamepadButton.RightBumper;
        
        // Weapons & Actions (Keyboard/Mouse)
        [Header("Weapons & Actions (Keyboard/Mouse)")] 
        public KeyCode FireKey = KeyCode.Mouse0;
        public KeyCode ZoomKey = KeyCode.Mouse1;
        public string SwapAxis = "Mouse ScrollWheel";
        public KeyCode SwapKeyLeft = KeyCode.LeftBracket;
        public KeyCode SwapKeyRight = KeyCode.RightBracket;
        public KeyCode AbilityKey = KeyCode.F;
		
        // Weapons & Actions (Controller)
        [Header("Weapons & Actions (Controller)")] 
        public GamepadButton FireButton = GamepadButton.RightTrigger;
        public GamepadButton ZoomButton = GamepadButton.RightStick;
        public GamepadButton SwapButton = GamepadButton.Y;
        public GamepadButton AbilityButton = GamepadButton.LeftTrigger;
        
        // Misc. Actions (Keyboard/Mouse)
        [Header("Misc. Actions (Keyboard/Mouse)")] 
        public KeyCode MenuKey = KeyCode.Escape;
        public KeyCode BackKey = KeyCode.Backspace;
        public KeyCode ConfirmKey = KeyCode.Return;
        public KeyCode NextElementKey = KeyCode.Tab;
        
        // Misc. Actions (Controller)
        [Header("Misc. Actions (Controller)")]
        public GamepadButton MenuButton = GamepadButton.Start;
        public GamepadButton BackButton = GamepadButton.Select;
        public GamepadButton ConfirmButton = GamepadButton.A;
    }
}