using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using SteamNet;

public class UIChat : MonoBehaviour {

    public Text ChatBox;

	public void OnChat(InputField input) {

        //Compose Message
        N_CHT message = new N_CHT {
            message = input.text
        };

        //Send Message
        SteamNetManager.Instance.BroadcastChatMessage(message);

        //clear and refocus input field
        input.text = "";
        input.ActivateInputField();
    }
    private void Update() {

        ChatBox.text = SteamNetManager.Instance.CurrentlyJoinedLobby?.ChatLog;
    }
}
