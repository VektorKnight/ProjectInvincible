using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SteamNet;

public class UIControlBar : MonoBehaviour {

    //Object Reference
    [SerializeField] private Text EconomyText;

	// Update is called once per frame
	void Update () {

        EconomyText.text = SteamNetManager.LocalPlayer.Economy.Resources.ToString();

	}
}
