using System;
using InvincibleEngine.HudSystem;
using InvincibleEngine.Managers;
using InvincibleEngine.WeaponSystem.Interfaces;
using UnityEngine;

namespace InvincibleEngine.Components.Player {
	[RequireComponent(typeof(HoverTankController))]
	[RequireComponent(typeof(PlayerWeaponController))]
	public class PlayerEntity : MonoBehaviour, IDestructable {
		
		// Unity Inspector
		[Header("Required Prefabs")] 
		[SerializeField] private GameObject _wreckagePrefab;
		
		[Header("Armor Settings")] 
		[SerializeField] private float _maxArmor = 100f; // The maximum armor of the player
		[SerializeField] private float _armorDamageRatio = 0.75f; // Fraction of damage that the armor will absorb
		[SerializeField] private bool _armorRegen = true; // Whether or not armor will passively regenerate
		[SerializeField] private float _armorRegenDelay = 3.0f; // Armor regen delay after taking damage
		[SerializeField] private float _armorRegenRate = 10.0f; // Rate at which armor will regenerate (units/second)
		[SerializeField] private float _armorRegenLimit = 1.0f; // Armor will only regenerate up to this fraction of the total
		
		[Header("Health Settings")] 
		[SerializeField] private bool _healthRegen = true; // Whether or not health will passively regenerate
		[SerializeField] private float _maxhealth = 100f; // The maximum health of the player
		[SerializeField] private float _healthRegenDelay = 5.0f; // Health regen delay after taking damage
		[SerializeField] private float _healthRegenRate = 20.0f; // Rate at which health will regenerate (units/second)
		[SerializeField] private float _healthRegenLimit = 1.0f; // Health will only regenerate up to this fraction of the total

		[Header("Energy Shield Effects")] 
		[SerializeField] private PlayerShield _playerShield;
		
		// Private: State
		private bool _initialized;
		private PlayerState _playerState;
		
		// Private: Required References
		private PlayerManager _playerManager;
		private PlayerHud _playerHud;
		
		// Private: Armor Stats
		private float _armorRegenTimer;
		private float _armorRegenTotal;
		
		// Private: Health Stats
		private float _healthRegenTimer;
		private float _healthRegenTotal;
		
		// Private: Damage Source
		private ulong _lastDamageSource;
		
		// Public Readonly: Player Stats
		public float MaxArmor => _maxArmor;
		public float CurrentArmor { get; private set; }

		public float Maxhealth => _maxhealth;
		public float CurrentHealth { get; private set; }

		// Initialization
		public void Initialize(PlayerManager manager) {
			// Exit if already initialized
			if (_initialized) return;
			
			// Assign required references
			_playerManager = manager;
			_playerHud = manager.HudSystem;
			
			// Set initial stat values
			CurrentArmor = _maxArmor;
			CurrentHealth = _maxhealth;
			
			// Set up entity components
			var materials = GetComponent<MeshRenderer>().sharedMaterials;
			materials[2] = GameManager.TeamColors[(int) _playerManager.PlayerTeam];
			GetComponent<MeshRenderer>().sharedMaterials = materials;
			GetComponent<HoverTankController>().Initialize(_playerManager);
			GetComponent<PlayerWeaponController>().Initialize(_playerManager);
			
			// Initialization complete!
			_initialized = true;
			_playerState = PlayerState.Alive;
		}
	
