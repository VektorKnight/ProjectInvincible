using UnityEngine;

namespace _3rdParty.EffectExamples.Shared.Scripts {
	[System.Serializable]
	public class ParticleExamples {

		public string title;
		[TextArea]
		public string description;
		public bool isWeaponEffect;
		public GameObject particleSystemGO;
		public Vector3 particlePosition, particleRotation;
	}
}
