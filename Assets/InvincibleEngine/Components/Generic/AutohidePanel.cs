using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace InvincibleEngine.Components.Generic {
    /// <summary>
    /// Autohide script for UI panels and other elements.
    /// Delay is in seconds.
    /// Delta and Speed are in pixels/second;
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class AutohidePanel : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
        // Local autohide direction enum
        public enum AutohideBoundary { Left, Right, Top, Bottom }
        
        // Unity Inspector
        [Header("Autohide Settings")]
        [SerializeField] private AutohideBoundary _boundary = AutohideBoundary.Left;
        [SerializeField] private float _hideDelay = 1f;
        [SerializeField] private float _hideDelta = 100f;
        [SerializeField] private float _hideSpeed = 200f;
        [SerializeField] private bool _startLocked = true;
        
        // Private: Required References
        private RectTransform _rectTransform;
        // Private: State / Positions
        private bool _locked;
        private bool _hiding = true;
        private Vector3 _originalOffset;
        private float _currentOffset;
        
        // Set lock state of the panel
        public void SetLockState(bool locked) {
            _locked = locked;
            _hiding = false;
        }
        
        // Set hide state of the panel
        public void HidePanel() {
            _hiding = true;
        }
        
        // OnMouseEnter event handler
        public void OnPointerEnter(PointerEventData data) {
            // Do nothing if the panel is locked
            if (_locked) return;
            
            // Set hiding flag to false and cancel any invokes from exit event
            _hiding = false;
            CancelInvoke();
        }
        
        // OnMouseExit event handler
        public void OnPointerExit(PointerEventData data) {
            // Do nothing if the panel is locked
            if (_locked) return;
            
            // Invoke state change after specified delay
            Invoke(nameof(HidePanel), _hideDelay);
        }
        
        // Initialization
        private void Start() {
            _rectTransform = GetComponent<RectTransform>();
            _originalOffset = _rectTransform.localPosition;
            SetLockState(_startLocked);
        }
        
        // Unity Update
        private void Update() {    
            // Calculate movement delta
            var sign = _hiding ? -1.0f : 1.0f;
            var moveDelta = _hideSpeed * sign * Time.deltaTime;
            
            // Move and clamp the panel based on the boundary and hiding flag
            var clampedPosition = _rectTransform.localPosition;
            switch (_boundary) {
                case AutohideBoundary.Left:
                    clampedPosition += Vector3.right * moveDelta;
                    clampedPosition.x = Mathf.Clamp(clampedPosition.x, _originalOffset.x - _hideDelta, _originalOffset.x);
                    break;
                case AutohideBoundary.Right:
                    clampedPosition += Vector3.left * moveDelta;
                    clampedPosition.x = Mathf.Clamp(clampedPosition.x, _originalOffset.x, _originalOffset.x + _hideDelta);
                    break;
                case AutohideBoundary.Top:
                    clampedPosition += Vector3.down * moveDelta;
                    clampedPosition.y = Mathf.Clamp(clampedPosition.y, _originalOffset.y, _originalOffset.y + _hideDelta);
                    break;
                case AutohideBoundary.Bottom:
                    clampedPosition += Vector3.up * moveDelta;
                    clampedPosition.y = Mathf.Clamp(clampedPosition.y, _originalOffset.y - _hideDelta, _originalOffset.y);
                    break;
                default:
                    return;
            }
            _rectTransform.localPosition = clampedPosition;
        }
    }
}