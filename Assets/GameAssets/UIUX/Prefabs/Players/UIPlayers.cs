using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using SteamNet;
using _3rdParty.Steamworks.Plugins.Steamworks.NET.types.SteamClientPublic;

public class UIPlayers : MonoBehaviour {

    public Dictionary<CSteamID, UIPlayerSlot> DisplayedPlayers = new Dictionary<CSteamID, UIPlayerSlot>();
    public GameObject PlayerCardPrefab; 

	// Update is called once per frame
	void Update () {

        //if a player exists that doesnt have a card, add one
        foreach (SteamnetPlayer n in SteamManager.Instance.LobbyData.LobbyMembers) {

            //Player has no card, create one
            if(!DisplayedPlayers.ContainsKey(n.SteamID)) {
                UIPlayerSlot p = Instantiate(PlayerCardPrefab, transform).GetComponent<UIPlayerSlot>();
                p.CardID = n.SteamID;
                p.Name = n.DisplayName;

                DisplayedPlayers.Add(n.SteamID, p);
            }
        }
    }
}
