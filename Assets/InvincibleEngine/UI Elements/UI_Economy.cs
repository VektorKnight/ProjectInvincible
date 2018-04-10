using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using InvincibleEngine.Managers;

public class UI_Economy : MonoBehaviour {

    public Text text;
	
	// Update is called once per frame
	void Update () {
        text.text = "Resources: " + NetManager.Instance.LocalPlayer?.Resources.ToString("0") + " Energy: " + (NetManager.Instance.LocalPlayer.EnergyIn - NetManager.Instance.LocalPlayer.EnergyOut).ToString("0");
	}
}
