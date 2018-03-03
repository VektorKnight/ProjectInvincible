using System;
using System.Collections.Generic;
using InvincibleEngine.Components.Generic;
using InvincibleEngine.DataTypes;
using UnityEngine;
using XInputDotNetPure;

namespace InvincibleEngine.Managers {
	/// <summary>
	/// Manages the gameplay loop for matches.
	/// </summary>
	public class GameManager : MonoBehaviour {
		
		// Singeton Instance
		private static GameManager _singleton;

		public static GameManager Instance => _singleton ?? new GameObject("GameManager").AddComponent<GameManager>();
		
		// Unity Inspector
		[Header("Player Prefab")] 
		[SerializeField] private PlayerManager _playerPrefab;

		[Header("Match Settings: Players")] 
		[SerializeField] [Range(1, 4)] private int _localPlayerCount;
		[SerializeField] private int _networkPlayerCount;
		[SerializeField] private Material[] _teamColors = new Material[8];

		[Header("Match Settings: Game")] 
		[SerializeField] private int _numRounds = 1;			// Number of rounds in a game
		[SerializeField] private int _startDelay = 5; 			// Seconds
		[SerializeField] private int _timeLimit = 10; 			// Minutes
		[SerializeField] private int _respawnDelay = 5; 		// Seconds
		
		[Header("Match Settings: Scoring")] 
		[SerializeField] private int _scoreToWin = 1000;		// Wining score
		[SerializeField] private int _killValue = 100;			// Value of a kill
		[SerializeField] private int _assistValue = 50;			// Value of an assist
		[SerializeField] private int _betrayalPenalty = -200;	// Penalty for betrayal
		[SerializeField] private int _suicidePenalty = -75;		// Penalty for suicide
		
		// Private: Player IDs and references
		private readonly Dictionary<ulong, PlayerMetadata> _playerMetadata = new Dictionary<ulong, PlayerMetadata>();
		
		// Private: Level Spawns
		private readonly Dictionary<Team, List<SpawnPoint>> _spawnPoints = new Dictionary<Team, List<SpawnPoint>>();
		
		//TODO: Temporary fully random spawns
		private readonly Queue<SpawnPoint> _openSpawns = new Queue<SpawnPoint>();
		
		// Private: Match State
		private MatchState _matchState = MatchState.NotStarted;
		private float _timeRemaining;
		
		// Public Read/Write
		public static int LocalPlayerCount {
			get { return Instance._localPlayerCount; }
			set { Instance._localPlayerCount = Mathf.Clamp(value, 1, 4); }
		}
		
		public static int NetworkPlayerCount => Instance._networkPlayerCount;
		
		// Public Readonly
		public static int ScoreToWin => Instance._scoreToWin;
		public static int KillValue => Instance._killValue;
		public static int AssistValue => Instance._assistValue;
		public static int BetrayalPenalty => Instance._betrayalPenalty;
		public static int SuicidePenalty => Instance._suicidePenalty;

		public static int TimeLimit => Instance._timeLimit;
		public static int RespawnDelay => Instance._respawnDelay;
		public static float TimeRemaining => Instance._timeRemaining;
		public static Material[] TeamColors => Instance._teamColors;
		
		// Public Delegates
		public delegate void MatchStartEvent();
		public delegate void MatchEndEvent();
		
		public static event MatchStartEvent OnMatchStart;
		public static event MatchEndEvent OnMatchEnd;

		// Initialization
		private void Awake () {
			// Enforce Singleton Instance
			if (_singleton == null) { _singleton = this; }
			else if (_singleton != this) { Destroy(gameObject); }
			
			// Ensure this manager is not destroyed on scene load
			DontDestroyOnLoad(gameObject);

			// Auto Start a Match
			Invoke(nameof(StartMatchWrapper), _startDelay);
		}
		
		//TODO: Temporary fix for calling static function StartMatch from Invoke()
		private void StartMatchWrapper() {
			StartMatch();
		}
	
		// Start a Match
		public static void StartMatch() {
			// Exit if a match is already in progress
			if (Instance._matchState != MatchState.NotStarted) return;
			Debug.Log($"<b><color=purple>GameManager:</color></b> Spawning in players...");
			
			// Disable the level preview camera
			var previewCam = GameObject.FindWithTag("MainCamera");
			if (previewCam != null) previewCam.SetActive(false);
			
			// Find all available spawn points
			FindSpawnPoints();
			
			// Spawn in the local guest players
			InitializePlayers();
			
			// Set the match timer
			Instance._timeRemaining = Instance._timeLimit;
			
			// Invoke the OnMatchStart event
			OnMatchStart?.Invoke();
			
			// Set the match state to Playing
			Debug.Log($"<b><color=purple>GameManager:</color></b> Match started successfully!");
			Instance._matchState = MatchState.Playing;
		}
		
		// Unity Update
		private void Update() {
			if (_matchState == MatchState.Playing) {
				_timeRemaining -= Time.deltaTime;
			}
		}
		
		/// <summary>
		/// Internal function: Finds all spawn points in a level and adds them to a dictionary.
		/// Should be called before spawning players.
		/// </summary>
		private static void FindSpawnPoints() {
			// Find all available spawn points
			var spawnObjects = GameObject.FindGameObjectsWithTag("PlayerSpawn");
			foreach (var spawnObject in spawnObjects) {
				// Reference the spawn point component
				var spawnPoint = spawnObject.GetComponent<SpawnPoint>();
				
				//TODO: Random spawns
				Instance._openSpawns.Enqueue(spawnPoint);
				// Add a new entry for the spawn points team if necessary
				//if (!Instance._spawnPoints.ContainsKey(spawnPoint.Team)) Instance._spawnPoints.Add(spawnPoint.Team, new List<SpawnPoint>());

				// Add the spawn point to the dictionary
				//Instance._spawnPoints[spawnPoint.Team].Add(spawnPoint);
			}
		}
		
