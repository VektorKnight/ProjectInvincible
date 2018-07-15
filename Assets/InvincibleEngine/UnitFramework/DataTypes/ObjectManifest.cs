
using UnityEngine;
using VektorLibrary.Collections;

namespace InvincibleEngine.UnitFramework.DataTypes {
    /// <summary>
    /// Defines all spawnable objects within the main game and any expansions.
    /// 
    /// The default manifest is "Internal"
    /// </summary>
    public class ObjectManifest : ScriptableObject {
        // Manifest Objects
        private HashedArray<GameObject> _objects;
    }
}