using System;
using System.Runtime.InteropServices;

namespace InvincibleEngine.DataTypes {
    public struct CachedAsset {
        // Asset and ID
        public object Asset;
        public Type Type;
        public int ID;
        
        // Constructor
        public CachedAsset(object asset, Type type, int id) {
            Asset = asset;
            Type = type;
            ID = id;
        }
    }
}