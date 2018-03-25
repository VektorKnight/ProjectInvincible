using System;
using UnityEngine;

namespace InvincibleEngine.DataTypes {
    /// <summary>
    /// Holds configuration values set by the user.
    /// </summary>
    [Serializable]
    public class UserSettings {
        // Version Check
        public string Version;
        
        // Graphics Settings: Screen
        public int[] Resolution;
        public ScreenMode ScreenMode;
        public float FrameRateTarget;
        public bool EnableVsync;
        
        // Graphics Settings: Fidelity
        public QualityLevel QualityPreset;
        
        // Audio Settings
        public float MusicVolume;
        public float SfxVolume;
        public bool EnableMusic;
        public bool EnableSfx;
        
        // Input Settings
        public float MouseSensitivity;
        public float StickSensitivity;
        public bool InvertHorizontal;
        public bool InvertVertical;
        
        // Misc Settings
        public bool ShowFps;
        public bool ShowNetStats;
        public bool ShowDebugInfo;
        
        // Generate and set default config values
        public UserSettings(string version) {
            Version = version;
            Resolution = new[] { Screen.width, Screen.height, Screen.currentResolution.refreshRate};
            ScreenMode = Screen.fullScreen ? ScreenMode.Fullscreen : ScreenMode.Windowed;
            FrameRateTarget = Screen.currentResolution.refreshRate;
            EnableVsync = true;
            
            QualityPreset = QualityLevel.High;

            MusicVolume = 1.0f;
            SfxVolume = 1.0f;
            EnableMusic = true;
            EnableSfx = true;
            MouseSensitivity = 1.0f;
            StickSensitivity = 1.0f;
            InvertHorizontal = false;
            InvertVertical = false;

            ShowFps = false;
            ShowNetStats = false;
            ShowDebugInfo = false;
        }
    }
    
    [Serializable]
    public enum ScreenMode {
        Windowed,
        Fullscreen
    }
    
    [Serializable]
    public enum QualityLevel {
        Low,
        Medium,
        High,
        Ultra,
        Custom
    }

    public enum AntiAliasingMode {
        FXAA,
        SMAA,
        TAA
    }
}