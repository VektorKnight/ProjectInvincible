using System;
using UnityEngine;

namespace InvincibleEngine.WeaponSystem.DataTypes {
    [Serializable]
    public class SoundParticlePair {
        public AudioClip SoundEffect;
        public ParticleSystem ParticleEffect;
    }
}