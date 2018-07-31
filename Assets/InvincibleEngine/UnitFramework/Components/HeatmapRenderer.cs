using UnityEngine;
using UnityEngine.UI;

namespace InvincibleEngine.UnitFramework.Components {
    [RequireComponent(typeof(Image))]
    public class HeatmapRenderer : MonoBehaviour {

        public ComputeShader Shader;

        private Texture2D _heatmap;

        private Image _image;
        
        private void Awake() {

            _image = GetComponent<Image>();

            RenderTexture tex = new RenderTexture(256,256,24);
            tex.enableRandomWrite = true;
            tex.Create();
            
            var handle = Shader.FindKernel("CSMain");
            Shader.SetTexture(handle, "Result", tex);
            Shader.Dispatch(handle, 1, 1, 1);
            
            _image.material.SetTexture("_MainTex", tex);
        }
    }
}