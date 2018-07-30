using InvincibleEngine.Managers;
using InvincibleEngine.UnitFramework.Enums;
using UnityEngine;

namespace InvincibleEngine.UnitFramework.Components {
    /// <summary>
    /// Handles callbacks from the Unit Commands UI element and relays them to
    /// the Player Manager
    /// </summary>
    public class UnitCommandsPanel : MonoBehaviour {
        // Move command handler
        public void OnMoveClicked() {
            var test = Resources.Load<Texture2D>("Textures/Cursors/Cursor_Move");
            Debug.Log(test.name);
            Cursor.SetCursor(test, Vector2.one * 20f, CursorMode.Auto);
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