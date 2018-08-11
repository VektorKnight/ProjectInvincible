using UnityEngine;

namespace InvincibleEngine.CameraSystem {
    /// <summary>
    /// Sets the attached camera to write the scene depth texture for use in various effects.
    /// </summary>
    [ExecuteInEditMode]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Camera))]
    public class WriteDepthTexture : MonoBehaviour {
        private void OnEnable() {
            GetComponent<Camera>().depthTextureMode = DepthTextureMode.DepthNormals;
        }
    }
}