using System.Collections;
using System.Collections.Generic;
using _3rdParty.Steamworks.Plugins.Steamworks.NET.types.SteamClientPublic;
using SteamNet;
using UnityEngine;

public class UIFriends : UIBehavior {

    //Prefabs
    public UIFriendPanel FriendPanelPrefab;

    //List of lobbies displayed
    Dictionary<CSteamID, UIFriendPanel> CurrentlyDisplayedLobbies = new Dictionary<CSteamID, UIFriendPanel>();

    public override void OnOnlineLobbyUpdate(bool added, LobbyData changed, CSteamID id) {
        base.OnOnlineLobbyUpdate(added, changed, id);

        //if added
        if(added) {

            //instantiate and store new lobby
            UIFriendPanel n = (Instantiate(FriendPanelPrefab, transform) as UIFriendPanel);

            //set lobby data
            n.SetData(changed, id);

            //add to list
            CurrentlyDisplayedLobbies.Add(id, n);

        }

        //If removed
        if(!added) {

            //destroy object and remove from list
            Destroy(CurrentlyDisplayedLobbies[id].gameObject);
            CurrentlyDisplayedLobbies.Remove(id);
          
        }
        
    }

}
