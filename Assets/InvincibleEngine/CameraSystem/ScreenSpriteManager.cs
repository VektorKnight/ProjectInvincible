using InvincibleEngine.UnitFramework.Components;
using UnityEngine;

namespace InvincibleEngine.CameraSystem {
    /// <summary>
    /// Used to render sprites in screen-space avoiding the use of the UI system.
    /// </summary>
    public class ScreenSpriteManager : MonoBehaviour {
        // Static singleton instance
        public static ScreenSpriteManager Instance { get; private set; }
        
        // Private: Required References
        private Camera _camera;
        private int _screenHeight;
        
        // Preload method
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Preload() {
            // Create the container object and attach necessary components
            var container = new GameObject("ScreenSpaceManager");
            Instance = container.AddComponent<ScreenSpriteManager>();

            // Ensure this singleton does not get destroyed on scene load
            DontDestroyOnLoad(Instance.gameObject);
            
            // Initialize the instance
            Instance.Initialize();
        }
        
        // Initialization
        private void Initialize() {
            // Instantiate and reference required components
            _camera = gameObject.AddComponent<Camera>();

            _screenHeight = Screen.height;
            
            // Initialize the camera
            _camera.clearFlags = CameraClearFlags.Depth;
            _camera.cullingMask = 1 << LayerMask.NameToLayer("ScreenSprites");
            _camera.orthographic = true;
            _camera.renderingPath = RenderingPath.Forward;
            _camera.orthographicSize = _screenHeight / 2f;
        }
        
        // Append a sprite to the screen layer
        public static void AppendSprite(UnitScreenSprite sprite) {
            sprite.SetParent(Instance.transform);
            sprite.SetScreenPosition(Vector2.zero);
        }

        // Unity update
        private void Update() {
            if (Screen.height == _screenHeight) return;
            // Recalculate the camera orthographic size
            _screenHeight = Screen.height;
            _camera.orthographicSize = _screenHeight / 2f;
        }
    }
}