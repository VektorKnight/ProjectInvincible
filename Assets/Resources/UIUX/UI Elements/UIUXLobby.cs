using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIUXLobby : UIBehavior {

    //Private: List of all lobby elements
    [SerializeField] private RectTransform MainLobby;

    //Match Started
    public override void OnMatchStart() {
        base.OnMatchStart();
    }
    
    //Lobby Joined
    public override void OnJoinLobby() {
        base.OnJoinLobby();

        //Show Main Lobby
        MainLobby.gameObject.SetActive(true);
    }
}
