using InvincibleEngine.UnitFramework.Components;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace InvincibleEngine.Managers {
    public static class AssetManager {

        [Header("Manifest of all entities")]
        [SerializeField] private static List<NetworkEntity> _manifest = new List<NetworkEntity>();
        [SerializeField] public static MapData[] LoadedMaps;

        [Header("Globally spawnable objects")]
        [SerializeField] public static GameObject CommandCenter;

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
            var loadedResources = Resources.LoadAll<NetworkEntity>("");

            // Convert to a list for data manipuiation if necessary
            _manifest = loadedResources.ToList<NetworkEntity>();

            foreach (NetworkEntity n in _manifest) {
                Debug.Log($"<color=blue>Asset {n.name} found with ID: {_manifest.IndexOf(n)}</Color>");
                n.AssetID = (ushort)_manifest.IndexOf(n);
            }

            Debug.Log($"<color=blue>...Done. Found {_manifest.Count} prefabs in resource folder that can be spawned</Color>");

            #endregion

            //----------------------------------------------------
            #region  Grab other assets used in other parts of the game
            //----------------------------------------------------

            //Populate globally spawnable objects
            CommandCenter = (GameObject)Resources.Load<GameObject>("Objects/Structures/CommandCenter/CommandCenter");
            
            //Grab loadable maps
            LoadedMaps = Resources.LoadAll<MapData>("");

            #endregion
        }

        /// <summary>
        /// Retrieve asset by ID
        /// </summary>
        /// <param name="ID">Asset ID</param>
        /// <returns></returns>
        public static GameObject LoadAssetByID(ushort ID) {
            return _manifest[ID].gameObject;
        }

        /// <summary>
        /// Get ID from asset prefab
        /// </summary>
        /// <param name="asset"></param>
        /// <returns></returns>
        public static ushort GetIDByAsset(GameObject asset) {
            return asset.GetComponent<NetworkEntity>().AssetID;
        }
    }
}
