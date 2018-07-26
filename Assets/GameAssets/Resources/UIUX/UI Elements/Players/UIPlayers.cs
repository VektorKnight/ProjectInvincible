using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Linq;
using UnityEngine;
using SteamNet;
using _3rdParty.Steamworks.Plugins.Steamworks.NET.types.SteamClientPublic;

public class UIPlayers : MonoBehaviour {

    public Dictionary<CSteamID, UIPlayerSlot> DisplayedPlayers = new Dictionary<CSteamID, UIPlayerSlot>();
    public GameObject PlayerCardPrefab;

    // Update is called once per frame
    private void Start() {
        for(int i=0;i<transform.childCount; i++) {
            Destroy(transform.GetChild(i).gameObject);
        }
    }

    void Update () {

        //Sort list by color


        //if a player exists that doesnt have a card, add one
        foreach (KeyValuePair<CSteamID, SteamnetPlayer> n in SteamNetManager.Instance.CurrentlyJoinedLobby.LobbyMembers.OrderBy(o=>o.Value.team)) {

            //Player has no card, create one
            if(!DisplayedPlayers.ContainsKey(n.Key)) {
                UIPlayerSlot p = Instantiate(PlayerCardPrefab, transform).GetComponent<UIPlayerSlot>();
                p.CardID = n.Value.SteamID;
                p.Name = n.Value.DisplayName;

                DisplayedPlayers.Add(n.Value.SteamID, p);
            }
        }

        //Remove those that have left
        for(int i=0; i<DisplayedPlayers.Count; i++) { 
            if(!SteamNetManager.Instance.CurrentlyJoinedLobby.LobbyMembers.ContainsKey(DisplayedPlayers.ElementAt(i).Key)) {
                Destroy(DisplayedPlayers.ElementAt(i).Value.gameObject);
                DisplayedPlayers.Remove(DisplayedPlayers.ElementAt(i).Key);
            }
        }
    }
}
