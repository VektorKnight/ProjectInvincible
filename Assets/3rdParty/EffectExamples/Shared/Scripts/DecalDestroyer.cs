using System.Collections;
using UnityEngine;

namespace _3rdParty.EffectExamples.Shared.Scripts {
	public class DecalDestroyer : MonoBehaviour {

		public float lifeTime = 5.0f;

		private IEnumerator Start()
		{
			yield return new WaitForSeconds(lifeTime);
			Destroy(gameObject);
		}
	}
}
