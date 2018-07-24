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

        // Selection Variables
        private const int SELECTION_BORDER_WIDTH = 2;
        private bool _selecting;
        private Texture2D _selectionTexture;
        private Rect _selectionBox = new Rect(0, 0, 0, 0);

        // Location of mouse on screen
        public Vector2 MousePosition;

        // List of selected Entities
        public List<UnitBehavior> SelectedEntities = new List<UnitBehavior>();

        // Building variables
        public bool BuildMode { get; set; }
        private GameObject _buildPreview;

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
            // Set selection texture color
            _selectionTexture = new Texture2D(1, 1);
            _selectionTexture.SetPixel(1, 1, Color.white);
            _selectionTexture.wrapMode = TextureWrapMode.Repeat;
            _selectionTexture.Apply();

            // instantiate preview object
            _buildPreview = Instantiate(new GameObject());
        }
        
        // Building state of the manager
        private void BuildingState(float deltaTime) {
            // Set build preview to mouse point
            //:: REMOVED WHILE GRID SYSTEM IS MOVED:: _buildPreview.transform.position = MatchManager.Instance.GridSystem.WorldToGridPoint(MousePoint);

            // Attempt build when released
            if (!Input.GetMouseButtonDown(0)) return;
            
            // Destroy Preview 
            Destroy(_buildPreview);

            // Cancel build mode
            BuildMode = false;
        }

        private void Update() {
            // Set mouse position
            MousePosition = new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y);         
            
            // Handle commands ()
            if (SelectedEntities.Count > 0) {
                // Movement Command ([Q] or [Mouse1])
                if (Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.Mouse1)) {
                    foreach (var unit in SelectedEntities) {
                        unit.ProcessCommand(new UnitCommand(UnitActions.Move, InvincibleCamera.MouseData.WorldPosition));
                    }
                }
                
                // Stop Command ([S])
                if (Input.GetKeyDown(KeyCode.End)) {
                    foreach (var unit in SelectedEntities) {
                        unit.ProcessCommand(new UnitCommand(UnitActions.Stop, null));
                    }
                }
            }
            
            // Check and see if the player is trying to make a selection
            if (Input.GetMouseButtonDown(0)) {

                // Set selecting to true
                _selecting = true;

                // Set initial values of box
                _selectionBox = new Rect(MousePosition.x, MousePosition.y, 0, 0);
            }

            // As long as it's held make the rectangle 
            if (Input.GetMouseButton(0) && _selecting) {
                _selectionBox.width = MousePosition.x - _selectionBox.x;
                _selectionBox.height = MousePosition.y - _selectionBox.y;
            }

            // On mouse up stop selecting
            if (Input.GetMouseButtonUp(0) && _selecting) {

                // Toggle selecting bool
                _selecting = false;

                // Invoke the OnDeselected callback on all entities
                foreach (var behavior in SelectedEntities) {
                    behavior.OnDeselected();
                }
                
                // Clear the list of selected entities
                SelectedEntities.Clear();
                
                // Check the mouse delta to determine if this was a single click or drag selection
                var mouseDelta = new Vector2(_selectionBox.width, _selectionBox.height);
                
                // Assume single click if delta is small and select the hovered object if possible
                if (mouseDelta.magnitude < 4f) {
                    // Check if the cursor is hovering over an object and determine if it can be selected
                    var hoveredObject = InvincibleCamera.MouseData.HoveredObject;
                    var selectable = hoveredObject != null ? hoveredObject.GetComponent<UnitBehavior>() : null;
                    
                    // Select the hovered object if possible
                    if (selectable != null) {
                        SelectedEntities.Add(selectable);
                        selectable.OnSelected();
                    }
                }

                // Optimized selection using on-screen objects
                foreach (var entity in InvincibleCamera.VisibleObjects) {                 
                    // Cache screen position of object
                    var objectPosition = InvincibleCamera.PlayerCamera.WorldToScreenPoint(entity.transform.position);

                    // Account for odd screen mapping
                    objectPosition.y = Screen.height - objectPosition.y;

                    // If the object is within the rect, select it, else continue
                    if (!_selectionBox.Contains(objectPosition, true)) continue;
                    
                    // Add the object to the list of selected entities
                    SelectedEntities.Add(entity);
                    
                    // Invoke the OnSelected callback
                    entity.OnSelected();
                }
                
                // Reset the selection rect
                _selectionBox = new Rect(0, 0, 0, 0);
            }
        }

        // Called when the player attempts to build somthing
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
                GUI.DrawTexture(new Rect(_selectionBox.x, _selectionBox.y, _selectionBox.width, SELECTION_BORDER_WIDTH), _selectionTexture); //TL-TR
                GUI.DrawTexture(new Rect(_selectionBox.x + _selectionBox.width, _selectionBox.y, SELECTION_BORDER_WIDTH, _selectionBox.height), _selectionTexture); //TR-BR
                GUI.DrawTexture(new Rect(_selectionBox.x, _selectionBox.y + _selectionBox.height, _selectionBox.width, SELECTION_BORDER_WIDTH), _selectionTexture); //BL-BR
                GUI.DrawTexture(new Rect(_selectionBox.x, _selectionBox.y, SELECTION_BORDER_WIDTH, _selectionBox.height), _selectionTexture); //TL-BL
            }
        }
    }
}
