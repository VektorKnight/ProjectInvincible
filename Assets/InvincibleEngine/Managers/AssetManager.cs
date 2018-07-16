using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;


using UnityEngine;


public static class AssetManager{

    private static List<NetworkEntity> Manifest = new List<NetworkEntity>();

    /// <summary>
    /// On game start generate recurrsive and non-random asset directory
    /// who's ID is the index of the obejct in the collection
    /// </summary>
    [RuntimeInitializeOnLoadMethod]
    static void GenerateManifest() {
        Debug.Log("<color=blue>Asset Manager generating manifest...</Color>");
        
        //Load all Gameobjects (prefabs) into an array
        var loadedResources = Resources.LoadAll<NetworkEntity>("");

        //Convert to a list for data manipuiation if necessary
        Manifest = loadedResources.ToList<NetworkEntity>();

        foreach(NetworkEntity n in Manifest) {
            Debug.Log($"<color=blue>Asset {n.name} found with ID: {Manifest.IndexOf(n)}</Color>");
            n.AssetID = (ushort)Manifest.IndexOf(n);
        }
        Debug.Log($"<color=blue>...Done. Found {Manifest.Count} prefabs in resource folder that can be spawned</Color>");
    }

    /// <summary>
    /// Retrieve asset by ID
    /// </summary>
    /// <param name="ID">Asset ID</param>
    /// <returns></returns>
    public static GameObject LoadAssetByID(ushort ID) {
        return Manifest[ID].gameObject;
    }

    public static ushort GetIDByAsset(GameObject asset) {
        return asset.GetComponent<NetworkEntity>().AssetID;
    }
}
