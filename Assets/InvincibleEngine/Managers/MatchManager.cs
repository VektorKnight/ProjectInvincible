using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using VektorLibrary.EntityFramework.Components;
using InvincibleEngine.UnitFramework.Components;

/// <summary>
/// The Game manager is in charge of issuing commands, scraping data for network, loading levels, and most other game functions
/// </summary>
public class MatchManager : MonoBehaviour {

    //Scruct for stats about games
    public struct GameStats {
        public int Winner;
        public int[] Scores;
    }

    //Stats about the game that just occured
    public GameStats PreviousGameStats = new GameStats();

    //match state
    public bool MatchStarted = false;
    public bool MatchHost = false;

    //Selection variables
    private bool Selecting = false;

    private Rect SelectionBox = new Rect(0,0,0,0);

    private Texture2D SelectionTexture;
    private int SelectionBorderWidth = 2;

    public Vector2 MousePotition = new Vector2();

    //stores all selected entities
    public List<UnitBehavior> SelectedEntities = new List<UnitBehavior>();

    // Singleton Instance Accessor
    public static MatchManager Instance { get; private set; }
    
    // Preload Method
    [RuntimeInitializeOnLoadMethod]
    private static void Preload() {
        //Make sure the Managers object exists
        GameObject Managers = GameObject.Find("Managers") ?? new GameObject("Managers");       

        // Ensure this singleton initializes at startup
        if (Instance == null) Instance = Managers.AddComponent<MatchManager>();

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
    }

    /// <summary>
    /// 
    /// </summary>
    private void Update() {
        //TODO: TEMPORARY SELECTION INDICATIONS

        //Set mouse position
        MousePotition = new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y);
        
        //Check and see if the player is trying to make a selection
        if(Input.GetMouseButtonDown(0)) {

            //Set selecting to true
            Selecting = true;

            //Set initial values of box
            SelectionBox = new Rect(MousePotition.x, MousePotition.y, 0, 0);
        }

        //As long as it's held make the rectangle 
        if(Input.GetMouseButton(0)) {
            SelectionBox.width = MousePotition.x - SelectionBox.x;
            SelectionBox.height = MousePotition.y-SelectionBox.y;
        }

        //On mouse up stop selecting
        if(Input.GetMouseButtonUp(0)) {
            Selecting = false;
            OnSelect(SelectionBox);
            SelectionBox = new Rect(0, 0, 0, 0);
        }

    }

    /// <summary>
    /// 
    /// </summary>
    private void OnGUI() {

        //If player is selecting draw rectangle
        if (Selecting) {
            GUI.DrawTexture(new Rect(SelectionBox.x, SelectionBox.y, SelectionBox.width, SelectionBorderWidth), SelectionTexture); //TL-TR
            GUI.DrawTexture(new Rect(SelectionBox.x + SelectionBox.width, SelectionBox.y, SelectionBorderWidth, SelectionBox.height), SelectionTexture); //TR-BR
            GUI.DrawTexture(new Rect(SelectionBox.x,SelectionBox.y+SelectionBox.height,SelectionBox.width, SelectionBorderWidth), SelectionTexture); //BL-BR
            GUI.DrawTexture(new Rect(SelectionBox.x, SelectionBox.y, SelectionBorderWidth, SelectionBox.height), SelectionTexture); //TL-BL
        }
    }

    /// <summary>
    /// Called when the player selects objects on the screen
    /// </summary>
    /// <param name="Selection">Rectangle of selection box</param>
    private void OnSelect(Rect Selection) {

        //deselect all objects if the user isnt holding shift
        foreach(UnitBehavior x in SelectedEntities) {
            x.OnDeselected();
        }

        //iterate over all objects to see if they are selected
        Object[] objects = GameObject.FindGameObjectsWithTag("Entity");
        foreach(GameObject n in objects) {
            Vector2 screenPoint = Camera.main.WorldToScreenPoint(n.transform.position);
            screenPoint.y = Screen.height - screenPoint.y;
            if(Selection.Contains(screenPoint)) {
                UnitBehavior u = n.GetComponent<UnitBehavior>();
                u.OnSelected();
                SelectedEntities.Add(u);
            }
        }
    }

    /// <summary>
    /// Starts match, loads into desired scene with parameters
    /// </summary>
    public void StartMatch(bool isHost, int level) {

        //load scene, set parameters
        SceneManager.LoadScene(level);
        MatchStarted = true;
        MatchHost = isHost;
    }

    /// <summary>
    /// Ends game, returns to lobby scene with after game report
    /// </summary>
    public void EndMatch() {

        //load lobby scene, always scene 0
        MatchStarted = false;
        SceneManager.LoadScene(0);
    }
}
