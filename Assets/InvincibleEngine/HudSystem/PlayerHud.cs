using System.Collections.Generic;
using InvincibleEngine.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace InvincibleEngine.HudSystem {
	public class PlayerHud : MonoBehaviour {
		
		// Unity Inspector
		[Header("Hud Elements")]
		[SerializeField] private Image _weaponReticle;
		[SerializeField] private Image _armorBar;
		[SerializeField] private Image _healthBar;
		[SerializeField] private Text _scoreText;
		[SerializeField] private Image _respawnPanel;
		[SerializeField] private VerticalLayoutGroup _killFeed;
		[SerializeField] private Text _killTextPrefab;

		[Header("Color Settings")] 
		[SerializeField] private Gradient _healthGradient;
		
		// Private: Respawn Panel
		private bool _showRespawnPanel;
		private Text _respawnText;
		private float _respawnTimer;
		
		// Private: Customn Weapon Huds
		private List<GameObject> _weaponHuds = new List<GameObject>();

		// Public Read/Write: Stat Values
		public float ArmorValue { get; set; }
		public float HealthValue { get; set; }
		public int ScoreValue { get; set; }
		
		// Initialization
		private void Start() {
			_respawnText = _respawnPanel.GetComponentInChildren<Text>();
		}
	
		// Update is called once per frame
		private void Update () {
			// Update the stat readouts
			_armorBar.fillAmount = ArmorValue;
			_healthBar.fillAmount = HealthValue;
			_healthBar.color = _healthGradient.Evaluate(HealthValue);
			
			// Update the respawn timer if necessary
			if (_showRespawnPanel) {
				if (_respawnTimer > 0f) {
					_respawnTimer -= Time.deltaTime;
					_respawnText.text = $"Respawning in {_respawnTimer:n1}";
				}
				else {
					_showRespawnPanel = false;
					_respawnPanel.gameObject.SetActive(false);
				}
			}
		}
		
		// Swap the aiming reticle
		public void SwapReticle(Sprite reticle) {
			_weaponReticle.sprite = reticle;
			_weaponReticle.rectTransform.sizeDelta = reticle.rect.size;
		}
		
		// Add a custom weapon hud
		public void AddWeaponHud(GameObject hudObject) {
			
		}
		
		// Remove a custom weapon hud
		public void RemoveWeaponHud(GameObject hudObject) {
			
		}
		
		// Show respawn timer
		public void ShowRespawnTimer() {
			_showRespawnPanel = true;
			_respawnPanel.gameObject.SetActive(true);
			_respawnTimer = GameManager.RespawnDelay;
		}
		
		// Update the score text
		public void UpdateScoreDisplay(int value) {
			_scoreText.text = $"Score: {value}";
		}
		
		// Push a new kill indicator to the kill feed
		public void UpdateKillFeed(string text) {
			var killText = Instantiate(_killTextPrefab, _killFeed.transform, false);
			killText.GetComponent<Text>().text = text;
		}
	}
}
