using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MapData : MonoBehaviour {

    [Header("Asset Links")]
    [SerializeField] public Sprite Splash;
    [SerializeField] public Scene Scene;
    [SerializeField] public GameObject[] SpawnPoints;

    [Header("Map Properties")]
    [SerializeField] public string MapName = "";
    [SerializeField] public bool InDev = false;
    [SerializeField] public int MaxPlayers { get { return SpawnPoints.Length; } }
    
}
