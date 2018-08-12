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
            var offset = new Vector2(_rectTransform.sizeDelta.x * _rectTransform.pivot.x, _rectTransform.sizeDelta.y * _rectTransform.pivot.y);
            clampedPosition.x = Mathf.Clamp(clampedPosition.x, offset.x, Screen.width - offset.x);
            clampedPosition.y = Mathf.Clamp(clampedPosition.y, offset.y, Screen.height - offset.y);
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
            var minimum = new Vector2(_rectTransform.sizeDelta.x * _rectTransform.pivot.x, _rectTransform.sizeDelta.y * _rectTransform.pivot.y);
            var maximum = new Vector2(_rectTransform.sizeDelta.x - minimum.x, _rectTransform.sizeDelta.y - minimum.y);
            clampedPosition.x = Mathf.Clamp(clampedPosition.x, minimum.x, Screen.width - maximum.x);
            clampedPosition.y = Mathf.Clamp(clampedPosition.y, minimum.y, Screen.height - maximum.y);
            _rectTransform.position = clampedPosition;
        }

        public void OnPointerClick(PointerEventData eventData) {
            transform.SetAsLastSibling();
        }
    }
}