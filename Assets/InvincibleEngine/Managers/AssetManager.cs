using InvincibleEngine.UnitFramework.Components;
using System.Collections.Generic;
using System.Linq;
using InvincibleEngine.DataTypes;
using InvincibleEngine.Utility;
using UnityEngine;
using VektorLibrary.Utility;

namespace InvincibleEngine.Managers {
    /// <summary>
    /// Handles loading of assets from disk into memory.
    /// </summary>
    public static class AssetManager {

        [Header("Manifest of all entities")]
        [SerializeField] private static List<UnitBehavior> _manifest = new List<UnitBehavior>();
        [SerializeField] public static MapData[] LoadedMaps;

        [Header("Globally spawnable objects")]
        [SerializeField] public static StructureBehavior CommandCenter;
        
        // Runtime asset cache (projectiles, particle effects, etc)
        private static Dictionary<string, CachedAsset> _cachedAssets;

        /// <summary>
        /// On game start generate recurrsive and non-random asset directory
        /// who's ID is the index of the obejct in the collection
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void GenerateManifest() {
            Debug.Log("<color=blue>Asset Manager generating manifest...</Color>");

            //----------------------------------------------------
            #region  Generate manifest of all generic unit behaviors for spawning
            //----------------------------------------------------

            // Load all Gameobjects (prefabs) into an array
            var loadedResources = Resources.LoadAll<UnitBehavior>("");

            // Convert to a list for data manipulation if necessary
            _manifest = loadedResources.ToList<UnitBehavior>();


            //Assign asset values to each object
            for(int i=0; i<_manifest.Count; i++) {
                _manifest[i].AssetID = (ushort)i;
                
                Debug.Log($"Loaded resource {_manifest[i].name}");
            }

            #endregion

            //----------------------------------------------------
            #region  Grab other assets used in other parts of the game
            //----------------------------------------------------

            //Populate globally spawnable objects
            CommandCenter = Resources.Load<StructureBehavior>("Objects/Structures/CommandCenter/CommandCenter");
            
            //Grab loadable maps
            LoadedMaps = Resources.LoadAll<MapData>("");

            //Sort them
            LoadedMaps.OrderBy(o => o.BuildIndex);

            #endregion

            // Initialize the asset cache
            _cachedAssets = new Dictionary<string, CachedAsset>();
        }

        /// <summary>
        /// Retrieve asset by ID
        /// </summary>
        /// <param name="ID">Asset ID</param>
        /// <returns></returns>
        public static UnitBehavior LoadAssetByID(ushort ID) {
            return _manifest[ID].gameObject.GetComponent<UnitBehavior>();
        }

        /// <summary>
        /// Get ID from asset prefab
        /// </summary>
        /// <param name="asset"></param>
        /// <returns></returns>
        public static ushort GetIDByAsset(GameObject asset) {
            return asset.GetComponent<UnitBehavior>().AssetID;
        }
        
        /// <summary>
        /// Tries to load an asset from the resources folder and cache it for later use.
        /// </summary>
        /// <param name="path">The path of the object to be loaded.</param>
        /// <typeparam name="T">The type of object to be loaded.</typeparam>
        /// <returns>The loaded object, null if it fails.</returns>
        public static T LoadAsset<T>(string path) where T : Object {
            // Check to see if the asset exists in the cache
            if (_cachedAssets.ContainsKey(path)) {
                // Load the asset from the cache
                var cachedAsset = _cachedAssets[path];
                var type = cachedAsset.Type;
                
                // Perform a sanity check on the type and return the asset if possible
                if (type == typeof(T)) {
                    // Return the cached asset
                    return cachedAsset.Asset as T;
                }
                
                // Log an error to the console and remove the corrupted object from the cache
                DevConsole.LogError("AssetManager", $"Type mismatch occurred on cached asset <b>{path}</b>!\n" +
                                                    $"Will attempt to load from disk instead.");
                
                _cachedAssets.Remove(path);
            }
            
            // Try to load the specified asset from disk
            var loadedAsset = Resources.Load<T>(path);
            
            // If load was successful, cache and return the asset
            if (loadedAsset != null) {
                // Add to the asset cache (will be ignored if already cached)
                _cachedAssets.Add(path, new CachedAsset(loadedAsset, typeof(T), loadedAsset.GetHashCode()));
                
                // Log a message to the console
                DevConsole.Log("AssetManager", $"Successfully loaded and cached asset at <b>{path}</b>.");
                
                // Return the loaded asset
                return loadedAsset;
            }
            
            // Log an error to the console and return null
            DevConsole.LogError("AssetManager", $"Failed to load the specified asset at <b>{path}</b>!");
            return null;
        }
    }
}
