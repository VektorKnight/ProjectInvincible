using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using SteamNet;
using InvincibleEngine;

public class UIActionPanel : MonoBehaviour {

    //Object displaying actions of
    private EntityBehavior _CurrentlySelectedObject;
    

	void Update () {

        //Check to see if the selected unit changed
        if (PlayerManager.Instance.SelectedEntities[0].name != _CurrentlySelectedObject.name) {

            //Set currently actioned entity
            _CurrentlySelectedObject = PlayerManager.Instance.SelectedEntities[0];
        }
		
	}
}
