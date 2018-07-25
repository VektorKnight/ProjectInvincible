using System;
using InvincibleEngine.CameraSystem;
using InvincibleEngine.Managers;
using InvincibleEngine.UnitFramework.Enums;
using UnityEngine;
using UnityEngine.UI;

namespace InvincibleEngine.UnitFramework.Utility {
    public class UnitToolsMenu : MonoBehaviour {
        // Unity Inspector
        [Header("Required Objects")] 
        [SerializeField] private Dropdown _teamDropdown;
        
        // Private: Current Settings
        private Team _selectedTeam;
        
        // Private: State
        private bool _readyToTeleport;
        
        // Initialization
        private void Awake() {
            // Initialize the team dropdown
            foreach (var team in Enum.GetNames(typeof(Team))) {
                _teamDropdown.options.Add(new Dropdown.OptionData(team));
            }
        }
        
        // Unity Update
        private void Update() {
            // Exit if we are not ready to teleport
            if (!_readyToTeleport) return;
            
            // Call teleport units on mouse click
            if (Input.GetKeyDown(KeyCode.Mouse0))
                TeleportUnits();
        }
        
        // Handler for the team dropdown
        public void OnTeamChanged(int value) {
            _selectedTeam = (Team) value;
        }
        
        // Handler for the change team button
        public void OnChangeTeamClicked() {
            var selectedUnits = PlayerManager.SelectedUnits;
            foreach (var unit in selectedUnits) {
                unit.SetTeam(_selectedTeam);
            }
        }
        
        // Handler for the teleport button
        public void OnTeleportClicked() {
            _readyToTeleport = true;
        }
        
        // Handler for the reset health button
        public void OnResetHealthClicked() {
            
        }
        
        // Handler for the destroy button
        public void OnDestroyClicked() {
            var selectedUnits = PlayerManager.SelectedUnits;
            foreach (var unit in selectedUnits) {
                Destroy(unit);
            }
        }
        
        // Method for teleporting selected units to the new position
        private void TeleportUnits() {
            var selectedUnits = PlayerManager.SelectedUnits;
            foreach (var unit in selectedUnits) {
                unit.transform.position = InvincibleCamera.MouseData.WorldPosition;
            }

            _readyToTeleport = false;
        }
    }
}