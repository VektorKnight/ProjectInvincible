using System;
using InvincibleEngine.Managers;
using XInputDotNetPure;

namespace InvincibleEngine.DataTypes {
	/// <summary>
	/// Holds all data relevant to a player in-game.
	/// </summary>
	[Serializable]
	public class PlayerMetadata {
		// Global Info
		public PlayerManager Manager; 	// The PlayerManager instance assigned to this player
		public ulong UniqueId;			// The Steam ID of the player (or a generated one if local guest)
		public string DisplayName;		// The display name of the player and/or entity
		public bool IsLocalPlayer;		// True if player is local to the client
		public PlayerIndex LocalIndex;	// The local index of the player used for input
		
		// Round Specific
		public Team Team; 				// The Team that this player is assigned to
		public int Kills;				// Number of kills during the current round
		public int Assists;				// Number of assists during the current round
		public int Betrayals;			// Number of betrayals (team kills) during the current round
		public int Suicides;
		public int Score; 				// Cumulative score for the player during the current round
		
		// Post-Game Stats
		public int TotalKills;			// Total number of kills over an entire game
		public int TotalAssists;		// Total number of assists over an entire game
		public int TotalBetrayals;		// Total number of betrayals over an entire game
		public int TotalSuicides;		// Total number of suicides over an entire game
		public int TotalScore;			// The cumulative score calculated from the previous values
		public float DamageDealt;		// The total amount of damage dealt to enemy players during a match
		public float DamageTaken;		// The total amount of damage taken during a match
		public float Accuracy;			// Percentage of shots fired that hit enemies during a match

        // Network Data
        public int ConnectionId;
        public bool IsMatchHost;        // True if the player is the match host
		
		/// <summary>
		/// Add a thing to the player's score.
		/// </summary>
		/// <param name="type">The type of score thing to add stuff.</param>
		/// <param name="value">The value of the score type thing.</param>
		public void AddScoreValue(ScoreType type, int value) {
			switch (type) {
				case ScoreType.Kill:
					Kills += value;
					Score += value * GameManager.KillValue;
					break;
				case ScoreType.Assist:
					Assists += value;
					Score += value * GameManager.AssistValue;
					break;
				case ScoreType.Betrayal:
					Betrayals += value;
					Score += value * GameManager.BetrayalPenalty;
					break;
				case ScoreType.Suicide:
					Suicides += value;
					Score += GameManager.SuicidePenalty;
					break;
				default:
					return;
			}
		}
		
		/// <summary>
		/// Totals the scores from the previous round and resets the values for a new round.
		/// </summary>
		public void NewScoringRound() {
			// Add current score values to the totals
			TotalKills += Kills;
			TotalAssists += Assists;
			TotalBetrayals += Betrayals;
			TotalSuicides += Suicides;
			TotalScore += Score;
			
			// Reset current scores for a new round
			Kills = 0;
			Assists = 0;
			Betrayals = 0;
			Suicides = 0;
			Score = 0;
		}
    }

	[Serializable]
	public enum ScoreType {
		Kill,
		Assist,
		Betrayal,
		Suicide
	}
}
