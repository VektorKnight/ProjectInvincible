using System;
using System.Collections.Generic;
using InvincibleEngine.CameraSystem;
using InvincibleEngine.UnitFramework.Components;
using InvincibleEngine.UnitFramework.DataTypes;
using InvincibleEngine.UnitFramework.Enums;
using InvincibleEngine.UnitFramework.Interfaces;
using UnityEngine;
using UnityEngine.EventSystems;
using VektorLibrary.AI.Systems;
using VektorLibrary.EntityFramework.Components;

namespace InvincibleEngine.Managers {
    /// <summary>
    /// Handles player control and input to pass to match manager
    /// Also is in charge of displaying previews and other visual cues about
    /// what the player is doing 
    /// 
    /// Only active during match, disabled in lobbies
    /// </summary>
    public class PlayerManager : MonoBehaviour {
        
        // Singleton Instance Accessor
        public static PlayerManager Instance { get; private set; }

        // Private: Unit Selection
        private List<UnitBehavior> _selectedUnits;
        private int _selectionBorderWidth = 2;
        private Texture2D _selectionTexture;
        private Rect _selectionBox;
        private bool _selecting;
        
        // Private: Command Processing
        private UnitCommands _desiredCommand;
        private bool _readyToIssue;

        // Location of mouse on screen
        public Vector2 MousePosition;

        // List of selected Entities

        // Building variables
        public bool BuildMode { get; set; }
        private GameObject _buildPreview;
        
        // Public Static: Selection
        public static List<UnitBehavior> SelectedUnits => Instance._selectedUnits;

        // Preload Method
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Preload() {
            //Make sure the Managers object exists
            var managers = GameObject.Find("Managers") ?? new GameObject("Managers");

            // Ensure this singleton initializes at startup
            if (Instance == null) Instance = managers.GetComponent<PlayerManager>() ?? managers.AddComponent<PlayerManager>();

            // Ensure this singleton does not get destroyed on scene load
            DontDestroyOnLoad(Instance.gameObject);
        }

        /// <summary>
        /// Set variables
        /// </summary>
        private void Awake() {
            // Initialize the selected units list
            _selectedUnits = new List<UnitBehavior>();
            
            // Set selection texture color
            _selectionTexture = new Texture2D(1, 1);
            _selectionTexture.SetPixel(1, 1, Color.white);
            _selectionTexture.wrapMode = TextureWrapMode.Repeat;
            _selectionTexture.Apply();
            
            // Initialize the selection rect
            _selectionBox = new Rect(0f, 0f, 0f, 0f);

            // Instantiate build preview object
            _buildPreview = Instantiate(new GameObject());
        }
        
        // Unity Update
        private void Update() {
            // Set mouse position
            MousePosition = new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y);         
            
            

            // if in build mode, display preview options for player
            if (BuildMode) {

                //Show the build preview at nearby grid points
                _buildPreview.transform.position = MatchManager.Instance.GridSystem.WorldToGridPoint(InvincibleCamera.MouseData.WorldPosition).WorldPosition;

                //Change the render color based on if the node we are hovering over is occupied
                if(!MatchManager.Instance.GridSystem.WorldToGridPoint(InvincibleCamera.MouseData.WorldPosition).IsOpen()) {
                    _buildPreview.GetComponentInChildren<Renderer>().material.color = new Color32(255, 0, 0, 100);
                }
                else {
                    _buildPreview.GetComponentInChildren<Renderer>().material.color = new Color32(255, 255, 255, 100);
                }
                
                //On left click, try and construct the building
                if (Input.GetMouseButtonDown(0)) {

                }
            }
            
            // Execute command routine
            CommandRoutine();
            
            // Execute selection routine
            SelectionRoutine();
        }
        
        // Unit commanding routine
        private void CommandRoutine() {
            // Exit if there are no units selected
            if (_selectedUnits.Count == 0) return;
            
            // Handle commands issued from an external call (Gameplay UI, etc...)
            if (_readyToIssue) {
                if (Input.GetKeyDown(KeyCode.Mouse0)) {
                    // Construct a UnitCommand object based on the desired command
                    var command = new UnitCommand();
                    switch (_desiredCommand) {
                        case UnitCommands.Move:
                            command.Command = UnitCommands.Move;
                            command.Data = InvincibleCamera.MouseData.WorldPosition;
                            break;
                        case UnitCommands.AMove:
                            command.Command = UnitCommands.AMove;
                            command.Data = InvincibleCamera.MouseData.WorldPosition;
                            break;
                        case UnitCommands.Engage:
                            // TODO: Implement logic for this command, for now just have it act like Move
                            command.Command = UnitCommands.Move;
                            command.Data = InvincibleCamera.MouseData.WorldPosition;
                            break;
                        case UnitCommands.Assist:
                            // TODO: Not yet implemented
                            break;
                        case UnitCommands.Patrol:
                            // TODO: Not yet implemented
                            break;
                        case UnitCommands.Hold:
                            // TODO: Not yet implemented
                            break;
                        case UnitCommands.Stop:
                            command.Command = UnitCommands.Stop;
                            break;
                        default:
                            return;
                    }
                    
                    // Dispatch the command to all selected units
                    foreach (var unit in _selectedUnits) {
                        unit.ProcessCommand(command);
                    }
                    
                    // Reset the ready to issue flag
                    _readyToIssue = false;
                }
            }
            
            // Handle commands bound to keys
            if (!_readyToIssue) {
                // Movement Command ([M] or [Mouse1])
                if (Input.GetKeyDown(KeyCode.M) || Input.GetKeyDown(KeyCode.Mouse1)) {
                    foreach (var unit in _selectedUnits) {
                        unit.ProcessCommand(new UnitCommand(UnitCommands.Move, InvincibleCamera.MouseData.WorldPosition));
                    }
                }

                // Stop Command ([S])
                if (Input.GetKeyDown(KeyCode.End)) {
                    foreach (var unit in _selectedUnits) {
                        unit.ProcessCommand(new UnitCommand(UnitCommands.Stop, null));
                    }
                }
            }
        }
        
