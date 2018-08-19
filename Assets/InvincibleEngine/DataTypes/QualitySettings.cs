using System;

namespace InvincibleEngine.DataTypes {
    /// <summary>
    /// Set of values for quality levels.
    /// </summary>
    [Serializable]
    public struct QualitySettings {
        public AntiAliasingMode AntiAliasing;
        public bool EnableAo;
        public bool EnableBloom;
        public bool EnableMotionBlur;
        public bool EnableVolumetrics;
        public bool EnableReflections;
    }
}