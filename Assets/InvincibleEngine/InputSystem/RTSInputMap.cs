using UnityEngine;

namespace InvincibleEngine.InputSystem {
    public class RTSInputMap {
        [Header("Camera Navigation")] 
        public string MovementX = "Horizontal";
        public string MovementY = "Vertical";
        public string ZoomAxis = "Mouse ScrollWheel";
        public KeyCode[] Rotation = { KeyCode.PageDown, KeyCode.PageUp, KeyCode.Home }; // Left, Right, Reset
    }
}