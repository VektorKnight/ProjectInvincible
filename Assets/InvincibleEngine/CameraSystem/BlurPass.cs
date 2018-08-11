using UnityEngine;
using UnityEngine.Rendering;

namespace InvincibleEngine.CameraSystem {
	/// <summary>
	/// Downsamples, blurs, and writes the camera pass to a global texture.
	/// </summary>
	[ExecuteInEditMode]
	[DisallowMultipleComponent]
	[RequireComponent(typeof(Camera))]
	public class BlurPass : MonoBehaviour {
		// Unity Inspector
		[Header("Configuration")] 
		[SerializeField] private CameraEvent _bufferEvent = CameraEvent.AfterImageEffects;
		[SerializeField] [Range(1f, 8f)] private float _blurPower = 6;
		[SerializeField] [Range(1, 16)] private int _downSample = 4;
	
		// Private: Required Reference
		private Camera _camera;
		private CommandBuffer _buffer;
		private bool _bufferInitialized;
	
		// Private: Blur Material Instance
		private Shader _blurShader;
		private Material _blurMaterial;
	
		// Initialization
		private void Start() {
			// Load blur shader if null
			if (_blurShader == null)
				_blurShader = Resources.Load<Shader>("Shaders/Effect/SeparableBlur");
		
			// Instantiate the blur material
			_blurMaterial = new Material(_blurShader);
		
			// Reference attached camera
			_camera = GetComponent<Camera>();
		}

		// Remove command buffers from all cameras we added into
		private void Cleanup() {
			if (!_bufferInitialized)
				return;
		
			_camera.RemoveCommandBuffer (_bufferEvent, _buffer);
			_bufferInitialized = false;
		}
	
		// OnEnable Callback
		public void OnEnable() {
			Cleanup();
		}
	
		// OnDisable Callback
		public void OnDisable() {
			Cleanup();
		}
	
		// Unity Update
		public void Update()
		{
			var act = gameObject.activeInHierarchy && enabled;
			if (!act)
			{
				Cleanup();
				return;
			}
		
			// Exit if we've already set up the command buffer
			if (_bufferInitialized)
				return;
		
			// Set up command buffer
			_buffer = new CommandBuffer { name = "Grab, downsample, and blur screen" };

			// copy screen into temporary RT
			var screenCopyID = Shader.PropertyToID("_ScreenCopyTexture");
			_buffer.GetTemporaryRT (screenCopyID, -1, -1, 0, FilterMode.Bilinear);
			_buffer.Blit (BuiltinRenderTextureType.CurrentActive, screenCopyID);
		
			// Create two smaller RTs
			var blurredID = Shader.PropertyToID("_Temp1");
			var blurredID2 = Shader.PropertyToID("_Temp2");
			_buffer.GetTemporaryRT (blurredID, -_downSample, -_downSample, 0, FilterMode.Bilinear);
			_buffer.GetTemporaryRT (blurredID2, -_downSample, -_downSample, 0, FilterMode.Bilinear);
		
			// Downsample screen copy into smaller RT, release screen RT
			_buffer.Blit (screenCopyID, blurredID);
			_buffer.ReleaseTemporaryRT (screenCopyID); 
		
			// horizontal blur
			_buffer.SetGlobalVector("offsets", new Vector4(_blurPower/Screen.width,0,0,0));
			_buffer.Blit (blurredID, blurredID2, _blurMaterial);
			// vertical blur
			_buffer.SetGlobalVector("offsets", new Vector4(0,_blurPower/Screen.height,0,0));
			_buffer.Blit(blurredID2, blurredID, _blurMaterial);
			// horizontal blur
			_buffer.SetGlobalVector("offsets", new Vector4(2f * _blurPower/Screen.width,0,0,0));
			_buffer.Blit (blurredID, blurredID2, _blurMaterial);
			// vertical blur
			_buffer.SetGlobalVector("offsets", new Vector4(0,2f * _blurPower/Screen.height,0,0));
			_buffer.Blit (blurredID2, blurredID, _blurMaterial);

			_buffer.SetGlobalTexture("_DSGrabTex", blurredID);
			_buffer.SetGlobalInt("DSGrabRatio", _downSample);

			_camera.AddCommandBuffer (_bufferEvent, _buffer);
		
			// Set the buffer initialized flag
			_bufferInitialized = true;
		}	
	}
}
