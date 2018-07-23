using UnityEngine;
using UnityEngine.UI;

namespace InvincibleEngine.UnitFramework.DataTypes {
    [RequireComponent(typeof(Image))]
    [RequireComponent(typeof(Shadow))]
    public class UnitIcon : MonoBehaviour {
        // Unity Inspector
        [Header("Sprite Setup")] 
        [SerializeField] private Sprite _selected;
        [SerializeField] private Sprite _unselected;
        
        // Private: Required References
        private Image _image;
        private Shadow _shadow;
        
        // Initialization
        public void Initialize() {
            // Reference required components
            _image = GetComponent<Image>();
            _shadow = GetComponent<Shadow>();
            
            // Set default sprite
            _image.sprite = _unselected;
            _image.SetNativeSize();
            
            // Set default outline state
            _shadow.effectColor = Color.white;
            _shadow.enabled = false;
        }
        
        // Set selection state of the icon
        public void SetSelected(bool selected) {
            _image.sprite = selected ? _selected : _unselected;
            _image.SetNativeSize();
            _shadow.enabled = selected;
        }
        
        // Set the color of the icon
        public void SetColor(Color color) {
            _image.color = color;
        }
        
        // Set the parent canvas
        public void SetParent(Transform parent) {
            _image.rectTransform.SetParent(parent, false);
        }
        
        // Set the screen position
        public void SetScreenPosition(Vector3 position) {
            _image.rectTransform.position = position;
        }
        
        // Set render state (show/hide)
        public void SetRender(bool render) {
            _image.enabled = render;
        }
    }
}