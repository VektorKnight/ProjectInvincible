using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using SteamNet;

//Steam
using _3rdParty.Steamworks.Plugins.Steamworks.NET.types.SteamClientPublic;
using _3rdParty.Steamworks.Plugins.Steamworks.NET.autogen;
using _3rdParty.Steamworks.Plugins.Steamworks.NET;


public class UINavigation : MonoBehaviour {


    public class UIPanel {
        public RectTransform Panel;
        public float TargetDelta = 0;
    }

    public Text State;

    public List<UIPanel> Panels = new List<UIPanel>();


    // Use this for initialization
    void Start() {

    }

    // Update is called once per frame
    void Update() {
        switch (SteamManager.Instance.NetworkState) {
            case ENetworkState.Connected: {
                    State.text = "In Lobby".ToUpper();
                    break;
                }
            case ENetworkState.Hosting: {
                    State.text = "In Lobby (hosting)".ToUpper();
                    break;
                }
            case ENetworkState.Stopped: {
                    State.text = "Main Menu".ToUpper();
                    break;
                }
            default: { break; }
        }
    }

    public void Play() {
        SteamManager.Instance.CreateLobby();
    }

    public void LeaveLobby() {
        SteamManager.Instance.LeaveLobby();
    }
}
