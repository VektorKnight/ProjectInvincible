using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace InvincibleEngine.UnitFramework.DataTypes {
    [RequireComponent(typeof(Image))]
    [RequireComponent(typeof(Shadow))]
    public class UnitIcon : MonoBehaviour {
        // Private: Settings
        private float _fadeTime = 0.1f;
        
        // Private: Required References
        private Image _image;
        private Shadow _shadow;
        
        // Initialization
        public void Initialize(Sprite sprite, Color color) {
            // Reference required components
            _image = GetComponent<Image>();
            _shadow = GetComponent<Shadow>();
            
            // Set default sprite and color
            SetSprite(sprite);
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
            // Set the image state
            _image.enabled = render;
        }
    }
}