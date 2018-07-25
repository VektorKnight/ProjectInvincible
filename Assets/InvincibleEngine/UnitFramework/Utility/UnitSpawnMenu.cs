using System;
using InvincibleEngine.CameraSystem;
using InvincibleEngine.UnitFramework.Components;
using InvincibleEngine.UnitFramework.Enums;
using UnityEngine;
using UnityEngine.UI;

namespace InvincibleEngine.UnitFramework.Utility {
    public class UnitSpawnMenu : MonoBehaviour {
        // Unity Inspector
        [Header("Required Objects")] 
        [SerializeField] private Dropdown _unitDropdown;
        [SerializeField] private Dropdown _unitTeam;
        [SerializeField] private InputField _countField;
        
        // Private: Spawnable Units
        private UnitBehavior[] _spawnableUnits;
        
        // Private: Current Settings
        private int _selectedUnit;
        private Team _selectedTeam;
        private int _spawnCount;
        
        // Private: State
        private bool _readyToSpawn;
        
        // Initialization
        private void Awake() {
            // Load all spawnable units from the Resources folder
            _spawnableUnits = Resources.LoadAll<UnitBehavior>("");
            
            // Initialize the spawnable units dropdown
            foreach (var unit in _spawnableUnits) {
                _unitDropdown.options.Add(new Dropdown.OptionData(unit.gameObject.name));
            }
            
            // Initialize the team dropdown
            foreach (var team in Enum.GetNames(typeof(Team))) {
                _unitTeam.options.Add(new Dropdown.OptionData(team));
            }
        }
        
        // Callback for the selection dropdown
        public void OnUnitChanged(int index) {
            _selectedUnit = index;
        }
        
        // Callback for team dropdown
        public void OnTeamChanged(int value) {
            _selectedTeam = (Team) value;
        }
        
        // Callback for the spawn count field
        public void OnCountChanged(int value) {
            _spawnCount = value;
        }
        
        // Callback for the spawn unit button
        public void OnSpawnReady() {
            
        }
        
        // Method for spawning a unit based on current settings
        public void SpawnUnit() {
            var unit = Instantiate(_spawnableUnits[_selectedUnit], InvincibleCamera.MouseData.WorldPosition, Quaternion.identity);
        }
    }
}