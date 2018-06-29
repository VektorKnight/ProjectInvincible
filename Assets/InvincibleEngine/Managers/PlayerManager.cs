using System.Collections;
using System.Collections.Generic;
using InvincibleEngine;
using UnityEngine;

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

    private Rect SelectionBox = new Rect(0, 0, 0, 0);

    private Texture2D SelectionTexture;
    private int SelectionBorderWidth = 2;

    public Vector2 MousePotition = new Vector2();

    // Singleton Instance Accessor
    public static PlayerManager Instance { get; private set; }

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
    }

    private void Update() {
        //TODO: TEMPORARY SELECTION INDICATIONS

        //Set mouse position
        MousePotition = new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y);

        //Check and see if the player is trying to make a selection
        if (Input.GetMouseButtonDown(0)) {
            //Set selecting to true
            Selecting = true;

            //Set initial values of box
            SelectionBox = new Rect(MousePotition.x, MousePotition.y, 0, 0);
        }

        //As long as it's held make the rectangle 
        if (Input.GetMouseButton(0)) {
            SelectionBox.width = MousePotition.x - SelectionBox.x;
            SelectionBox.height = MousePotition.y - SelectionBox.y;
        }

        //On mouse up stop selecting
        if (Input.GetMouseButtonUp(0)) {
            Selecting = false;


            SelectionBox = new Rect(0, 0, 0, 0);
        }
    }

    //Called when the player attempts to build somthing
    public void OnBuildPreview(GameObject building) {

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
