using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using VektorLibrary.AI.Systems;

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
        private bool _selecting = false;
        private Texture2D _selectionTexture;
        private const int SELECTION_BORDER_WIDTH = 2;
        private Rect _selectionBox = new Rect(0, 0, 0, 0);

        // Location of mouse on screen
        public Vector2 MousePosition;
        public Vector3 MousePoint;

        // List of selected Entities
        public List<EntityBehavior> SelectedEntities = new List<EntityBehavior>();

        // Building variables
        public bool BuildMode { get; set; }
        private GameObject _buildPreview;
        
        // State Machine
        private StackFSM<float> _stateMachine;

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
            
            // Initialize the state machine
            _stateMachine = new StackFSM<float>(SelectionState);
        }
        
        // Default state of the manager (Selection)
        private void SelectionState(float deltaTime) {
            if (EventSystem.current.IsPointerOverGameObject() && !_selecting) return;
            //Check and see if the player is trying to make a selection
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

                // Toggle selecting
                _selecting = false;

                // Deselect all objects
                foreach (EntityBehavior n in SelectedEntities) {
                    n.OnDeselected();
                }
                SelectedEntities.Clear();

                // EXTREMELY bad way of selecting, change later
                foreach (var entity in EntityManager.Instance._behaviors) {
                    // Workaround: Skip null objects
                    if (entity == null) continue;
                    
                    // Cache obejct position
                    var objectPosition = Camera.main.WorldToScreenPoint(entity.transform.position);

                    // account for weird mapping
                    objectPosition.y = Screen.height - objectPosition.y;

                    // Check if within selection and call on select if possible
                    if (!_selectionBox.Contains(objectPosition)) continue;
                    SelectedEntities.Add(entity);
                    entity.OnSelected();
                }

                _selectionBox = new Rect(0, 0, 0, 0);
            }
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
        
        // Commanding state of the manager
        private void CommandingState(float deltaTime) {
            
        }

        private void Update() {
            //TODO: TEMPORARY SELECTION INDICATIONS

            //Set mouse position
            MousePosition = new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y);

            //Determine where the mouse cursor is hovering
            RaycastHit hit;
            if(Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 1024, 1 << 8)) {
                MousePoint = hit.point;
            }
            
            // Update the state machine
            _stateMachine.Update(Time.deltaTime);
        }

        //Called when the player attempts to build somthing
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