		// Get a spawn point
		public static SpawnPoint GetSpawnPoint(Team team) {
			// Return null if there are no spawns available for the specified team
			//if (!Instance._spawnPoints.ContainsKey(team)) return null;
			//return Instance._spawnPoints[team].Count == 0 ? null : Instance._spawnPoints[team][0];
			//var spawns = (from kvp in Instance._spawnPoints where kvp.Key == team || kvp.Key == Team.None from sp in kvp.Value select sp).ToList();
			return Instance._openSpawns.Dequeue();
		}
		
		// Return a spawn point
		public static void ReturnSpawnPoint(SpawnPoint spawnPoint) {
			Instance._openSpawns.Enqueue(spawnPoint);
		}
		
		/// <summary>
		/// Internal function: Spawns in participating players.
		/// Should be called once spawn points are collected and the match is ready to start.
		/// TODO: This function needs to be heavily modified once HexNet and the main source code are fully merged.
		/// </summary>
		private static void InitializePlayers() {
			// Spawn in the main player using Steam profile info
			var playerManager = Instantiate(Instance._playerPrefab, Vector3.zero, Quaternion.identity);
			var uniqueId = DataManager.MainProfile != null ? DataManager.MainProfile.UniqueId : GlobalObjectManager.GetUniqueId();
			var displayName = DataManager.MainProfile != null ? DataManager.MainProfile.DisplayName : "Player Zero";
			playerManager.Initialize(uniqueId, 0, (Team) 1);
			
			// Configure the player metadata for the main profile
			//TODO: This needs to pull data from the DataManager based on participating players
			var metaData = new PlayerMetadata {
				DisplayName = displayName,
				IsLocalPlayer = true,
				LocalIndex = 0,
				Manager = playerManager,
				Team = 0,
				UniqueId = uniqueId
			};
			
			// Register the player with the metadata dictionary
			Instance._playerMetadata.Add(uniqueId, metaData);
			
			// Spawn in the remaining local guest players
			for (var i = 1; i < Instance._localPlayerCount; i++) {
				// Spawn in the player manager
				playerManager = Instantiate(Instance._playerPrefab, Vector3.zero, Quaternion.identity);
				uniqueId = GlobalObjectManager.GetUniqueId();
				playerManager.Initialize(uniqueId, (PlayerIndex) i, (Team) i + 1);
				
				// Configure the player metadata
				//TODO: This needs to pull data from the DataManager based on participating players
				metaData = new PlayerMetadata {
					DisplayName = $"Guest {i + 1}",
					IsLocalPlayer = true,
					LocalIndex = (PlayerIndex) i,
					Manager = playerManager,
					Team = (Team) i + 1,
					UniqueId = uniqueId
				};

				// Register the player with the metadata dictionary
				Instance._playerMetadata.Add(uniqueId, metaData);
			}
		}

		/// <summary>
		/// Called by a Player Manager when its attached player has died
		/// </summary>
		/// <param name="victimId">The ID of the player that has died.</param>
		/// <param name="killerId">The ID of the entity responsible, zero if unknown.</param>
		public static void OnPlayerDeath(ulong victimId, ulong killerId) {
			// Attempt to grab the data for each player
			var victimData = Instance._playerMetadata.ContainsKey(victimId) ? Instance._playerMetadata[victimId] : null;
			var killerData = Instance._playerMetadata.ContainsKey(killerId) ? Instance._playerMetadata[killerId] : null;
			
			// Attempt to grab the names of each player
			var victimName = victimData != null ? victimData.DisplayName : "-NameError-";
			var killerName = killerData != null ? killerData.DisplayName : "-NameError-";
			
			// If killer is null, default to suicide for now
			if (killerData == null) {
				// Update all kill feeds
				foreach (var playerMeta in Instance._playerMetadata) {
					playerMeta.Value.Manager.HudSystem.UpdateKillFeed($"{victimName} committed suicide!");
				}
				
				// Update the victims score
				victimData?.AddScoreValue(ScoreType.Suicide, 1);
				victimData?.Manager.HudSystem.UpdateScoreDisplay(victimData.Score);
				
				// We're done here
				return;
			}
			
			// If killer is not null, check for equal teams (betrayal)
			if (victimData?.Team == killerData.Team) {
				foreach (var playerMeta in Instance._playerMetadata) {
					playerMeta.Value.Manager.HudSystem.UpdateKillFeed($"{killerName} betrayed {victimName}!");
				}
				
				// Update the killers score
				killerData.AddScoreValue(ScoreType.Betrayal, 1);
				killerData.Manager.HudSystem.UpdateScoreDisplay(killerData.Score);
				
				// We're done here
				return;
			}
			
			// Teams are not equal, update all kill feeds and scores
			foreach (var playerMeta in Instance._playerMetadata) {
				playerMeta.Value.Manager.HudSystem.UpdateKillFeed($"{killerName} annihilated {victimName}!");
			}
			
			killerData.AddScoreValue(ScoreType.Kill, 1);
			killerData.Manager.HudSystem.UpdateScoreDisplay(killerData.Score);
		}
	}
	
	// Match State
	[Serializable]
	public enum MatchState {
		NotStarted,
		Starting,
		Playing,
		Ending
	}
	
	// Player Team
	public enum Team {
		None,
		One,
		Two,
		Three,
		Four,
		Five,
		Six,
		Seven,
		Eight
	}
}
