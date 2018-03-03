using InvincibleEngine.WeaponSystem.Components.Weapons;
using UnityEngine;

namespace InvincibleEngine.Components.Generic {
	public class WeaponTest : MonoBehaviour {

		public Transform Target;
		public BasicWeapon Weapon;

		// Use this for initialization
		void Start () {
		
		}
	
		// Update is called once per frame
		void Update () {
			transform.LookAt(Target);
			InvokeRepeating("Fire", 2f, 2f);
		}

		void Fire() {
			Weapon.TriggerDown();
		}
	}
}
