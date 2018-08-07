using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SteamNet;

/// <summary>
/// Attached to any entity objects that are to be synced over clients
/// </summary>
public class NetworkEntity : MonoBehaviour {

    //Unique ID  for tracking
    public ushort NetID;
    public ushort AssetID;

    
	// Use this for initialization
	void Awake () {


	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
