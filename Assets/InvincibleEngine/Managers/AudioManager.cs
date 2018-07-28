using UnityEngine;

namespace InvincibleEngine.Managers {
    public class AudioManager : MonoBehaviour {
        
        // Configuration constants
        public const int MAX_AUDIO_SOURCES = 32;
        public const int BGM_RESERVED_INDEX = 0;
        public const int UI_RESERVED_START = 1;
        public const int UI_RESERVED_END = 7;
        public const int MAX_INSTANCES_PER_CLIP = 4;
        
        // Static Singleton Instance
        public static AudioManager Instance { get; private set; }
        
        // Array of AudioSources
        private AudioSource[] _audioSources;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Preload() {
            //Make sure the Managers object exists
            GameObject managers = GameObject.Find("Managers") ?? new GameObject("Managers");

            // Ensure this singleton initializes at startup
            if (Instance == null) Instance = managers.GetComponent<AudioManager>() ?? managers.AddComponent<AudioManager>();

            // Ensure this singleton does not get destroyed on scene load
            DontDestroyOnLoad(Instance.gameObject);
            
            // Initialize the instance
            Instance.Initialize();
        }
        
        // Initialization
        private void Initialize() {
            // Initialize the audio source array
            _audioSources = new AudioSource[MAX_AUDIO_SOURCES];
            
            // Instantiate audio source objects
            for (var i = 0; i < _audioSources.Length; i++) {
                // Instantiate the audio source object
                var audioSource = new GameObject("AS" + i).AddComponent<AudioSource>();
                
                // Set audio source config
                audioSource.playOnAwake = false;
                
                // Set spatial blend for 3D sources
                if (i > UI_RESERVED_END) {
                    audioSource.spatialBlend = 1.0f;
                }
            }
        }
    }
}