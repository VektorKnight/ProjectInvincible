using System.Collections;
using System.Collections.Generic;
using InvincibleEngine;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Handles player control and input to pass to match manager
/// Also is in charge of displaying previews and other visual cues about
/// what the player is doing 
/// 
/// Only active during match, disabled in lobbies
/// </summary>
public class PlayerManager : MonoBehaviour {

    //Selection variables
    private bool Selecting = false;
    private Texture2D SelectionTexture;
    private readonly int SelectionBorderWidth = 2;

    //Selection box values
    private Rect SelectionBox = new Rect(0, 0, 0, 0);
    Vector2 objectPosition = new Vector2();

    //Location of mouse on screen
    public Vector2 MousePotition = new Vector2();
    public Vector3 MousePoint = new Vector3();

    // Singleton Instance Accessor
    public static PlayerManager Instance { get; private set; }

    //List of selected Entities
    public List<EntityBehavior> SelectedEntities = new List<EntityBehavior>();

    //Building variables
    private bool _BuildMode = false;
    public bool BuildMode {
        get {
            return _BuildMode;
        }
        set {
            _BuildMode = value;            
        }
    }
    private GameObject BuildPreview;

    // Preload Method
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Preload() {
        //Make sure the Managers object exists
        GameObject Managers = GameObject.Find("Managers") ?? new GameObject("Managers");

        // Ensure this singleton initializes at startup
        if (Instance == null) Instance = Managers.GetComponent<PlayerManager>() ?? Managers.AddComponent<PlayerManager>();

        // Ensure this singleton does not get destroyed on scene load
        DontDestroyOnLoad(Instance.gameObject);
    }

    /// <summary>
    /// Set variables
    /// </summary>
    private void Awake() {

        //Set selection texture color
        SelectionTexture = new Texture2D(1, 1);
        SelectionTexture.SetPixel(1, 1, Color.white);
        SelectionTexture.wrapMode = TextureWrapMode.Repeat;
        SelectionTexture.Apply();

        //instantiate preview object
        BuildPreview = Instantiate(new GameObject());
       
    }

    private void Update() {
        //TODO: TEMPORARY SELECTION INDICATIONS

        //Set mouse position
        MousePotition = new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y);

        //Determine where the mouse cursor is hovering
        RaycastHit hit;
        if(Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 1024, 1 << 8)) {
            MousePoint = hit.point;
        }

        //SELECTION MODE
        if ((!EventSystem.current.IsPointerOverGameObject() || Selecting) && !BuildMode) {

            //Check and see if the player is trying to make a selection
            if (Input.GetMouseButtonDown(0)) {

                //Set selecting to true
                Selecting = true;

                //Set initial values of box
                SelectionBox = new Rect(MousePotition.x, MousePotition.y, 0, 0);
            }

            //As long as it's held make the rectangle 
            if (Input.GetMouseButton(0) && Selecting) {
                SelectionBox.width = MousePotition.x - SelectionBox.x;
                SelectionBox.height = MousePotition.y - SelectionBox.y;
            }

            //On mouse up stop selecting
            if (Input.GetMouseButtonUp(0) && Selecting) {

                //Toggle selecting
                Selecting = false;

                //Deselect all objects
                foreach (EntityBehavior n in SelectedEntities) {
                    n.OnDeselected();
                }
                SelectedEntities.Clear();

                //EXTREMELY bad way of selecting, change later
                foreach (EntityBehavior n in EntityManager.Instance._behaviors) {

                    //Cache obejct position
                    objectPosition = Camera.main.WorldToScreenPoint(n.transform.position);

                    //account for weird mapping
                    objectPosition.y = Screen.height - objectPosition.y;

                    //Check if within selection and call on select if possible
                    if (SelectionBox.Contains(objectPosition)) {
                        SelectedEntities.Add(n);
                        n.OnSelected();
                    }
                }

                SelectionBox = new Rect(0, 0, 0, 0);
            }
        }

        //BUILD MODE
        if(BuildMode) {

            //Set build preview to mouse point
            BuildPreview.transform.position = MatchManager.Instance.GridSystem.WorldToGridPoint(MousePoint);

            //Attempt build when released
            if(Input.GetMouseButtonDown(0)) {

                //Destroy Preview 
                Destroy(BuildPreview);

                //cancel build mode
                BuildMode = false;
            }
        }
    }

    //Called when the player attempts to build somthing
    public void OnBuildRequest(EntityBehavior building) {

        //Set build preview
        BuildPreview = GenerateEmptyObject(building.gameObject);

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
        if (Selecting) {
            GUI.DrawTexture(new Rect(SelectionBox.x, SelectionBox.y, SelectionBox.width, SelectionBorderWidth), SelectionTexture); //TL-TR
            GUI.DrawTexture(new Rect(SelectionBox.x + SelectionBox.width, SelectionBox.y, SelectionBorderWidth, SelectionBox.height), SelectionTexture); //TR-BR
            GUI.DrawTexture(new Rect(SelectionBox.x, SelectionBox.y + SelectionBox.height, SelectionBox.width, SelectionBorderWidth), SelectionTexture); //BL-BR
            GUI.DrawTexture(new Rect(SelectionBox.x, SelectionBox.y, SelectionBorderWidth, SelectionBox.height), SelectionTexture); //TL-BL
        }
    }
}
