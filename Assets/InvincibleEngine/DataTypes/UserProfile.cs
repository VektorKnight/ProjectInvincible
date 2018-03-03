using System;
using UnityEngine;

namespace InvincibleEngine.DataTypes {
    [Serializable]
    public class UserProfile {
        public bool IsLocalProfile { get; }                // True if this profile is not tied to a Steam profile, i.e local players 2-4
        public ulong UniqueId { get; }                     // The ID of the Steam user tied to this profile, will be a smaller number if local
        public string DisplayName { get; private set; }    // The display name of this profile
        public UserSettings Settings { get; set; }         // The game settings tied to this profile, ignored if local user

        // Constructor
        public UserProfile(bool isLocal, ulong uniqueId, string displayName) {
            IsLocalProfile = isLocal;
            UniqueId = uniqueId;
            DisplayName = displayName;
            Settings = new UserSettings(Application.version);
        }
        
        // Edit the profile name
        public void SetDisplayName(string name) {
            if (name == string.Empty) return;
            DisplayName = name;
        }
    }
}