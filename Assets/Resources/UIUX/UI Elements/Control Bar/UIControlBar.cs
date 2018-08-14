using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SteamNet;

public class UIControlBar : MonoBehaviour {

    //Object Reference
    [SerializeField] private Text Resources,Energy;

	// Update is called once per frame
	void Update () {
        if(SteamNetManager.Instance.NetworkState!= ENetworkState.Stopped)
        Resources.text = SteamNetManager.LocalPlayer.Economy.Resources.ToString();
        Energy.text = SteamNetManager.LocalPlayer.Economy.Energy.ToString();

    }
}
