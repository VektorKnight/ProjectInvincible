using UnityEngine;
using UnityEngine.UI;

namespace InvincibleEngine.UnitFramework.Components {
    [RequireComponent(typeof(Image))]
    [RequireComponent(typeof(Shadow))]
    public class UnitScreenElement : MonoBehaviour {
        // Private: Settings
        private float _fadeTime = 0.1f;
        
        // Private: Required References
        private RectTransform _rectTransform;
        private Image _image;
        private Shadow _shadow;
        
        // Initialization (Sprite)
        public void Initialize(Sprite sprite, Color color) {
            // Reference required components
            _rectTransform = GetComponent<RectTransform>();
            _image = GetComponent<Image>();
            _shadow = GetComponent<Shadow>();
            
            // Set default sprite and color
            SetSprite(sprite);
            SetColor(color);
            
            // Set default outline state
            _shadow.effectColor = Color.white;
            _shadow.enabled = false;
        }
        
        // Initialization (Dimensions Only)
        public void Initialize(Vector2Int dimensions, Color color) {
            // Reference required components
            _rectTransform = GetComponent<RectTransform>();
            _image = GetComponent<Image>();
            _shadow = GetComponent<Shadow>();
            
            // Set dimensions and color
            _image.sprite = null;
            _rectTransform.sizeDelta = dimensions;
            SetColor(color);
            
            // Set default outline state
            _shadow.effectColor = Color.white;
            _shadow.enabled = false;
        }
        
        // Set the primary sprite of the icon
        public void SetSprite(Sprite sprite) {
            _image.sprite = sprite;
            _image.SetNativeSize();
        }
        
        // Set selection state of the icon
        public void SetSelected(bool selected) {
            if (_shadow == null) return;
            _shadow.enabled = selected;
        }
        
        // Set the color of the icon
        public void SetColor(Color color) {
            _image.color = color;
        }
        
        // Set the fill of the icon
        public void SetFill(float fill) {
            fill = Mathf.Clamp01(fill);

            _image.fillAmount = fill;
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
            // Set the image state
            _image.enabled = render;
        }
    }
}