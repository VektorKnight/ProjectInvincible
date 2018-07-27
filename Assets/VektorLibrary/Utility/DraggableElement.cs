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
        }

        public void OnPointerClick(PointerEventData eventData) {
            transform.SetAsLastSibling();
        }
    }
}