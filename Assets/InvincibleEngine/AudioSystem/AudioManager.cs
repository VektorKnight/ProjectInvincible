using System.Collections.Generic;
using UnityEngine;

namespace InvincibleEngine.AudioSystem {
    public class AudioManager : MonoBehaviour {
        
        // Configuration constants
        public const int MAX_AUDIO_SOURCES = 128;
        public const int BGM_RESERVED_INDEX = 0;
        public const int NON_SPATIAL_RESERVED_START = 1;
        public const int NON_SPATIAL_RESERVED_END = 15;
        public const int MAX_INSTANCES_PER_CLIP = 8;
        
        // Static Singleton Instance
        public static AudioManager Instance { get; private set; }
        
        // Array of AudioSources
        private AudioSource[] _audioSources;
        
        // Dictionary of unique audio clips and their active counts
        private Dictionary<AudioClip, int> _uniqueClips;
        
        // Dictionary of audio source indices and assigned clips
        private Dictionary<int, AudioClip> _sourceAssignments;
        
        // Stack of free sources for UI
        private Stack<int> _nonSpatialSources;
        
        // Stack of free sources for gameplay
        private Stack<int> _spatialSources;

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
            
            // Initialize the unique clips dictionary
            _uniqueClips = new Dictionary<AudioClip, int>();
            
            // Initialize the assignments dictionary
            _sourceAssignments = new Dictionary<int, AudioClip>();
            
            // Initialize free index stacks
            _nonSpatialSources = new Stack<int>();
            _spatialSources = new Stack<int>();
            
            // Instantiate audio source objects
            for (var i = 0; i < _audioSources.Length; i++) {
                // Instantiate the audio source object
                var audioSource = new GameObject("AS" + i).AddComponent<AudioSource>();
                _audioSources[i] = audioSource;
                audioSource.transform.parent = transform;
                
                // Register with the dictionary
                _sourceAssignments.Add(i, null);
                
                // Add to the proper stack
                if (i > 0 && i < NON_SPATIAL_RESERVED_END)
                    _nonSpatialSources.Push(i);
                else 
                    _spatialSources.Push(i);
                
                // Set audio source config
                audioSource.playOnAwake = false;
                
                // Set spatial blend for 3D sources
                if (i > NON_SPATIAL_RESERVED_END) {
                    audioSource.spatialBlend = 1.0f;
                    audioSource.minDistance = 20f;
                    audioSource.maxDistance = 500f;
                }
            }
        }
        
        // Unity Update
        private void Update() {
            // Loop through the audio sources and check if they are free (skip 0 for BGM)
            for (var i = 1; i < _audioSources.Length; i++) {
                // Skip the source if it is still playing
                if (_audioSources[i].isPlaying) continue;
                
                // Skip the source if it is not assigned
                if (_sourceAssignments[i] == null) continue;
                
                // Update dictionary values
                _sourceAssignments[i] = null;
                _uniqueClips[_audioSources[i].clip]--;

                // Add the source index to the proper stack
                if (i < NON_SPATIAL_RESERVED_END)
                    _nonSpatialSources.Push(i);
                else
                    _spatialSources.Push(i);
            }
        }
        
        // Set the current BGM audio clip
        public static void SetBGMClip(AudioClip clip, float volume = 1.0f, bool playNow = true) {
            var bgmSource = Instance._audioSources[0];
            
            bgmSource.Stop();
            bgmSource.clip = clip;
            bgmSource.volume = Mathf.Clamp01(volume);
            bgmSource.loop = true;
            
            if (playNow)
                bgmSource.Play();
        }
        
        
        // Play a non-spatial audioclip
        public static bool PlayNonSpatialClip(AudioClip clip, float volume = 1.0f) {
            // Exit and return false if there are no free sources
            if (Instance._nonSpatialSources.Count == 0) return false;
            
            // Ensure this clip is registered with the dictionaries and under the limit
            if (!Instance._uniqueClips.ContainsKey(clip)) Instance._uniqueClips.Add(clip, 0);
            if (!(Instance._uniqueClips[clip] < MAX_INSTANCES_PER_CLIP)) return false;
            
            // Pop a source ID from the stack
            var sourceID = Instance._nonSpatialSources.Pop();
            var audioSource = Instance._audioSources[sourceID];
            
            // Update dictionaries
            Instance._uniqueClips[clip]++;
            Instance._sourceAssignments[sourceID] = clip;
            
            // Configure the source
            audioSource.clip = clip;
            audioSource.volume = Mathf.Clamp01(volume);
            audioSource.Play();
            
            // Return true
            return true;
        }
        
        // Play an AudioClip at a specified world position
        public static bool PlayClipAtPosition(Vector3 position, AudioClip clip, float volume = 1.0f) {
            // Exit and return false if there are no free sources
            if (Instance._spatialSources.Count == 0) return false;
            
            // Ensure this clip is registered with the dictionaries and under the limit
            if (!Instance._uniqueClips.ContainsKey(clip)) Instance._uniqueClips.Add(clip, 0);
            if (!(Instance._uniqueClips[clip] < MAX_INSTANCES_PER_CLIP)) return false;
            
            // Pop a source ID from the stack
            var sourceID = Instance._spatialSources.Pop();
            var audioSource = Instance._audioSources[sourceID];
            
            // Update dictionaries
            Instance._uniqueClips[clip]++;
            Instance._sourceAssignments[sourceID] = clip;
            
            // Configure the source
            audioSource.clip = clip;
            audioSource.transform.position = position;
            audioSource.volume = Mathf.Clamp01(volume);
            audioSource.Play();
            
            // Return true
            return true;
        }
    }
}