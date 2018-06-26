using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using SteamNet;

public class UIChat : MonoBehaviour {

    public Text ChatBox;

	public void OnChat(Text input) {

        //Compose Message
        N_CHT message = new N_CHT {
            message = input.text
        };

        //Send Message
        SteamManager.Instance.BroadcastChatMessage(message);        
    }
}
