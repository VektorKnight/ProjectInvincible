using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace InvincibleEngine.Managers {
    public static class AssetManager{

        private static List<NetworkEntity> _manifest = new List<NetworkEntity>();

        /// <summary>
        /// On game start generate recurrsive and non-random asset directory
        /// who's ID is the index of the obejct in the collection
        /// </summary>
        [RuntimeInitializeOnLoadMethod]
        static void GenerateManifest() {
            Debug.Log("<color=blue>Asset Manager generating manifest...</Color>");
        
            // Load all Gameobjects (prefabs) into an array
            var loadedResources = Resources.LoadAll<NetworkEntity>("");

            // Convert to a list for data manipuiation if necessary
            _manifest = loadedResources.ToList<NetworkEntity>();

            foreach(NetworkEntity n in _manifest) {
                Debug.Log($"<color=blue>Asset {n.name} found with ID: {_manifest.IndexOf(n)}</Color>");
                n.AssetID = (ushort)_manifest.IndexOf(n);
            }
            Debug.Log($"<color=blue>...Done. Found {_manifest.Count} prefabs in resource folder that can be spawned</Color>");
        }

        /// <summary>
        /// Retrieve asset by ID
        /// </summary>
        /// <param name="ID">Asset ID</param>
        /// <returns></returns>
        public static GameObject LoadAssetByID(ushort ID) {
            return _manifest[ID].gameObject;
        }

        public static ushort GetIDByAsset(GameObject asset) {
            return asset.GetComponent<NetworkEntity>().AssetID;
        }
    }
}
