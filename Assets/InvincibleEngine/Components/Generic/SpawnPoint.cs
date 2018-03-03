using InvincibleEngine.Managers;
using UnityEngine;

namespace InvincibleEngine.Components.Generic {
	public class SpawnPoint : MonoBehaviour {
		
		// Unity Inspector
		[SerializeField] private Team _team = Team.None;

		// Public Readonly: Team Assignment
		public Team Team => _team;
	}
}
