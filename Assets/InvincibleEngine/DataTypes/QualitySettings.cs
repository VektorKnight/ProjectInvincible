using System;
using _3rdParty.PostProcessing.Runtime;

namespace InvincibleEngine.DataTypes {
    /// <summary>
    /// Set of values for quality levels.
    /// </summary>
    [Serializable]
    public struct QualitySettings {
        public PostProcessingProfile Profile;
        public AntiAliasingMode AntiAliasing;
        public bool EnableAo;
        public bool EnableBloom;
        public bool EnableMotionBlur;
        public bool EnableVolumetrics;
        public bool EnableReflections;
    }
}