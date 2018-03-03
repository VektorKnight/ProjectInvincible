using InvincibleEngine.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace InvincibleEngine.Components.Utility {
	[RequireComponent(typeof(Text))]
	public class DevStats : MonoBehaviour {
		
		// Text Readout
		private const string DISPLAY = "Screen: {0}x{1}\n" +
		                               "FPS: {2}\n" +
		                               "ΔTime: {3:n1}ms\n" +
		                               "Object Pool: {4}/{5}\n" +
		                               "Local Players: {6}";
		private Text _displayText;
		private string _displayString;
		
		
		// FPS Counter
		private const float FPS_PERIOD = 0.5f;
		private int _fpsAccum;
		private float _fpsNextPeriod;
		private int _currentFps;
		

		// Use this for initialization
		private void Start () {
			_displayText = GetComponent<Text>();
			_fpsNextPeriod = Time.realtimeSinceStartup + FPS_PERIOD;
		}
	
		// Update is called once per frame
		private void Update () {
			// Calculate average FPS
			_fpsAccum++;
			if (Time.realtimeSinceStartup > _fpsNextPeriod) {
				_currentFps = (int) (_fpsAccum / FPS_PERIOD);
				_fpsAccum = 0;
				_fpsNextPeriod += FPS_PERIOD;
			}
			
			// Fetch Object Pool Data
			var poolActive = 0;
			var poolTotal = 0;
			foreach (var kvp in GlobalObjectManager.MultiObjectPool.MultiPool) {
				poolActive += kvp.Value.ActiveObjects;
				poolTotal += kvp.Value.TotalObjects;
			}
			
			// Format the readout
			_displayString = string.Format(DISPLAY, 
				Screen.width, 
				Screen.height, 
				_currentFps,
				Time.deltaTime * 1000f,
				poolActive,
				poolTotal,
				GameManager.LocalPlayerCount);
			_displayText.text = _displayString;
		}
	}
}
