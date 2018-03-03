using UnityEngine;

namespace InvincibleEngine.WeaponSystem.Components.Weapons {
	/// <summary>
	/// Simple weapon class suitable for basic semi and fully automatic weapons.
	/// </summary>
	[AddComponentMenu("Weapon System/Weapons/Basic Weapon")]
	public class BasicWeapon : Weapon {
		
		// Unity Inspector
		[Header("Weapon Atributes")] 
		[SerializeField] private bool _isFullAuto;
		[SerializeField] private bool _hasChargeSequence;	
		
		// Public Readonly: Weapon Attributes
		public bool IsFullAuto => _isFullAuto;
		public bool HasChargeSequence => _hasChargeSequence;
		
		/// <summary>
		/// Causes the weapon to begin its firing sequence. Best linked to an input axis.
		/// </summary>
		public override void TriggerDown() {
			// Branch for semi/full-auto weapons
			if (IsFullAuto || HasChargeSequence) WeaponAnimator.SetBool("Fire", true);
			else WeaponAnimator.SetTrigger("Fire");
		}

		/// <summary>
		/// Causes the weapon to abort its firing sequence. Best linked to an input axis.
		/// </summary>
		public override void TriggerUp() {
			// Toggle animator boolean
			WeaponAnimator.SetBool("Fire", false);
		}
		
		/// <summary>
		/// Not implemented on Basic Weapon
		/// </summary>
		public override void Reload() { }
		
		/// <summary>
		/// Not implemented on Basic Weapon
		/// </summary>
		public override void ResetFireState() { }
	}
}
