using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using VektorLibrary.EntityFramework.Components;
using InvincibleEngine.UnitFramework.Components;
using InvincibleEngine;
using InvincibleEngine.Managers;

namespace InvincibleEngine {
    /// <summary>
    /// The Game manager is in charge of issuing commands, scraping data for network, loading levels, and most other game functions
    /// </summary>
    [Serializable]
    public class BuildOption {
        public int Cost;
        public Texture Preview;
        public GameObject PrefabSpawn;
    }
    public enum ResourceType {
        Mass,
        Energy
    }
    public class MatchManager : MonoBehaviour {

        //Scruct for stats about games
        public struct GameStats {
            public int Winner;
            public int[] Scores;
        }


        public object[] BuildOptions;

        //Stats about the game that just occured
        public GameStats PreviousGameStats = new GameStats();

        //match state
        public bool MatchStarted = false;
        public bool MatchHost = false;
        private bool Building = false;

        private GameObject PreviewObject;

        //stores all selected entities
        public List<UnitBehavior> SelectedEntities = new List<UnitBehavior>();

        // Singleton Instance Accessor
        public static MatchManager Instance { get; private set; }

        // Preload Method
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Preload() {
            //Make sure the Managers object exists
            GameObject Managers = GameObject.Find("Managers") ?? new GameObject("Managers");

            // Ensure this singleton initializes at startup
            if (Instance == null) Instance = Managers.GetComponent<MatchManager>() ?? Managers.AddComponent<MatchManager>();

            // Ensure this singleton does not get destroyed on scene load
            DontDestroyOnLoad(Instance.gameObject);
        }

        /// <summary>
        /// On awake load all resources
        /// </summary>
        private void Awake() {
            BuildOptions = Resources.LoadAll("Structures", typeof(GameObject));
        }

        private void Update() {

            //Preview Structure placement
            RaycastHit hit = new RaycastHit();
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Physics.Raycast(ray, out hit);
            if (Building) {
                PreviewObject.transform.position = new Vector3(Mathf.Round(hit.point.x / 2) * 2,
                                        hit.point.y,
                                        Mathf.Round(hit.point.z / 2) * 2);

                //player attemptes to build a structure
                if (Input.GetMouseButtonDown(0)) {

                }
               
                //player cancels build
                if (Input.GetMouseButtonDown(1)) {
                    GameObject.Destroy(PreviewObject);
                    Building = false;
                }
            }
        }

        /// <summary>
        /// Called when the player wants to build a structure
        /// display a preview of the structure 
        /// </summary>
        public void OnPlayerBuild(int index) {
           

        }

        /// <summary>
        /// Called when a player tries to build a structure
        /// </summary>
        /// <param name="structure"></param>
        /// <param name="location"></param>
        public void BuildStructure(BuildOption structure, Vector3 location, LobbyMember member) {

            //if we are hosting see if the player can build structure
            if(NetManager.Hosting) {

            }

            //if we are client just ask host to build
            else {

            }
        }

        /// <summary>
        /// Called by objects to generate resource
        /// </summary>
        public void OnGenerateResource(float resources, float energy) {
            if (!MatchStarted) { return; }
            try {
                NetManager.Instance.LocalPlayer.Resources += (resources * Time.deltaTime);
            }
            catch {
                Debug.Log("Player Not found");
            }
        }

        /// <summary>
        /// Called when the player selects objects on the screen
        /// </summary>
        /// <param name="Selection">Rectangle of selection box</param>
        public void OnSelect(Rect Selection) {

            //deselect all objects if the user isnt holding shift
            foreach (UnitBehavior x in SelectedEntities) {
                x.OnDeselected();
            }

            //Clear all selected units for new selection
            SelectedEntities.Clear();

            //iterate over all objects to see if they are selected
            object[] objects = GameObject.FindGameObjectsWithTag("Entity");
            foreach (GameObject n in objects) {
                Vector2 screenPoint = Camera.main.WorldToScreenPoint(n.transform.position);
                screenPoint.y = Screen.height - screenPoint.y;
                try {
                    if (Selection.Contains(screenPoint, true)) {
                        UnitBehavior u = n.GetComponent<UnitBehavior>();
                        u.OnSelected();
                        SelectedEntities.Add(u);
                    }
                }
                catch (NullReferenceException) {
                    Debug.Log($"Entity {n} does not have a unit behavior attatched!");
                }
            }
        }

        /// <summary>
        /// Starts match, loads into desired scene with parameters
        /// </summary>
        public void StartMatch(bool isHost, int level) {

            //load scene, set parameters
            SceneManager.LoadScene(level);
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

        public void OnLevelWasLoaded(int level) {
            if(level !=0) {
                MatchStarted = true;

            }
        }
    }
}
