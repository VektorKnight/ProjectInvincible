using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SteamNet;

public class UILobbyBlocker : MonoBehaviour {

    //Objects
    [SerializeField] private GameObject BlockingPanel;

	// Update is called once per frame
	void Update () {
        BlockingPanel.SetActive((SteamNetManager.Instance.Hosting | SteamNetManager.Instance.Connected) ? false : true);
    }
}
