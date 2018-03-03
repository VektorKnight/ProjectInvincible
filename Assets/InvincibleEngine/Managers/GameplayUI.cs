using UnityEngine;
using UnityEngine.SceneManagement;

namespace InvincibleEngine.Managers {
	/// <summary>
	/// Handles the root menus and HUD elements present in gameplay
	/// </summary>
	public class GameplayUI : MonoBehaviour {
		
		//Singleton Instance
		public static GameplayUI Instance { get; private set; }
		
		// Unity Inspector
		[Header("Menu Objects")]
		[SerializeField] private GameObject _menuPause;
		
		// UI Events
		public delegate void MenuToggledEventHandler();
		public event MenuToggledEventHandler OnMenuToggled;

		// Initialization
		private void Awake () {
			//Enforce Singleton Instance
			if (Instance == null) { Instance = this; }
			else if (Instance != this) { Destroy(gameObject); }
			
			// Ensure the in-game menu is disabled on load
			if (_menuPause.activeSelf) _menuPause.SetActive(false);
		}
		
		// Toggle the in-game menu
		public void ToggleMenu() {
			OnMenuToggled?.Invoke();
			_menuPause.SetActive(!_menuPause.activeSelf);
		}

		// TEMP: Reload the current scene (runtime only, ignored in editor)
		public void ReloadScene() {
			Debug.LogWarning("Scene reloading not supported in editor, use the editor controls instead.");
			if (Application.isEditor) return;
			SceneManager.LoadScene(SceneManager.GetActiveScene().name);
		}
		
		// TEMP: Exit the current game (runtime only, ignored in editor)
		public void ExitGame() {
			Debug.LogWarning("Application exiting not supported in editor, use the editor controls instead.");
			if (Application.isEditor) return;
			Application.Quit();
		}
	
		// Update is called once per frame
		private void Update () {
			// Handle Pause Menu
			if (Input.GetKeyDown(KeyCode.Escape)) {
				ToggleMenu();
			}
		}
	}
}
