using InvincibleEngine.CameraSystem;
using InvincibleEngine.Components.Player;
using InvincibleEngine.HudSystem;
using UnityEngine;
using XInputDotNetPure;

namespace InvincibleEngine.Managers {
	/// <summary>
	/// Handles various functions as well as gameplay entities for any connected players both local and networked.
	/// </summary>
	public class PlayerManager : MonoBehaviour {
		
		// Unity Inspector
		[Header("Player Assignment")]
		[SerializeField] private PlayerIndex _playerIndex;
		[SerializeField] private Team _playerTeam;
		
		[Header("Required Prefabs")] 
		[SerializeField] private VektorCamera _cameraSystemPrefab;
		[SerializeField] private PlayerHud _hudSystemPrefab;
		[SerializeField] private PlayerEntity _playerEntityPrefab;
		
		// Private: State
		private bool _initialized;
		
		// Public Readonly: Player Metadata
		public ulong UniqueId { get; private set; }
		public PlayerIndex PlayerIndex => _playerIndex;
		public Team PlayerTeam => _playerTeam;

		// Public Readonly: Attached Objects
		public VektorCamera CameraSystem { get; private set; }
		public PlayerHud HudSystem { get; private set; }
		public PlayerEntity PlayerEntity { get; private set; }

		// Initialization
		// TODO: Split instantiation of gameplay systems into a seperate function
		// TODO: So that spawning of gameplay systems are no longer tied to initialization
		public void Initialize(ulong uniqueId, PlayerIndex index, Team team) {
			// Exit if already initialized
			if (_initialized) return;
			
			// Assign player id, index and team
			UniqueId = uniqueId;
			_playerIndex = index;
			_playerTeam = team;
			
			// Spawn in and assign required systems
			CameraSystem = Instantiate(_cameraSystemPrefab, Vector3.zero, Quaternion.identity);
			HudSystem = Instantiate(_hudSystemPrefab, Vector3.zero, Quaternion.identity);
			
			// Grab a spawn point and spawn in the player entity
			var spawnPoint = GameManager.GetSpawnPoint(_playerTeam);
			PlayerEntity = Instantiate(_playerEntityPrefab, spawnPoint.transform.position + Vector3.up * 3f, spawnPoint.transform.rotation);
			PlayerEntity.Initialize(this);
			
			// Set up required systems
			CameraSystem.Initialize(this, PlayerIndex, PlayerEntity.transform);
			
			// Initialization complete
			_initialized = true;
		}
		
		// Called when the player has died
		public void OnPlayerDeath(PlayerEntity entity, ulong entityId) {
			// Exit if the PlayerEntity is null
			if (PlayerEntity == null) return;
			
			// Inform the Game Manager that a player has died
			GameManager.OnPlayerDeath(UniqueId, entityId);
			
			// Show the respawn hud element
			HudSystem.ShowRespawnTimer();
			
			// Deactivate the player entity
			PlayerEntity.gameObject.SetActive(false);
			
			// Invoke a respawn event
			Invoke(nameof(RespawnPlayer), GameManager.RespawnDelay);
		}

		private void RespawnPlayer() {
			// Exit if the PlayerEntity is null
			if (PlayerEntity == null) return;
			
			// Grab a spawn point and reactivate the player entity
			var spawnPoint = GameManager.GetSpawnPoint(_playerTeam);
			PlayerEntity.transform.SetPositionAndRotation(spawnPoint.transform.position + Vector3.up * 3f, spawnPoint.transform.rotation);
			PlayerEntity.gameObject.SetActive(true);
			
			// Call the respawn method on the player entity
			PlayerEntity.OnPlayerRespawn(this);
			
			// Return the spawn point
			GameManager.ReturnSpawnPoint(spawnPoint);
		}
	}
}
