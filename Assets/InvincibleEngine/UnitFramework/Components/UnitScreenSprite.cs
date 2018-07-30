using UnityEngine;
using UnityEngine.UI;

namespace InvincibleEngine.UnitFramework.Components {
    [RequireComponent(typeof(SpriteRenderer))]
    public class UnitScreenSprite : MonoBehaviour {      
        // Private: Required References
        private SpriteRenderer _renderer;
        
        // Initialization
        public void Initialize(Sprite sprite, Color color) {
            // Reference required components
            _renderer = GetComponent<SpriteRenderer>();
            
            // Set default sprite and color
            SetSprite(sprite);
            SetColor(color);
        }
        
        // Set the primary sprite of the icon
        public void SetSprite(Sprite sprite) {
            _renderer.sprite = sprite;
        }
        
        // Set selection state of the icon
        public void SetSelected(bool selected) {
            // TODO: Implement selection behavior
        }
        
        // Set the color of the icon
        public void SetColor(Color color) {
            _renderer.color = color;
        }
        
        // Set the fill of the icon
        public void SetScale(Vector2 scale) {
            transform.localScale = scale;
        }
        
        // Set the parent canvas
        public void SetParent(Transform parent) {
            transform.parent = parent;
        }
        
        // Set the screen position
        public void SetScreenPosition(Vector2 position) {
            transform.localPosition = new Vector3(position.x - (Screen.width / 2f), position.y - (Screen.height / 2f), 1f);
        }
        
        // Set render state (show/hide)
        public void SetRender(bool render) {
            // Set the image state
            _renderer.enabled = render;
        }
    }
}