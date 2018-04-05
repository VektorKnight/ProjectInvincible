using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using VektorLibrary.EntityFramework.Components;
using InvincibleEngine.UnitFramework.Components;
using InvincibleEngine;

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


        public List<BuildOption> BuildOptions = new List<BuildOption>();

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

     
        private void Update() {
            //Preview Structure placement
            RaycastHit hit = new RaycastHit();
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Physics.Raycast(ray, out hit);
            if (Building) {
                PreviewObject.transform.position = hit.point;
                if (Input.GetMouseButtonDown(0)) {

                }
                if (Input.GetMouseButtonUp(0)) {

                }
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
        public void OnPlayerBuild(BuildOption structure) {
            Building = true;

            //Generate preview object
            PreviewObject = Instantiate(structure.PrefabSpawn);
            foreach(var n in PreviewObject.GetComponentsInChildren<MonoBehaviour>(true)) {
                Destroy(n);
            }
        }

        /// <summary>
        /// Called by objects to generate resource
        /// </summary>
        public void OnGenerateResource(int resources, int energy) {

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
}
