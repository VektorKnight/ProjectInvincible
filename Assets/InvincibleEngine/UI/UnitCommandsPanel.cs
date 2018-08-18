using InvincibleEngine.Managers;
using InvincibleEngine.UnitFramework.Enums;
using UnityEngine;

namespace InvincibleEngine.UI {
    /// <summary>
    /// Handles callbacks from the Unit Commands UI element and relays them to
    /// the Player Manager
    /// </summary>
    public class UnitCommandsPanel : MonoBehaviour {
        // Move command handler
        public void OnMoveClicked() {
            PlayerManager.SetDesiredCommand(UnitCommands.Move);
        }
        
        // Engage command handler
        public void OnAttackClicked() {
            PlayerManager.SetDesiredCommand(UnitCommands.Engage);
        }
        
        // Stop command handler
        public void OnStopClicked() {
            PlayerManager.IssueCommandDirect(UnitCommands.Stop);
        }
    }
}