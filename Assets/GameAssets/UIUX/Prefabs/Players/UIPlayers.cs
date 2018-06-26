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
	void Update () {

        //if a player exists that doesnt have a card, add one
        foreach (KeyValuePair<CSteamID, SteamnetPlayer> n in SteamManager.Instance.CurrentlyJoinedLobby.LobbyMembers) {

            //Player has no card, create one
            if(!DisplayedPlayers.ContainsKey(n.Key)) {
                UIPlayerSlot p = Instantiate(PlayerCardPrefab, transform).GetComponent<UIPlayerSlot>();
                p.CardID = n.Value.SteamID;
                p.Name = n.Value.DisplayName;

                DisplayedPlayers.Add(n.Value.SteamID, p);
            }
        }

        for(int i=0; i<DisplayedPlayers.Count; i++) { 
            if(!SteamManager.Instance.CurrentlyJoinedLobby.LobbyMembers.ContainsKey(DisplayedPlayers.ElementAt(i).Key)) {
                Destroy(DisplayedPlayers.ElementAt(i).Value.gameObject);
                DisplayedPlayers.Remove(DisplayedPlayers.ElementAt(i).Key);
            }
        }
    }
}
