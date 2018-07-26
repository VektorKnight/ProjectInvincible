﻿using System;
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
        
        // Private: Spawnable Units
        private UnitBehavior[] _spawnableUnits;
        
        // Private: Current Settings
        private int _selectedUnit;
        private Team _selectedTeam;
        private int _spawnCount = 1;
        
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
        
        // Unity Update
        private void Update() {
            // Exit if we are not ready to spawn a unit
            if (!_readyToSpawn) return;
            
            // Spawn a unit at the cursor when the user clicks
            if (Input.GetKeyDown(KeyCode.Mouse0))
                SpawnUnit();
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
        public void OnCountChanged(string raw) {
            int.TryParse(raw, out _spawnCount);
        }
        
        // Callback for the spawn unit button
        public void OnSpawnClicked() {
            _readyToSpawn = true;
        }
        
        // Method for spawning a unit based on current settings
        public void SpawnUnit() {
            for (var i = 0; i < _spawnCount; i++) {
                var unit = Instantiate(_spawnableUnits[_selectedUnit], InvincibleCamera.MouseData.WorldPosition, Quaternion.identity);
                unit.SetTeam(_selectedTeam);
            }
            _readyToSpawn = false;
        }
    }
}