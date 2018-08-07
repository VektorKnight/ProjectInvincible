using UnityEngine;
using UnityEngine.EventSystems;

namespace VektorLibrary.Utility {
    [RequireComponent(typeof(RectTransform))]
    public class DraggableElement : MonoBehaviour, IDragHandler, IPointerClickHandler {
        // Private: Required References
        private RectTransform _rectTransform;
        
        // Initialization
        private void Awake() {
            // Reference required components
            _rectTransform = GetComponent<RectTransform>();
            
            // Clamp to screen space
            var clampedPosition = _rectTransform.position;
            clampedPosition.x = Mathf.Clamp(clampedPosition.x, 0f, Screen.width);
            clampedPosition.y = Mathf.Clamp(clampedPosition.y, 0f, Screen.height);
            _rectTransform.position = clampedPosition;
        }
        
        // Drag event handler
        public void OnDrag(PointerEventData eventData) {
            // Set hierarchy index
            transform.SetAsLastSibling();
            
            // Apply mouse delta to transform
            _rectTransform.position += (Vector3)eventData.delta;
            
            // Round transform values to ints for pixel clarity
            var position = _rectTransform.position;
            position.x = (int) position.x;
            position.y = (int) position.y;
            _rectTransform.position = position;
            
            // Clamp to screen space
            var clampedPosition = _rectTransform.position;
            clampedPosition.x = Mathf.Clamp(clampedPosition.x, 0f, Screen.width);
            clampedPosition.y = Mathf.Clamp(clampedPosition.y, 0f, Screen.height);
            _rectTransform.position = clampedPosition;
        }

        public void OnPointerClick(PointerEventData eventData) {
            transform.SetAsLastSibling();
        }
    }
}