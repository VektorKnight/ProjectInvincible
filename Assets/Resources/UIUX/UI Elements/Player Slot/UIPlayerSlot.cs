using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using SteamNet;
using _3rdParty.Steamworks.Plugins.Steamworks.NET.types.SteamClientPublic;
using _3rdParty.Steamworks.Plugins.Steamworks.NET.autogen;
using _3rdParty.Steamworks.Plugins.Steamworks.NET;
using InvincibleEngine.UnitFramework.Enums;

public class UIPlayerSlot : MonoBehaviour {
    [SerializeField]
    private Text _Name;

    [SerializeField]
    private Image _ColoredBackground;

    [SerializeField]
    private Image _TeamPickBlocker;
    
    public CSteamID CardID;
    public string Name {
        get { return _Name.text; }
        set { _Name.text = value.ToUpper(); }
    }

    private void Start() {

        //if this card belongs to the current user
        _TeamPickBlocker.gameObject.SetActive((SteamUser.GetSteamID() == CardID) ? false : true);

    }

    private void Update() {
        //Make sure the color of this slot matches it's player
        Color32 color = SteamNetManager.CurrentLobbyData.LobbyMembers[CardID].Team.EColor();
        _ColoredBackground.color = color;
        _TeamPickBlocker.color = color;
           
    }

    public void TeamChangeRequest(int team) {
        SteamNetManager.Instance.BroadcastChatMessage(new N_TMC(team));
    }
}