		// Update is called once per frame
		private void Update () {
			// Exit if not initialized or player is dead
			if (!_initialized || _playerState == PlayerState.Dead) return;
			
			// Decrement the delay timers if necessary
			if (_armorRegenTimer > 0f) _armorRegenTimer -= Time.deltaTime;
			if (_healthRegenTimer > 0f) _healthRegenTimer -= Time.deltaTime;
			
			// Regen stats to their limit if enabled and delay timers have expired
			if (_armorRegen && _armorRegenTimer <= 0f) {
				if (_armorRegenTotal < _armorRegenLimit * _maxArmor) {
					var regen = _armorRegenRate * Time.deltaTime;
					CurrentArmor += regen;
					_armorRegenTotal += regen;
					_playerShield?.FlareShield();
				}
			}
			if (_healthRegen && _healthRegenTimer <= 0f) {
				if (_healthRegenTotal < _healthRegenLimit * _maxhealth) {
					var regen = _healthRegenRate * Time.deltaTime;
					CurrentHealth += regen;
					_healthRegenTotal += regen;
				}
			}
			
			// Clamp stat values to the range (0, max)
			CurrentArmor = Mathf.Clamp(CurrentArmor, 0f, _maxArmor);
			CurrentHealth = Mathf.Clamp(CurrentHealth, 0f, _maxhealth);
			
			// Update the player hud
			_playerHud.ArmorValue = CurrentArmor / _maxArmor;
			_playerHud.HealthValue = CurrentHealth / _maxhealth;
			
			// Check if the player should be dead
			if (CurrentHealth <= 0f && _playerState == PlayerState.Alive) {
				_playerState = PlayerState.Dead;
				Instantiate(_wreckagePrefab, transform.position, Quaternion.identity);
				_playerManager.OnPlayerDeath(this, _lastDamageSource);
				_playerShield?.BurstShield();
			}
		}

		/// <summary>
		/// Applies damage to the player based on a given value.
		/// Negative values will heal the player.
		/// </summary>
		/// <param name="value"></param>
		/// <param name="sourceId">The id of the source of the damage</param>
		public void ApplyDamage(float value, ulong sourceId) {
			// Exit if player is not alive
			if (_playerState != PlayerState.Alive) return;
			
			// Make the player shield flare if possible
			if (CurrentArmor > 0f) _playerShield?.FlareShield();
			
			// Set the last damage source
			_lastDamageSource = sourceId;
			
			// Branch depending on the sign of the damage
			if (value > 0f) {
				// Calculate and apply the damage absorbed by the armor
				var armorDamage = _armorDamageRatio * value;
				var armorDiff = CurrentArmor - armorDamage;
				if (CurrentArmor > 0 && CurrentArmor - armorDamage <= 0f) _playerShield?.BurstShield();
				CurrentArmor -= armorDamage;

				// Calculate and apply the remaining damage to health
				CurrentHealth -= value - armorDamage;

				// Apply any damage unable to be absorbed by the armor (armorDamage > _currentArmor)
				if (armorDiff < 0f) {
					CurrentHealth += armorDiff;
				}
				
				// Reset the regen totals
				_armorRegenTotal = 0f;
				_healthRegenTotal = 0f;
				
				// Set the delay timers
				_armorRegenTimer = _armorRegenDelay;
				_healthRegenTimer = _healthRegenDelay;
			}
			else {
				// Heal the player
				CurrentHealth += value;
			}	
		}
		
		/// <summary>
		/// Handles respawn behavior. 
		/// Should only be called from the player manager linked to this entity.
		/// </summary>
		/// <param name="manager"></param>
		public void OnPlayerRespawn(PlayerManager manager) {
			// Exit if this method was somehow called by another manager or the player is already alive
			if (manager != _playerManager || _playerState == PlayerState.Alive) return;
			
			// Reset the shield state
			_playerShield?.RechargeShield();
			
			// Reset stat values and timers
			CurrentArmor = _maxArmor;
			CurrentHealth = _maxArmor;
			_armorRegenTimer = 0f;
			_healthRegenTimer = 0f;
			
			// Zero the rigidbody velocity values
			var rigidBody = GetComponent<Rigidbody>();
			rigidBody.velocity = Vector3.zero;
			rigidBody.angularVelocity = Vector3.zero;
			
			// Set the player state to alive
			_playerState = PlayerState.Alive;
		}
	}
	
	// Player State
	[Serializable]
	public enum PlayerState {
		Alive,
		Dead,
	}
	
	// Player Death
	[Serializable]
	public enum PlayerDeath {
		Hazard, // Getting crushed, explody things, etc
		Suicide, // Falling off the level
		Betrayal, // Killed by an ally
		Enemy // Killed by an enemy
	}
}