        // Unit selection routine
        private void SelectionRoutine() {
            // Cancel and/or avoid selecting if the cursor is over a UI element
            if (EventSystem.current.IsPointerOverGameObject()) {
                // Reset selecting flag
                _selecting = false;
                
                // Reset selection rect
                _selectionBox = new Rect(0f, 0f, 0f, 0f);
                
                // Exit this routine
                return;
            }
            
            // Check and see if the player is trying to make a selection
            if (Input.GetKeyDown(KeyCode.Mouse0)) {
                // Set selecting flag to true
                _selecting = true;

                // Set initial values of box
                _selectionBox = new Rect(MousePosition.x, MousePosition.y, 0, 0);
            }

            // As long as it's held make the rectangle 
            if (Input.GetKey(KeyCode.Mouse0) && _selecting) {
                _selectionBox.width = MousePosition.x - _selectionBox.x;
                _selectionBox.height = MousePosition.y - _selectionBox.y;
            }

            // Exit if still the player has not finished selecting
            if (!Input.GetMouseButtonUp(0) || !_selecting) return;
            
            // Reset selecting flag
            _selecting = false;

            // Invoke the OnDeselected callback on all selected entities
            foreach (var behavior in _selectedUnits) {
                behavior.OnDeselected();
            }

            // Clear the list of selected entities
            _selectedUnits.Clear();

            // Check the mouse delta to determine if this was a single click or drag selection
            var mouseDelta = new Vector2(_selectionBox.width, _selectionBox.height);

            // Assume single click if delta is small and select the hovered object if possible
            if (mouseDelta.magnitude < 4f) {
                // Check if the cursor is hovering over an object and determine if it can be selected
                var hoveredObject = InvincibleCamera.MouseData.HoveredObject;
                var selectable = hoveredObject != null ? hoveredObject.GetComponent<UnitBehavior>() : null;

                // Select the hovered object if possible
                if (selectable != null) {
                    _selectedUnits.Add(selectable);
                    selectable.OnSelected();
                }
            }

            // Select on-screen units within the selection box
            foreach (var entity in InvincibleCamera.VisibleObjects) {
                // Skip null entries
                if (entity == null) continue;
                    
                // Cache screen position of object
                var objectPosition = InvincibleCamera.PlayerCamera.WorldToScreenPoint(entity.transform.position);

                // Account for odd screen mapping
                objectPosition.y = Screen.height - objectPosition.y;

                // If the object is within the rect, select it, else continue
                if (!_selectionBox.Contains(objectPosition, true)) continue;

                // Add the object to the list of selected entities
                _selectedUnits.Add(entity);

                // Invoke the OnSelected callback
                entity.OnSelected();
            }

            // Reset the selection rect
            _selectionBox = new Rect(0, 0, 0, 0);
        }
        
        // Called to set the desired command to be executed on next mouse click
        public static void SetDesiredCommand(UnitCommands command) {
            // Exit if we have nothing selected
            if (SelectedUnits.Count == 0) return;
            
            // Set the desired command and ready flag
            Instance._desiredCommand = command;
            Instance._readyToIssue = true;
        }
        
        // Called to try and directly issue a command to selected units
        public static void IssueCommandDirect(UnitCommands command) {
            // Exit if we have nothing selected
            if (SelectedUnits.Count == 0) return;

            // Construct command object based on issued command
            var commandObject = new UnitCommand();
            switch (command) {
                case UnitCommands.Stop:
                    commandObject.Command = UnitCommands.Stop;
                    break;
                default:
                    return;
            }
            
            // Dispatch the command to all selected units
            foreach (var unit in SelectedUnits) {
                unit.ProcessCommand(commandObject);
            }
        }
        
        // Called when the player wants to start building something, IE. generate preview object
        public void OnBuildRequest(EntityBehavior building) {

            //Set build preview
            _buildPreview = GenerateEmptyObject(building.gameObject);

            //Activate Build Mode
            BuildMode = true;
        }

        //Creates an empty object with no monobehaviors for preview
        public GameObject GenerateEmptyObject(GameObject source) {

            //instantiate object
            GameObject g = Instantiate(source);

            //Remove all behavior
            foreach(MonoBehaviour n in g.GetComponentsInChildren<MonoBehaviour>()) {
                Destroy(n);
            }
        
            //Activate and return
            g.SetActive(true);
            return g;
        }

        /// <summary>
        /// 
        /// </summary>
        private void OnGUI() {

            //If player is selecting draw rectangle
            if (_selecting) {
                GUI.DrawTexture(new Rect(_selectionBox.x, _selectionBox.y, _selectionBox.width, _selectionBorderWidth), _selectionTexture); //TL-TR
                GUI.DrawTexture(new Rect(_selectionBox.x + _selectionBox.width, _selectionBox.y, _selectionBorderWidth, _selectionBox.height), _selectionTexture); //TR-BR
                GUI.DrawTexture(new Rect(_selectionBox.x, _selectionBox.y + _selectionBox.height, _selectionBox.width, _selectionBorderWidth), _selectionTexture); //BL-BR
                GUI.DrawTexture(new Rect(_selectionBox.x, _selectionBox.y, _selectionBorderWidth, _selectionBox.height), _selectionTexture); //TL-BL
            }
        }
    }
}
